﻿//
// Editor GUI Helper
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OmniSARTechnologies.Helper
{
    public static class EditorGUIHelper
    {
        public static class Attributes
        {
            public static PropertyAttribute[] GetFieldAttributes<PropertyAttribute>(FieldInfo field,
                bool inherit = true)
            {
                if (null == field) return default;

                return field.GetCustomAttributes(typeof(PropertyAttribute), inherit) as PropertyAttribute[];
            }

            public static PropertyAttribute GetFieldFirstAttribute<PropertyAttribute>(FieldInfo field,
                bool inherit = true)
            {
                if (null == field) return default;

                var attributes = GetFieldAttributes<PropertyAttribute>(field, inherit);

                if (attributes.Length < 1) return default;

                return attributes[0];
            }

            public static PropertyAttribute[] GetSerializedPropertyAttributes<Type, PropertyAttribute>(
                SerializedProperty serializedProperty, bool inherit = true)
            {
                if (null == serializedProperty) return default;

                FieldInfo field = typeof(Type).GetField(serializedProperty.name);

                if (null == field) return default;

                return GetFieldAttributes<PropertyAttribute>(field, inherit);
            }

            public static PropertyAttribute GetSerializedPropertyFirstAttribute<Type, PropertyAttribute>(
                SerializedProperty serializedProperty, bool inherit = true)
            {
                if (null == serializedProperty) return default;

                var attributes = GetSerializedPropertyAttributes<Type, PropertyAttribute>(serializedProperty, inherit);

                if (default(PropertyAttribute[]) == attributes) return default;


                if (attributes.Length < 1) return default;

                return attributes[0];
            }

            public static string GetFieldDisplayName(FieldInfo field, bool inherit = true)
            {
                if (null == field) return default;

                DisplayNameAttribute attribute = GetFieldFirstAttribute<DisplayNameAttribute>(field, inherit);

                if (null == attribute) return default;

                return attribute.displayName;
            }

            public static string GetSerializedPropertyDisplayName<Type>(SerializedProperty serializedProperty,
                bool inherit = true)
            {
                if (null == serializedProperty) return default;

                FieldInfo field = typeof(Type).GetField(serializedProperty.name);

                if (null == field) return default;

                return GetFieldDisplayName(field, inherit);
            }

            public static string GetFieldTooltip(FieldInfo field, bool inherit = true)
            {
                if (null == field) return default;

                TooltipAttribute attribute = GetFieldFirstAttribute<TooltipAttribute>(field, inherit);

                if (null == attribute) return default;

                return attribute.tooltip;
            }

            public static string GetSerializedPropertyTooltip<Type>(SerializedProperty serializedProperty,
                bool inherit = true)
            {
                if (null == serializedProperty) return default;

                FieldInfo field = typeof(Type).GetField(serializedProperty.name);

                if (null == field) return default;

                return GetFieldTooltip(field, inherit);
            }

            public static Vector2 GetFieldFloatRange(FieldInfo field, bool inherit = true)
            {
                if (null == field) return default;

                RangeAttribute attribute = GetFieldFirstAttribute<RangeAttribute>(field, inherit);

                if (null == attribute) return default;

                return new Vector2(attribute.min, attribute.max);
            }

            public static Vector2 GetSerializedPropertyFloatRange<Type>(SerializedProperty serializedProperty,
                bool inherit = true)
            {
                if (null == serializedProperty) return default;

                FieldInfo field = typeof(Type).GetField(serializedProperty.name);

                if (null == field) return default;

                return GetFieldFloatRange(field, inherit);
            }
        }

        public static class Styles
        {
            public static GUIStyle boldFoldout = new(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
        }

        public static class GUIContentHelper
        {
            public static GUIContent GetSerializedPropertyGUIContent<Type>(SerializedProperty serializedProperty,
                string displayName = default)
            {
                if (default == displayName)
                {
                    string attributeDisplayName = Attributes.GetSerializedPropertyDisplayName<Type>(serializedProperty);
                    displayName = default == attributeDisplayName
                        ? serializedProperty.displayName
                        : attributeDisplayName;
                }

                string tooltip = Attributes.GetSerializedPropertyTooltip<Type>(serializedProperty);
                return new GUIContent(displayName, tooltip);
            }
        }

        public static class Drawing
        {
            public static GUIStyle MakeLabelGUIStyle(FontStyle fontStyle, int fontSizeIncrement)
            {
                GUIStyle guiStyle = new();

                guiStyle.font = GUI.skin.font;
                guiStyle.fontStyle = fontStyle;
                guiStyle.fontSize = GUI.skin.font.fontSize + fontSizeIncrement;

                return guiStyle;
            }

            public static void DrawLabel(GUIContent content, Vector2 position, Vector2 pivot = default,
                Color fontColor = default, FontStyle fontStyle = default, bool dropShadow = true)
            {
                GUIStyle labelStyle = new();
                labelStyle.normal.textColor = fontColor;
                labelStyle.font = GUI.skin.font;
                labelStyle.fontStyle = fontStyle;

                GUIContent textContent = content;
                Vector2 size = labelStyle.CalcSize(textContent);
                pivot.Scale(size);
                position -= pivot;

                if (dropShadow)
                    EditorGUI.DropShadowLabel(new Rect(position, size), textContent, labelStyle);
                else
                    EditorGUI.LabelField(new Rect(position, size), textContent, labelStyle);
            }

            public static void DrawPreviewLabel(GUIContent content, Vector2 position, Color fontColor)
            {
                DrawLabel(
                    content,
                    position,
                    fontColor: fontColor,
                    fontStyle: FontStyle.Bold,
                    dropShadow: true
                );
            }

            public static bool DrawMultiValueEnumPopup<Type>(SerializedProperty serializedProperty,
                string displayName = default, bool enumItemsTooltips = true)
            {
                GUIContent label =
                    GUIContentHelper.GetSerializedPropertyGUIContent<Type>(serializedProperty, displayName);

                var names = new GUIContent[serializedProperty.enumDisplayNames.Length];
                for (int i = 0; i < names.Length; i++)
                    names[i] = new GUIContent(
                        serializedProperty.enumDisplayNames[i],
                        enumItemsTooltips ? serializedProperty.enumDisplayNames[i] + " " + displayName : default
                    );

                serializedProperty.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                int newValue = serializedProperty.enumValueIndex;

                if (serializedProperty.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                    newValue = EditorGUILayout.Popup(label, 0, names);
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    newValue = EditorGUILayout.Popup(label, newValue, names);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.enumValueIndex = newValue;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }

                serializedProperty.serializedObject.Update();
                return !serializedProperty.hasMultipleDifferentValues;
            }

            public static bool DrawMultiValueColorField<Type>(SerializedProperty serializedProperty,
                string displayName = default)
            {
                GUIContent label =
                    GUIContentHelper.GetSerializedPropertyGUIContent<Type>(serializedProperty, displayName);

                serializedProperty.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                Color newValue = serializedProperty.colorValue;

                if (serializedProperty.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                    newValue = EditorGUILayout.ColorField(label, default);
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    newValue = EditorGUILayout.ColorField(label, newValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.colorValue = newValue;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }

                serializedProperty.serializedObject.Update();
                return !serializedProperty.hasMultipleDifferentValues;
            }

            public static bool DrawMultiValueToggle<Type>(SerializedProperty serializedProperty, bool left = false,
                string displayName = default)
            {
                GUIContent label =
                    GUIContentHelper.GetSerializedPropertyGUIContent<Type>(serializedProperty, displayName);

                serializedProperty.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                bool newValue = serializedProperty.boolValue;

                if (serializedProperty.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                    if (left)
                        newValue = EditorGUILayout.ToggleLeft(label, default);
                    else
                        newValue = EditorGUILayout.Toggle(label, default);
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    if (left)
                        newValue = EditorGUILayout.ToggleLeft(label, newValue);
                    else
                        newValue = EditorGUILayout.Toggle(label, newValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.boolValue = newValue;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }

                serializedProperty.serializedObject.Update();
                return !serializedProperty.hasMultipleDifferentValues;
            }

            public static bool DrawMultiValueObjectField<Type>(SerializedProperty serializedProperty,
                string displayName = default, bool allowSceneObjects = true)
            {
                GUIContent label =
                    GUIContentHelper.GetSerializedPropertyGUIContent<Type>(serializedProperty, displayName);

                serializedProperty.serializedObject.Update();

                if (serializedProperty.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                    EditorGUILayout.ObjectField(serializedProperty, label);
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    EditorGUILayout.ObjectField(serializedProperty, label);
                }

                serializedProperty.serializedObject.ApplyModifiedProperties();
                serializedProperty.serializedObject.Update();
                return !serializedProperty.hasMultipleDifferentValues;
            }

            public static bool DrawMultiValueSlider<Type>(SerializedProperty serializedProperty,
                string displayName = default)
            {
                GUIContent label =
                    GUIContentHelper.GetSerializedPropertyGUIContent<Type>(serializedProperty, displayName);

                serializedProperty.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                float newValue = serializedProperty.floatValue;

                Vector2 range = Attributes.GetSerializedPropertyFloatRange<Type>(serializedProperty);

                if (serializedProperty.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                    if (default != range)
                        newValue = EditorGUILayout.Slider(label, default, range.x, range.y);
                    else
                        newValue = EditorGUILayout.FloatField(label, default);
                    EditorGUI.showMixedValue = false;
                }
                else
                {
                    if (default != range)
                        newValue = EditorGUILayout.Slider(label, newValue, range.x, range.y);
                    else
                        newValue = EditorGUILayout.FloatField(label, newValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.floatValue = newValue;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }

                serializedProperty.serializedObject.Update();
                return !serializedProperty.hasMultipleDifferentValues;
            }

            public static bool DrawWhatsThisSections<Type>(SerializedProperty serializedProperty)
            {
                var attributes =
                    Attributes.GetSerializedPropertyAttributes<Type, WhatsThisAttribute>(serializedProperty);

                if (null == attributes) return false;

                if (0 == attributes.Length) return false;

                bool result = false;

                for (int i = 0; i < attributes.Length; i++)
                {
                    WhatsThisAttribute attribute = attributes[i];

                    if (null == attribute) continue;

                    if (0 == attribute.message.Length) continue;

                    EditorGUILayout.HelpBox(attribute.message, attribute.messageType);

                    result = true;
                }

                return result;
            }

            public static bool DrawWhatsThisFoldout<Type>(ref bool folded, SerializedProperty serializedProperty,
                GUIContent foldoutLabel)
            {
                folded = EditorGUILayout.Foldout(folded, foldoutLabel);

                if (!folded) return false;

                return DrawWhatsThisSections<Type>(serializedProperty);
            }
        }
    }
}