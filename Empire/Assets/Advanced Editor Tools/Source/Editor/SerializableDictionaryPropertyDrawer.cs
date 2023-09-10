using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [CustomPropertyDrawer(typeof(AdvancedEditorTools.AETDataDictionary))]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prevStatus = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = prevStatus;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}
