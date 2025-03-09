#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class AETDataDictionary : SerializableDictionary<MonoBehaviour, AETData>
    {
    }

    [Serializable]
    public class AETData : ScriptableObject
    {
        public bool baseInspectorFoldout;

        // [SerializeReference]
        // public List<SerializedField> fields = new();
        [SerializeReference] public List<LayoutInfo> layoutInfos = new();

        [SerializeReference] public List<ButtonInfo> buttonMethods = new();
    }
}
#endif