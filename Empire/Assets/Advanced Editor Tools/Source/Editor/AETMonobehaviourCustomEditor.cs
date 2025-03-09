using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedEditorTools.Attributes;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class AETMonobehaviourCustomEditor : Editor
    {
        private readonly int FIELD_WIDTH = 40;
        private readonly int INDENT_WIDTH = 12;
        private readonly int LABEL_MARGIN = 5;
        private readonly float LABEL_MAX_WIDTH_PROP = 0.70f;

        private readonly float LABEL_WIDTH_PROP = 0.35f;
        private AETData _data;

        private FieldInfo[] _fieldsFound;

        private int rootIndentLevel;

        private AETData Data
        {
            get
            {
                try
                {
                    if (_data == null) _data = AETManager.Instance.RetrieveDataOrCreate((MonoBehaviour)target);
                }
                catch
                {
                    return null;
                }

                return _data;
            }
        }

        private FieldInfo[] FieldsFound
        {
            get
            {
                if (_fieldsFound == null)
                {
                    Type targetType = target.GetType();
                    _fieldsFound = targetType
                        .GetFields()
                        .Concat(targetType
                            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                            .Where(field => field.GetCustomAttribute<SerializeField>() != null ||
                                            field.GetCustomAttribute<SerializeReference>() != null))
                        .ToArray();
                }

                return _fieldsFound;
            }
        }

        private GUIStyle LabelStyle => EditorStyles.label;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            AETData data = Data;
            // UpdateFields();
            UpdateLayoutInfos();
            try
            {
                ButtonInfo.UpdateButtonMethods(target, ref data);
            }
            catch
            {
            }
        }

        public override void OnInspectorGUI()
        {
            AETManager aeaManager = AETManager.Instance;
            if (!aeaManager.extensionEnabled)
            {
                base.OnInspectorGUI();
                return;
            }

            AETData data = Data;

            if (aeaManager.debugMode)
            {
                data.baseInspectorFoldout = EditorGUILayout.Foldout(data.baseInspectorFoldout, "Base Inspector");
                if (data.baseInspectorFoldout)
                {
                    EditorGUI.indentLevel++;
                    base.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }

                _ = EditorGUILayout.ObjectField("AEAData", data, typeof(AETData), true);
                if (GUILayout.Button("Reload"))
                    OnValidate();
            }

            EditorGUIUtility.fieldWidth = FIELD_WIDTH;
            PaintDefaultFields();
            EditorGUIUtility.fieldWidth = FIELD_WIDTH;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * LABEL_WIDTH_PROP;
            ButtonInfo.PaintButtonMethods(target, ref data);

            if (GUI.changed)
                EditorUtility.SetDirty(AETManager.Instance);
        }

        private void PaintDefaultFields()
        {
            AETData data = Data;

            // Paint first property disabled (script field)
            SerializedProperty property = serializedObject.GetIterator();
            _ = property.NextVisible(true);
            EditorGUI.BeginDisabledGroup(true);
            _ = EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();

            // Data relative to layout
            int layoutInfoIdx = 0;
            LayoutInfo layoutInfo = data.layoutInfos.Count > 0 ? data.layoutInfos[0] : null;
            rootIndentLevel = EditorGUI.indentLevel;
            int foldoutHiddenCount = 0;
            Stack<int> columnIndentsCompensated = new();
            bool fieldMustBePainted = true;
            Stack<LayoutStyle> columnStyles = new();

            // Paint fields 
            // foreach (var field in data.fields)
            while (property.NextVisible(false))
            {
                SerializedProperty field = property;
                // if (field.isUsingRealTarget && field.isUsingRealContainer) field.SerializedObject = serializedObject;

                bool fieldHasBeenConsidered = false;

                if (layoutInfo != null)
                    // while(layoutInfo.fieldName.Equals(field.fieldName) && layoutInfo.FieldType.Equals(field.Type))
                    while (layoutInfo.fieldName.Equals(property.name))
                    {
                        switch (layoutInfo)
                        {
                            case ColumnInfo columnInfo:
                                if (!fieldMustBePainted)
                                    break;

                                switch (columnInfo)
                                {
                                    case BeginColumnAreaInfo beginColumnAreaInfo:
                                        columnStyles.Push(beginColumnAreaInfo.columnStyle);
                                        _ = EditorGUILayout.BeginHorizontal(
                                            GetWindowStyle(beginColumnAreaInfo.windowStyle));
                                        CorrectEmptyColumnAutoWidth(layoutInfoIdx, ref data);
                                        _ = EditorGUILayout.BeginVertical(GetContainerStyle(columnStyles.Peek(),
                                            beginColumnAreaInfo.columnWidth));
                                        CompensateColumnIndent(ref columnIndentsCompensated);
                                        break;
                                    case NewColumnInfo newColumnInfo:
                                        EditorGUILayout.EndVertical();
                                        CorrectEmptyColumnAutoWidth(layoutInfoIdx, ref data);
                                        LayoutStyle columnStyle = newColumnInfo.columnStyle ?? columnStyles.Peek();
                                        _ = EditorGUILayout.BeginVertical(GetContainerStyle(columnStyle,
                                            newColumnInfo.columnWidth));

                                        DecompensateColumnIndent(ref columnIndentsCompensated);
                                        CompensateColumnIndent(ref columnIndentsCompensated);
                                        break;
                                    case NewEmptyColumnInfo newEmptyColumnInfo:
                                        EditorGUILayout.EndVertical();
                                        _ = EditorGUILayout.BeginVertical(
                                            GetEmptyContainerStyle(newEmptyColumnInfo.columnWidth));
                                        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                                        break;
                                    case EndColumnAreaInfo endColumnAreaInfo:
                                        if (endColumnAreaInfo.includeLast)
                                            if (!fieldHasBeenConsidered)
                                            {
                                                PaintField(field);
                                                fieldHasBeenConsidered = true;
                                            }

                                        _ = columnStyles.Pop();
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.EndHorizontal();
                                        DecompensateColumnIndent(ref columnIndentsCompensated);

                                        break;
                                }

                                break;
                            case BeginFoldoutInfo foldoutInfo:
                                if (fieldMustBePainted)
                                {
                                    foldoutInfo.foldout =
                                        EditorGUILayout.Foldout(foldoutInfo.foldout, foldoutInfo.label);
                                    if (foldoutInfo.foldout)
                                        EditorGUI.indentLevel++;
                                    else
                                        fieldMustBePainted = false;
                                }
                                else
                                {
                                    foldoutHiddenCount++;
                                }

                                break;
                            case EndFoldoutInfo foldoutInfo:
                                if (foldoutHiddenCount > 0)
                                {
                                    foldoutHiddenCount--;
                                }
                                else
                                {
                                    if (foldoutInfo.includeLast)
                                    {
                                        if (fieldMustBePainted && !fieldHasBeenConsidered)
                                            PaintField(field);

                                        fieldHasBeenConsidered = true;
                                    }

                                    if (fieldMustBePainted)
                                        EditorGUI.indentLevel--;
                                    DecompensateColumnIndent(ref columnIndentsCompensated);

                                    fieldMustBePainted = true;
                                }

                                break;
                        }

                        layoutInfoIdx++;
                        if (layoutInfoIdx == data.layoutInfos.Count)
                        {
                            layoutInfo = null;
                            break;
                        }

                        layoutInfo = data.layoutInfos[layoutInfoIdx];
                    }

                if (fieldMustBePainted && !fieldHasBeenConsidered)
                    PaintField(field);
            }

            if (EditorGUI.indentLevel != rootIndentLevel)
                Debug.LogWarning(
                    $"Foldout scope not closed in GameObject '{target.name}'. Unexpected behaviour may occur.");
            EditorGUI.indentLevel = rootIndentLevel;

            if (serializedObject.hasModifiedProperties)
                _ = serializedObject.ApplyModifiedProperties();
        }

        private void CompensateColumnIndent(ref Stack<int> columnIndentsCompensated)
        {
            if (EditorGUI.indentLevel != rootIndentLevel)
            {
                columnIndentsCompensated.Push(EditorGUI.indentLevel);
                EditorGUI.indentLevel = rootIndentLevel;
            }
        }

        private void DecompensateColumnIndent(ref Stack<int> columnIndentsCompensated)
        {
            EditorGUI.indentLevel =
                columnIndentsCompensated.Count > 0 ? columnIndentsCompensated.Pop() : rootIndentLevel;
        }

        private void PaintField(SerializedProperty property)
        {
            SetLabelSpace(property);
            _ = EditorGUILayout.PropertyField(property);
        }

        private void SetLabelSpace(SerializedProperty property)
        {
            float labelSize = GetMaximumLabelSize(property) + LABEL_MARGIN;

            _ = EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            Rect rect = GUILayoutUtility.GetLastRect();

            float suggestedLabelSize = LABEL_WIDTH_PROP * rect.width;
            float maxLabelSize = LABEL_MAX_WIDTH_PROP * rect.width;

            EditorGUIUtility.labelWidth = Mathf.Clamp(labelSize, suggestedLabelSize, maxLabelSize);
        }

        private float GetMaximumLabelSize(SerializedProperty prop)
        {
            float labelSize = GetLabelSize(prop);
            float childMaxLabelSize = GetMaximumLabelSizeRec(prop.Copy());

            return Mathf.Max(labelSize, childMaxLabelSize);
        }

        private float GetMaximumLabelSizeRec(SerializedProperty prop)
        {
            if (prop.hasVisibleChildren)
            {
                List<float> maxLabelSizes = new();
                EditorGUI.indentLevel++;
                SerializedProperty endOfChildrenIteration = prop.GetEndProperty();
                while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endOfChildrenIteration))
                    maxLabelSizes.Add(GetMaximumLabelSizeRec(prop));

                EditorGUI.indentLevel--;
                return maxLabelSizes.Count > 0 ? maxLabelSizes.Max() : 0;
            }

            return GetLabelSize(prop);
        }

        private float GetLabelSize(SerializedProperty prop)
        {
            return GetLabelSize(prop.displayName);
        }

        private float GetLabelSize(string label)
        {
            return LabelStyle.CalcSize(new GUIContent(label)).x + (EditorGUI.indentLevel + 1) * INDENT_WIDTH;
        }

        private void CorrectEmptyColumnAutoWidth(int layoutInfoIdx, ref AETData data)
        {
            if (layoutInfoIdx + 1 < data.layoutInfos.Count)
            {
                LayoutInfo nextLayout = data.layoutInfos[layoutInfoIdx + 1];
                if (nextLayout is BeginFoldoutInfo foldoutInfo && !foldoutInfo.foldout)
                    EditorGUIUtility.fieldWidth = LabelStyle.CalcSize(new GUIContent(foldoutInfo.label)).x;
            }
            else
            {
                EditorGUIUtility.fieldWidth = FIELD_WIDTH;
            }
        }

        /*
        private void UpdateFields()
        {
            var data = Data;
            var fields = FieldsFound;

            var newSerializableFields = fields.Select(fieldInfo => new SerializedField(target, fieldInfo.Name, fieldInfo.FieldType, serializedObject));
            data.fields = newSerializableFields.MatchEnumerables(data.fields, true).ToList();
        }
        */

        private GUIStyle GetWindowStyle(LayoutStyle style)
        {
            string strStyle = style.GetString();
            GUIStyle customWindowStyle = strStyle == null ? new GUIStyle() : new GUIStyle(strStyle);
            customWindowStyle.padding = new RectOffset(2, 2, 2, 2);
            customWindowStyle.margin = new RectOffset(2, 2, 2, 2);

            return customWindowStyle;
        }

        private GUIStyle GetContainerStyle(LayoutStyle style, float? columnWidth)
        {
            string strStyle = style.GetString();
            GUIStyle customBoxStyle = strStyle == null ? new GUIStyle() : new GUIStyle(strStyle);
            customBoxStyle.padding = new RectOffset(1, 1, 3, 3);
            customBoxStyle.margin = new RectOffset(0, INDENT_WIDTH, 0, 0);

            if (columnWidth.HasValue)
                customBoxStyle.fixedWidth = GetAvailableWidth() * columnWidth.Value * 0.95f;

            return customBoxStyle;
        }

        private GUIStyle GetEmptyContainerStyle(float width)
        {
            GUIStyle customBoxStyle = new()
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                fixedWidth = GetAvailableWidth() * width * 0.95f,
                fixedHeight = 0
            };
            return customBoxStyle;
        }

        private float GetAvailableWidth()
        {
            return EditorGUIUtility.currentViewWidth - (EditorGUI.indentLevel + 1) * INDENT_WIDTH;
        }

        private void UpdateLayoutInfos()
        {
            AETData data = Data;
            var fields = FieldsFound;
            List<LayoutInfo> newLayoutInfos = new();
            foreach (FieldInfo fieldInfo in fields)
            {
                object[] attributtes = fieldInfo.GetCustomAttributes(typeof(LayoutAttribute), true);
                foreach (object attr in attributtes)
                {
                    LayoutInfo layoutInfo = attr switch
                    {
                        FoldoutAttribute foldoutAttr =>
                            foldoutAttr is BeginFoldoutAttribute
                                ? new BeginFoldoutInfo { label = (foldoutAttr as BeginFoldoutAttribute).label }
                                : new EndFoldoutInfo { includeLast = (foldoutAttr as EndFoldoutAttribute).includeLast },
                        ColumnAttribute columnAttr =>
                            columnAttr is EndColumnAreaAttribute
                                ?
                                new EndColumnAreaInfo
                                    { includeLast = (columnAttr as EndColumnAreaAttribute).includeLast }
                                : columnAttr is BeginColumnAreaAttribute
                                    ? new BeginColumnAreaInfo
                                    {
                                        windowStyle = (columnAttr as BeginColumnAreaAttribute).areaStyle,
                                        columnStyle = (columnAttr as BeginColumnAreaAttribute).columnStyle,
                                        columnWidth = (columnAttr as BeginColumnAreaAttribute).columnWidth
                                    }
                                    : columnAttr is NewColumnAttribute
                                        ? new NewColumnInfo
                                        {
                                            columnStyle = (columnAttr as NewColumnAttribute).columnStyle,
                                            columnWidth = (columnAttr as NewColumnAttribute).columnWidth
                                        }
                                        :
                                        new NewEmptyColumnInfo
                                            { columnWidth = (columnAttr as NewEmptyColumnAttribute).columnWidth },
                        _ => throw new Exception("Invalid Layout Attribute found")
                    };
                    layoutInfo.fieldName = fieldInfo.Name;
                    layoutInfo.fieldTypeName = fieldInfo.FieldType.AssemblyQualifiedName;
                    newLayoutInfos.Add(layoutInfo);
                }
            }

            try
            {
                data.layoutInfos = newLayoutInfos.MatchEnumerables(data.layoutInfos).ToList();
            }
            catch
            {
            }
        }
    }
}