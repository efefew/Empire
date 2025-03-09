#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace AdvancedEditorTools
{
#if UNITY_EDITOR
    [Serializable]
    [ExecuteInEditMode]
    public class AETManager : MonoBehaviour
    {
        private static AETManager instance;


        [SerializeField] private AETDataDictionary dataDict;

        public bool extensionEnabled = true;
        public bool debugMode;


        private Dictionary<string, TypeMethods> _typesMethodsDictionary;

        public static AETManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AETManager>();
                    if (instance == null)
                    {
                        GameObject go = new("_AEAManager");
                        go.tag = "EditorOnly";
                        instance = go.AddComponent<AETManager>();
                    }
                }

                return instance;
            }
        }

        public Dictionary<string, TypeMethods> TypesMethodsDictionary
        {
            get
            {
                if (_typesMethodsDictionary == null)
                    _typesMethodsDictionary = DefaultSerializableTypes.GetTypeMethodsDictionary();
                return _typesMethodsDictionary;
            }
        }

        private void Awake()
        {
            if (Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            gameObject.hideFlags = debugMode ? HideFlags.NotEditable : HideFlags.HideInHierarchy;

            if (dataDict == null)
            {
                dataDict = new AETDataDictionary();
            }
            else
            {
                AETDataDictionary newDict = new();

                foreach (var kvp in dataDict)
                    if (kvp.Key != null)
                        newDict[kvp.Key] = kvp.Value;

                dataDict = newDict;
            }
        }

        public AETData RetrieveDataOrCreate(MonoBehaviour target)
        {
            dataDict ??= new AETDataDictionary();

            if (!dataDict.TryGetValue(target, out AETData data))
            {
                data = ScriptableObject.CreateInstance<AETData>();
                dataDict.Add(target, data);
            }

            return data;
        }

        public TypeMethods RetrieveTypeMethods(Type type)
        {
            var typesMethodsDict = TypesMethodsDictionary;
            if (typesMethodsDict.TryGetValue(type.Name, out TypeMethods typeMethods))
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
                    return typesMethodsDict["Array"];
                }

                if (HasEmptyConstructor(type))
                    return typesMethodsDict["Class"];
            }

            throw new KeyNotFoundException($"The type provided type '{type.FullName}' could not be serialized.");
        }

        public bool IsTypeImplemented(Type type)
        {
            var typesMethodsDict = TypesMethodsDictionary;
            return typesMethodsDict.ContainsKey(type.Name) || type.IsEnum || type.IsSubclassOf(typeof(Object)) ||
                   typeof(IList).IsAssignableFrom(type);
        }

        private static bool HasEmptyConstructor(Type type)
        {
            var constructors = type.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
                if (constructor.GetParameters().Length == 0)
                    return true;
            return false;
        }

        [MenuItem("Window/Advanced Editor Tools/Toggle Extension ON\\OFF", priority = 1000)]
        public static void ToggleExtension()
        {
            AETManager instance = Instance;
            instance.extensionEnabled = !instance.extensionEnabled;
            Debug.Log($"<i>Advanced Editor Tools</i>: <b>{(instance.extensionEnabled ? "Enabled" : "Disabled")}</b>");
        }

        [MenuItem("Window/Advanced Editor Tools/Toggle Debug Mode", priority = 1011)]
        public static void ToggleDebugMode()
        {
            AETManager instance = Instance;
            instance.debugMode = !instance.debugMode;
            instance.gameObject.hideFlags = instance.debugMode ? HideFlags.NotEditable : HideFlags.HideInHierarchy;
            Debug.Log($"<i>Advanced Editor Tools</i>: Debug mode set to <b>{instance.debugMode}</b>");
        }

        [MenuItem("Window/Advanced Editor Tools/Reload", priority = 1012)]
        public static void ReloadAdvancedEditor()
        {
            if (EditorUtility.DisplayDialog(
                    "Reload Advanced Editor Tools extension",
                    "WARNING: Reloading the extension will reset all your saved preferences of each GameObject, like the state of each " +
                    "foldout or the parameters of your button methods.",
                    "Reload",
                    "Cancel"
                ))
            {
                bool debugMode = Instance.debugMode;
                DestroyImmediate(Instance.gameObject);
                Selection.activeGameObject = null;
                if (debugMode)
                    ToggleDebugMode();
            }
        }
    }
#endif
}