#if UNITY_EDITOR

#region

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

[CustomPropertyDrawer(typeof(InterfaceAttribute))]
public class RequireInterfaceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InterfaceAttribute requiredAttribute = (InterfaceAttribute)attribute;
        Type interfaceType = requiredAttribute.InterfaceType;

        // ��������� ���� ��� MonoBehavior
        EditorGUI.BeginProperty(position, label, property);

        Object obj = EditorGUI.ObjectField(
            position,
            label,
            property.objectReferenceValue,
            typeof(MonoBehaviour),
            true
        );

        // ��������, ��������� �� ��������� ���������
        if (obj != null)
        {
            MonoBehaviour monoBehavior = (MonoBehaviour)obj;
            if (!interfaceType.IsAssignableFrom(monoBehavior.GetType()))
            {
                obj = null;
                Debug.LogError($"��������� ������ ������������� {interfaceType.Name}!");
            }
        }

        property.objectReferenceValue = obj;
        EditorGUI.EndProperty();
    }
}
#endif