#if UNITY_EDITOR

#region

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class ObjectValueWrapper : GenericValueWrapperReference<Object>
    {
        public override void OnInspector(Rect rect, string label)
        {
            SerializedField serializedField = SerializedField;
            EditorGUI.ObjectField(rect, serializedField.SerializedProperty, serializedField.Type,
                new GUIContent(label));
            if (serializedField.SerializedObject.hasModifiedProperties)
                serializedField.SerializedObject.ApplyModifiedProperties();
        }
    }
}
#endif