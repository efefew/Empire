#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class ObjectValueWrapper : GenericValueWrapperReference<Object>
    {
        public override void OnInspector(Rect rect, string label)
        {
            var serializedField = SerializedField;
            EditorGUI.ObjectField(rect, serializedField.SerializedProperty, serializedField.Type, new GUIContent(label));
            if (serializedField.SerializedObject.hasModifiedProperties)
                serializedField.SerializedObject.ApplyModifiedProperties();
        }
    }
}
#endif