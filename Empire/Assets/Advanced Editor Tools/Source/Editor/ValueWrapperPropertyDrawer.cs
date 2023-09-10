using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    // [CustomPropertyDrawer(typeof(ValueWrapper))]
    public class ValueWrapperPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.objectReferenceValue == null)
                return;
            var targetWrapper = new SerializedObject(property.objectReferenceValue);
            var childProp = targetWrapper.FindProperty("value");

            if (childProp != null)
            {
                EditorGUI.PropertyField(EditorGUI.IndentedRect(position), childProp, true);
                childProp.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property != null && property.objectReferenceValue != null)
            {
                var targetWrapper = new SerializedObject(property.objectReferenceValue);
                var childProp = targetWrapper.FindProperty("value");

                if (childProp != null)
                {
                    return EditorGUI.GetPropertyHeight(childProp, label, true);
                }
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}
