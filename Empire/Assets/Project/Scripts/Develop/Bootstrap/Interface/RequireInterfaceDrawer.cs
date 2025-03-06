#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

[CustomPropertyDrawer(typeof(InterfaceAttribute))]
public class RequireInterfaceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InterfaceAttribute requiredAttribute = (InterfaceAttribute)attribute;
        System.Type interfaceType = requiredAttribute.InterfaceType;

        // Отрисовка поля для MonoBehavior
        EditorGUI.BeginProperty(position, label, property);

        UnityEngine.Object obj = EditorGUI.ObjectField(
            position,
            label,
            property.objectReferenceValue,
            typeof(MonoBehaviour),
            true
        );

        // Проверка, реализует ли компонент интерфейс
        if (obj != null)
        {
            MonoBehaviour monoBehavior = (MonoBehaviour)obj;
            if (!interfaceType.IsAssignableFrom(monoBehavior.GetType()))
            {
                obj = null;
                Debug.LogError($"Компонент должен реализовывать {interfaceType.Name}!");
            }
        }

        property.objectReferenceValue = obj;
        EditorGUI.EndProperty();
    }
}
#endif