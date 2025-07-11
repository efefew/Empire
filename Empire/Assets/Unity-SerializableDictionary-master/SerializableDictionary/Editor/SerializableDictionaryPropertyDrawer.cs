﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
#if NET_4_6 || NET_STANDARD_2_0
[CustomPropertyDrawer(typeof(SerializableHashSetBase), true)]
#endif
public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
    private const string KeysFieldName = "m_keys";
    private const string ValuesFieldName = "m_values";
    protected const float IndentWidth = 15f;

    private static GUIContent s_iconPlus = IconContent("Toolbar Plus", "Add entry");
    private static GUIContent s_iconMinus = IconContent("Toolbar Minus", "Remove entry");

    private static GUIContent s_warningIconConflict =
        IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");

    private static GUIContent s_warningIconOther = IconContent("console.infoicon.sml", "Conflicting key");

    private static GUIContent s_warningIconNull =
        IconContent("console.warnicon.sml", "Null key, this entry will be lost");

    private static GUIStyle s_buttonStyle = GUIStyle.none;
    private static GUIContent s_tempContent = new();

    private static Dictionary<PropertyIdentity, ConflictState> s_conflictStateDict = new();

    private static Dictionary<SerializedPropertyType, PropertyInfo> s_serializedPropertyValueAccessorsDict;

    static SerializableDictionaryPropertyDrawer()
    {
        var serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>
        {
            { SerializedPropertyType.Integer, "intValue" },
            { SerializedPropertyType.Boolean, "boolValue" },
            { SerializedPropertyType.Float, "floatValue" },
            { SerializedPropertyType.String, "stringValue" },
            { SerializedPropertyType.Color, "colorValue" },
            { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
            { SerializedPropertyType.LayerMask, "intValue" },
            { SerializedPropertyType.Enum, "intValue" },
            { SerializedPropertyType.Vector2, "vector2Value" },
            { SerializedPropertyType.Vector3, "vector3Value" },
            { SerializedPropertyType.Vector4, "vector4Value" },
            { SerializedPropertyType.Rect, "rectValue" },
            { SerializedPropertyType.ArraySize, "intValue" },
            { SerializedPropertyType.Character, "intValue" },
            { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
            { SerializedPropertyType.Bounds, "boundsValue" },
            { SerializedPropertyType.Quaternion, "quaternionValue" }
        };
        Type serializedPropertyType = typeof(SerializedProperty);

        s_serializedPropertyValueAccessorsDict = new Dictionary<SerializedPropertyType, PropertyInfo>();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        foreach (var kvp in serializedPropertyValueAccessorsNameDict)
        {
            PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
            s_serializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        Action buttonAction = Action.None;
        int buttonActionIndex = 0;

        SerializedProperty keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
        SerializedProperty valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

        ConflictState conflictState = GetConflictState(property);

        if (conflictState.conflictIndex != -1)
        {
            keyArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
            SerializedProperty keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
            SetPropertyValue(keyProperty, conflictState.conflictKey);
            keyProperty.isExpanded = conflictState.conflictKeyPropertyExpanded;

            if (valueArrayProperty != null)
            {
                valueArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
                SerializedProperty valueProperty =
                    valueArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
                SetPropertyValue(valueProperty, conflictState.conflictValue);
                valueProperty.isExpanded = conflictState.conflictValuePropertyExpanded;
            }
        }

        float buttonWidth = s_buttonStyle.CalcSize(s_iconPlus).x;

        Rect labelPosition = position;
        labelPosition.height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
            labelPosition.xMax -= s_buttonStyle.CalcSize(s_iconPlus).x;

        EditorGUI.PropertyField(labelPosition, property, label, false);
        // property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
        if (property.isExpanded)
        {
            Rect buttonPosition = position;
            buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(conflictState.conflictIndex != -1);
            if (GUI.Button(buttonPosition, s_iconPlus, s_buttonStyle))
            {
                buttonAction = Action.Add;
                buttonActionIndex = keyArrayProperty.arraySize;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel++;
            Rect linePosition = position;
            linePosition.y += EditorGUIUtility.singleLineHeight;
            linePosition.xMax -= buttonWidth;

            foreach (EnumerationEntry entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
            {
                SerializedProperty keyProperty = entry.keyProperty;
                SerializedProperty valueProperty = entry.valueProperty;
                int i = entry.index;

                float lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition, i);

                buttonPosition = linePosition;
                buttonPosition.x = linePosition.xMax;
                buttonPosition.height = EditorGUIUtility.singleLineHeight;
                if (GUI.Button(buttonPosition, s_iconMinus, s_buttonStyle))
                {
                    buttonAction = Action.Remove;
                    buttonActionIndex = i;
                }

                if (i == conflictState.conflictIndex && conflictState.conflictOtherIndex == -1)
                {
                    Rect iconPosition = linePosition;
                    iconPosition.size = s_buttonStyle.CalcSize(s_warningIconNull);
                    GUI.Label(iconPosition, s_warningIconNull);
                }
                else if (i == conflictState.conflictIndex)
                {
                    Rect iconPosition = linePosition;
                    iconPosition.size = s_buttonStyle.CalcSize(s_warningIconConflict);
                    GUI.Label(iconPosition, s_warningIconConflict);
                }
                else if (i == conflictState.conflictOtherIndex)
                {
                    Rect iconPosition = linePosition;
                    iconPosition.size = s_buttonStyle.CalcSize(s_warningIconOther);
                    GUI.Label(iconPosition, s_warningIconOther);
                }


                linePosition.y += lineHeight;
            }

            EditorGUI.indentLevel--;
        }

        if (buttonAction == Action.Add)
        {
            keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
            if (valueArrayProperty != null)
                valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
        }
        else if (buttonAction == Action.Remove)
        {
            DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
            if (valueArrayProperty != null)
                DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
        }

        conflictState.conflictKey = null;
        conflictState.conflictValue = null;
        conflictState.conflictIndex = -1;
        conflictState.conflictOtherIndex = -1;
        conflictState.conflictLineHeight = 0f;
        conflictState.conflictKeyPropertyExpanded = false;
        conflictState.conflictValuePropertyExpanded = false;

        foreach (EnumerationEntry entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
        {
            SerializedProperty keyProperty1 = entry1.keyProperty;
            int i = entry1.index;
            object keyProperty1Value = GetPropertyValue(keyProperty1);

            if (keyProperty1Value == null)
            {
                SerializedProperty valueProperty1 = entry1.valueProperty;
                SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
                DeleteArrayElementAtIndex(keyArrayProperty, i);
                if (valueArrayProperty != null)
                    DeleteArrayElementAtIndex(valueArrayProperty, i);

                break;
            }


            foreach (EnumerationEntry entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
            {
                SerializedProperty keyProperty2 = entry2.keyProperty;
                int j = entry2.index;
                object keyProperty2Value = GetPropertyValue(keyProperty2);

                if (ComparePropertyValues(keyProperty1Value, keyProperty2Value))
                {
                    SerializedProperty valueProperty2 = entry2.valueProperty;
                    SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
                    DeleteArrayElementAtIndex(keyArrayProperty, j);
                    if (valueArrayProperty != null)
                        DeleteArrayElementAtIndex(valueArrayProperty, j);

                    goto breakLoops;
                }
            }
        }

        breakLoops:

        EditorGUI.EndProperty();
    }

    private static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty,
        Rect linePosition, int index)
    {
        bool keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

        if (valueProperty != null)
        {
            bool valueCanBeExpanded = CanPropertyBeExpanded(valueProperty);

            if (!keyCanBeExpanded && valueCanBeExpanded)
                return DrawKeyValueLineExpand(keyProperty, valueProperty, linePosition);

            string keyLabel = keyCanBeExpanded ? "Key " + index : "";
            string valueLabel = valueCanBeExpanded ? "Value " + index : "";
            return DrawKeyValueLineSimple(keyProperty, valueProperty, keyLabel, valueLabel, linePosition);
        }

        if (!keyCanBeExpanded) return DrawKeyLine(keyProperty, linePosition, null);

        {
            string keyLabel = string.Format("{0} {1}", ObjectNames.NicifyVariableName(keyProperty.type), index);
            return DrawKeyLine(keyProperty, linePosition, keyLabel);
        }
    }

    private static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty,
        string keyLabel, string valueLabel, Rect linePosition)
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        float labelWidthRelative = labelWidth / linePosition.width;

        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        Rect keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = labelWidth - IndentWidth;
        EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
        EditorGUI.PropertyField(keyPosition, keyProperty, TempContent(keyLabel), true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        Rect valuePosition = linePosition;
        valuePosition.height = valuePropertyHeight;
        valuePosition.xMin += labelWidth;
        EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
        EditorGUI.indentLevel--;
        EditorGUI.PropertyField(valuePosition, valueProperty, TempContent(valueLabel), true);
        EditorGUI.indentLevel++;

        EditorGUIUtility.labelWidth = labelWidth;

        return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
    }

    private static float DrawKeyValueLineExpand(SerializedProperty keyProperty, SerializedProperty valueProperty,
        Rect linePosition)
    {
        float labelWidth = EditorGUIUtility.labelWidth;

        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        Rect keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = labelWidth - IndentWidth;
        EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        Rect valuePosition = linePosition;
        valuePosition.height = valuePropertyHeight;
        EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

        EditorGUIUtility.labelWidth = labelWidth;

        return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
    }

    private static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
    {
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        Rect keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = linePosition.width;

        GUIContent keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
        EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

        return keyPropertyHeight;
    }

    private static bool CanPropertyBeExpanded(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Generic:
            case SerializedPropertyType.Vector4:
            case SerializedPropertyType.Quaternion:
                return true;
            default:
                return false;
        }
    }

    private static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index,
        int otherIndex, ConflictState conflictState)
    {
        conflictState.conflictKey = GetPropertyValue(keyProperty);
        conflictState.conflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
        float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        conflictState.conflictLineHeight = lineHeight;
        conflictState.conflictIndex = index;
        conflictState.conflictOtherIndex = otherIndex;
        conflictState.conflictKeyPropertyExpanded = keyProperty.isExpanded;
        conflictState.conflictValuePropertyExpanded = valueProperty != null ? valueProperty.isExpanded : false;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float propertyHeight = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            SerializedProperty keysProperty = property.FindPropertyRelative(KeysFieldName);
            SerializedProperty valuesProperty = property.FindPropertyRelative(ValuesFieldName);

            foreach (EnumerationEntry entry in EnumerateEntries(keysProperty, valuesProperty))
            {
                SerializedProperty keyProperty = entry.keyProperty;
                SerializedProperty valueProperty = entry.valueProperty;
                float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
                float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
                float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                propertyHeight += lineHeight;
            }

            ConflictState conflictState = GetConflictState(property);

            if (conflictState.conflictIndex != -1) propertyHeight += conflictState.conflictLineHeight;
        }

        return propertyHeight;
    }

    private static ConflictState GetConflictState(SerializedProperty property)
    {
        ConflictState conflictState;
        PropertyIdentity propId = new(property);
        if (!s_conflictStateDict.TryGetValue(propId, out conflictState))
        {
            conflictState = new ConflictState();
            s_conflictStateDict.Add(propId, conflictState);
        }

        return conflictState;
    }

    private static GUIContent IconContent(string name, string tooltip)
    {
        GUIContent builtinIcon = EditorGUIUtility.IconContent(name);
        return new GUIContent(builtinIcon.image, tooltip);
    }

    private static GUIContent TempContent(string text)
    {
        s_tempContent.text = text;
        return s_tempContent;
    }

    private static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
    {
        SerializedProperty property = arrayProperty.GetArrayElementAtIndex(index);
        // if(arrayProperty.arrayElementType.StartsWith("PPtr<$"))
        if (property.propertyType == SerializedPropertyType.ObjectReference) property.objectReferenceValue = null;

        arrayProperty.DeleteArrayElementAtIndex(index);
    }

    public static object GetPropertyValue(SerializedProperty p)
    {
        PropertyInfo propertyInfo;
        if (s_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
            return propertyInfo.GetValue(p, null);

        if (p.isArray)
            return GetPropertyValueArray(p);
        return GetPropertyValueGeneric(p);
    }

    private static void SetPropertyValue(SerializedProperty p, object v)
    {
        PropertyInfo propertyInfo;
        if (s_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
        {
            propertyInfo.SetValue(p, v, null);
        }
        else
        {
            if (p.isArray)
                SetPropertyValueArray(p, v);
            else
                SetPropertyValueGeneric(p, v);
        }
    }

    private static object GetPropertyValueArray(SerializedProperty property)
    {
        object[] array = new object[property.arraySize];
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            array[i] = GetPropertyValue(item);
        }

        return array;
    }

    private static object GetPropertyValueGeneric(SerializedProperty property)
    {
        var dict = new Dictionary<string, object>();
        SerializedProperty iterator = property.Copy();
        if (iterator.Next(true))
        {
            SerializedProperty end = property.GetEndProperty();
            do
            {
                string name = iterator.name;
                object value = GetPropertyValue(iterator);
                dict.Add(name, value);
            } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
        }

        return dict;
    }

    private static void SetPropertyValueArray(SerializedProperty property, object v)
    {
        object[] array = (object[])v;
        property.arraySize = array.Length;
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            SetPropertyValue(item, array[i]);
        }
    }

    private static void SetPropertyValueGeneric(SerializedProperty property, object v)
    {
        var dict = (Dictionary<string, object>)v;
        SerializedProperty iterator = property.Copy();
        if (iterator.Next(true))
        {
            SerializedProperty end = property.GetEndProperty();
            do
            {
                string name = iterator.name;
                SetPropertyValue(iterator, dict[name]);
            } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
        }
    }

    private static bool ComparePropertyValues(object value1, object value2)
    {
        if (value1 is Dictionary<string, object> && value2 is Dictionary<string, object>)
        {
            var dict1 = (Dictionary<string, object>)value1;
            var dict2 = (Dictionary<string, object>)value2;
            return CompareDictionaries(dict1, dict2);
        }

        return Equals(value1, value2);
    }

    private static bool CompareDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
    {
        if (dict1.Count != dict2.Count)
            return false;

        foreach (var kvp1 in dict1)
        {
            string key1 = kvp1.Key;
            object value1 = kvp1.Value;

            object value2;
            if (!dict2.TryGetValue(key1, out value2))
                return false;

            if (!ComparePropertyValues(value1, value2))
                return false;
        }

        return true;
    }

    private static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty,
        SerializedProperty valueArrayProperty, int startIndex = 0)
    {
        if (keyArrayProperty.arraySize > startIndex)
        {
            int index = startIndex;
            SerializedProperty keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
            SerializedProperty valueProperty = valueArrayProperty != null
                ? valueArrayProperty.GetArrayElementAtIndex(startIndex)
                : null;
            SerializedProperty endProperty = keyArrayProperty.GetEndProperty();

            do
            {
                yield return new EnumerationEntry(keyProperty, valueProperty, index);
                index++;
            } while (keyProperty.Next(false)
                     && (valueProperty != null ? valueProperty.Next(false) : true)
                     && !SerializedProperty.EqualContents(keyProperty, endProperty));
        }
    }


    private class ConflictState
    {
        public int conflictIndex = -1;
        public object conflictKey;
        public bool conflictKeyPropertyExpanded;
        public float conflictLineHeight;
        public int conflictOtherIndex = -1;
        public object conflictValue;
        public bool conflictValuePropertyExpanded;
    }

    private struct PropertyIdentity
    {
        public PropertyIdentity(SerializedProperty property)
        {
            instance = property.serializedObject.targetObject;
            propertyPath = property.propertyPath;
        }

        public Object instance;
        public string propertyPath;
    }

    private enum Action
    {
        None,
        Add,
        Remove
    }

    private struct EnumerationEntry
    {
        public SerializedProperty keyProperty;
        public SerializedProperty valueProperty;
        public int index;

        public EnumerationEntry(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
        {
            this.keyProperty = keyProperty;
            this.valueProperty = valueProperty;
            this.index = index;
        }
    }
}

[CustomPropertyDrawer(typeof(SerializableDictionaryBase.Storage), true)]
public class SerializableDictionaryStoragePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.Next(true);
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property.Next(true);
        return EditorGUI.GetPropertyHeight(property);
    }
}