using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
#if UNITY_EDITOR
    [System.Serializable]
    [ExecuteInEditMode]
    public class AETManager : MonoBehaviour
    {
        private static AETManager instance;
        public static AETManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<AETManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("_AEAManager");
                        go.tag = "EditorOnly";
                        instance = go.AddComponent<AETManager>();
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            if (Instance != this)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            this.gameObject.hideFlags = debugMode ? HideFlags.NotEditable : HideFlags.HideInHierarchy;

            if (this.dataDict == null)
            {
                this.dataDict = new();
            }
            else
            {
                AETDataDictionary newDict = new();

                foreach (var kvp in this.dataDict)
                {
                    if (kvp.Key != null)
                        newDict[kvp.Key] = kvp.Value;
                }

                dataDict = newDict;
            }
        }


        [SerializeField]
        private AETDataDictionary dataDict;

        public bool extensionEnabled = true;
        public bool debugMode = false;

        public AETData RetrieveDataOrCreate(MonoBehaviour target)
        {
            dataDict ??= new();

            if (!dataDict.TryGetValue(target, out var data))
            {
                data = ScriptableObject.CreateInstance<AETData>();
                dataDict.Add(target, data);
            }

            return data;
        }


        private Dictionary<string, TypeMethods> _typesMethodsDictionary;
        public Dictionary<string, TypeMethods> TypesMethodsDictionary
        {
            get
            {
                if (_typesMethodsDictionary == null)
                    _typesMethodsDictionary = DefaultSerializableTypes.GetTypeMethodsDictionary();
                return _typesMethodsDictionary;
            }
        }

        public TypeMethods RetrieveTypeMethods(System.Type type)
        {
            var typesMethodsDict = TypesMethodsDictionary;
            if (typesMethodsDict.TryGetValue(type.Name, out var typeMethods))
                return typeMethods;

            if (type.IsEnum)
                return typesMethodsDict["Enum"];
            if (type.IsValueType)
                return typesMethodsDict["Struct"];
            if (type.IsClass)
            {
                if (type.IsSubclassOf(typeof(Object)))
                    return typesMethodsDict["Object"];
                if (typeof(IList).IsAssignableFrom(type))
                {
                    if (type.IsGenericType)
                        return typesMethodsDict["List"];
                    else
                        return typesMethodsDict["Array"];
                }
                if (HasEmptyConstructor(type))
                    return typesMethodsDict["Class"];
            }

            throw new KeyNotFoundException($"The type provided type '{type.FullName}' could not be serialized.");
        }

        public bool IsTypeImplemented(System.Type type)
        {
            var typesMethodsDict = TypesMethodsDictionary;
            return typesMethodsDict.ContainsKey(type.Name) || type.IsEnum || type.IsSubclassOf(typeof(Object)) || typeof(IList).IsAssignableFrom(type);
        }

        private static bool HasEmptyConstructor(System.Type type)
        {
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                    return true;
            }
            return false;
        }

        [MenuItem("Window/Advanced Editor Tools/Toggle Extension ON\\OFF", priority = 1000)]
        public static void ToggleExtension()
        {
            var instance = Instance;
            instance.extensionEnabled = !instance.extensionEnabled;
            Debug.Log($"<i>Advanced Editor Tools</i>: <b>{(instance.extensionEnabled ? "Enabled" : "Disabled")}</b>");
        }

        [MenuItem("Window/Advanced Editor Tools/Toggle Debug Mode", priority = 1011)]
        public static void ToggleDebugMode()
        {
            var instance = Instance;
            instance.debugMode = !instance.debugMode;
            instance.gameObject.hideFlags = instance.debugMode ? HideFlags.NotEditable : HideFlags.HideInHierarchy;
            Debug.Log($"<i>Advanced Editor Tools</i>: Debug mode set to <b>{instance.debugMode}</b>");
        }

        [MenuItem("Window/Advanced Editor Tools/Reload", priority = 1012)]
        public static void ReloadAdvancedEditor()
        {
            if (EditorUtility.DisplayDialog(
                title: "Reload Advanced Editor Tools extension",
                message: "WARNING: Reloading the extension will reset all your saved preferences of each GameObject, like the state of each " +
                        "foldout or the parameters of your button methods.",
                ok: "Reload",
                cancel: "Cancel"
                ))
            {
                var debugMode = Instance.debugMode;
                DestroyImmediate(Instance.gameObject);
                Selection.activeGameObject = null;
                if (debugMode)
                    ToggleDebugMode();
            }
        }
    }
#endif
}
