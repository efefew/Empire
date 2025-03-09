using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Meryel.UnityCodeAssist.Editor.Input;
using Meryel.UnityCodeAssist.Editor.Preferences;
using Serilog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    [InitializeOnLoad]
    public static class Monitor
    {
        private static readonly string tagManagerFilePath;
        private static DateTime previousTagManagerLastWrite;

        private static bool isAppFocused;
        private static bool isAppFocusedOnTagManager;

        private static int dirtyCounter;
        private static readonly Dictionary<GameObject, int> dirtyDict;

        static Monitor()
        {
            tagManagerFilePath = CommonTools.GetTagManagerFilePath();
            previousTagManagerLastWrite = File.GetLastWriteTime(tagManagerFilePath);

            dirtyDict = new Dictionary<GameObject, int>();
            dirtyCounter = 0;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnUpdate;
            Undo.postprocessModifications += MyPostprocessModificationsCallback;
            Undo.undoRedoPerformed += MyUndoCallback;
            Selection.selectionChanged += OnSelectionChanged;
            //EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += EditorSceneManager_activeSceneChangedInEditMode;

            Application.logMessageReceived += Application_logMessageReceived;
            //System.Threading.Tasks.TaskScheduler.UnobservedTaskException += 
        }

        private static void EditorSceneManager_activeSceneChangedInEditMode(Scene arg0, Scene arg1)
        {
            //Debug.Log("EditorSceneManager_activeSceneChangedInEditMode");
            OnHierarchyChanged();
        }

        private static void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
        {
            Log.Debug("Monitor {Event} scene:{Scene} mode:{Mode}", nameof(EditorSceneManager_sceneOpened), scene.name,
                mode);
            //Debug.Log("EditorSceneManager_sceneOpened");
            OnHierarchyChanged();
        }

        private static void OnUpdate()
        {
            string? currentEditorFocus = null;
            if (Selection.activeObject)
                currentEditorFocus = Selection.activeObject.GetType().ToString();

            DateTime currentTagManagerLastWrite = File.GetLastWriteTime(tagManagerFilePath);
            if (currentTagManagerLastWrite != previousTagManagerLastWrite)
            {
                previousTagManagerLastWrite = currentTagManagerLastWrite;
                OnTagsOrLayersModified();
            }
            else if (currentEditorFocus == "UnityEditor.TagManager")
            {
                // since unity does not commit changes to the file immediately, checking if user is displaying and focusing on tag manager (tags & layers) inspector
                isAppFocusedOnTagManager = true;
            }


            if (isAppFocused != InternalEditorUtility.isApplicationActive)
            {
                isAppFocused = InternalEditorUtility.isApplicationActive;
                OnOnUnityEditorFocusChanged(isAppFocused);
                Log.Debug("On focus {State}", isAppFocused);
            }
        }

        private static void OnTagsOrLayersModified()
        {
            Log.Debug("Monitor {Event}", nameof(OnTagsOrLayersModified));

            Assister.SendTagsAndLayers();
        }

        private static void OnHierarchyChanged()
        {
            Log.Debug("Monitor {Event}", nameof(OnHierarchyChanged));

            // For requesting active doc's GO
            NetMQInitializer.Publisher?.SendHandshake();

            if (ScriptFinder.GetActiveGameObject(out GameObject? activeGO))
                NetMQInitializer.Publisher?.SendGameObject(activeGO);
            //Assister.SendTagsAndLayers(); Don't send tags & layers here
        }

        private static UndoPropertyModification[] MyPostprocessModificationsCallback(
            UndoPropertyModification[] modifications)
        {
            Log.Debug("Monitor {Event}", nameof(MyPostprocessModificationsCallback));

            foreach (UndoPropertyModification modification in modifications)
            {
                Object? target = modification.currentValue?.target;
                SetDirty(target);
            }

            // here, you can perform processing of the recorded modifications before returning them
            return modifications;
        }

        private static void MyUndoCallback()
        {
            Log.Debug("Monitor {Event}", nameof(MyUndoCallback));
            // code for the action to take on Undo
        }

        private static void OnOnUnityEditorFocusChanged(bool isFocused)
        {
            if (!isFocused)
            {
                if (isAppFocusedOnTagManager)
                {
                    isAppFocusedOnTagManager = false;
                    OnTagsOrLayersModified();
                }

                OnSelectionChanged();
                FlushAllDirty();
                /*
                Serilog.Log.Debug("exporting {Count} objects", selectedObjects.Count);

                //**--if too many
                foreach (var obj in selectedObjects)
                {
                    if (obj is GameObject go)
                        NetMQInitializer.Publisher.SendGameObject(go);
                }

                selectedObjects.Clear();
                */
            }
        }

        private static void OnSelectionChanged()
        {
            //**--check order, last selected should be sent last as well
            //**--limit here, what if too many?
            //selectedObjects.UnionWith(Selection.objects);
            foreach (Object? so in Selection.objects) SetDirty(so);
        }

        public static void SetDirty(Object? obj)
        {
            if (obj == null)
                return;
            if (obj is GameObject go)
                SetDirty(go);
            else if (obj is Component component)
                SetDirty(component.gameObject);
            //else
            //;//**--scriptable obj
        }

        public static void SetDirty(GameObject go)
        {
            dirtyCounter++;
            dirtyDict[go] = dirtyCounter;
        }

        private static void FlushAllDirty()
        {
            // Sending order is important, must send them in the same order as they are added to/modified in the collection
            // Using dict instead of hashset because of that. Dict value is used as add/modify order

            var sortedDict = from entry in dirtyDict orderby entry.Value descending select entry;

            foreach (var entry in sortedDict)
            {
                GameObject? go = entry.Key;
                NetMQInitializer.Publisher?.SendGameObject(go);
            }

            dirtyDict.Clear();
            dirtyCounter = 0;
        }


        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Warning)
                return;

            if (!stackTrace.Contains("Meryel.UnityCodeAssist.Editor"))
                return;

            string? typeStr = type.ToString();

            NetMQInitializer.Publisher?.SendErrorReport(condition, stackTrace, typeStr);
        }


        public static void LazyLoad(string category)
        {
            if (category == "PlayerPrefs")
                PreferenceMonitor.InstanceOfPlayerPrefs.Bump();
            else if (category == "EditorPrefs")
                PreferenceMonitor.InstanceOfEditorPrefs.Bump();
            else if (category == "InputManager")
                InputManagerMonitor.Instance.Bump();
            else
                Log.Error("Invalid LazyLoad category {Category}", category);
        }
    }
}