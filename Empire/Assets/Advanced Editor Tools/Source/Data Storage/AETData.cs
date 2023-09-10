#if UNITY_EDITOR
using AdvancedEditorTools.DataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class AETDataDictionary : SerializableDictionary<MonoBehaviour, AETData> { }

    [System.Serializable]
    public class AETData : ScriptableObject
    {
        public bool baseInspectorFoldout = false;
        // [SerializeReference]
        // public List<SerializedField> fields = new();
        [SerializeReference]
        public List<LayoutInfo> layoutInfos = new();
        [SerializeReference]
        public List<ButtonInfo> buttonMethods = new();
    }
}
#endif