using AdvancedEditorTools.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class AETMonobehaviourCustomEditor : Editor
    {
        private AETData _data;
        private AETData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = AETManager.Instance.RetrieveDataOrCreate((MonoBehaviour)target);
                }
                return _data;
            }
        }

        FieldInfo[] _fieldsFound;
        FieldInfo[] FieldsFound
        {
            get
            {
                if (_fieldsFound == null)
                {
                    var targetType = target.GetType();
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

        readonly float LABEL_WIDTH_PROP = 0.35f;
        readonly float LABEL_MAX_WIDTH_PROP = 0.70f;
        readonly int LABEL_MARGIN = 5;
        readonly int INDENT_WIDTH = 12;
        readonly int FIELD_WIDTH = 40;

        public override void OnInspectorGUI()
        {
            var aeaManager = AETManager.Instance;
            if (!aeaManager.extensionEnabled)
            {
                base.OnInspectorGUI();
                return;
            }

            var data = Data;

            if (aeaManager.debugMode)
            {
                data.baseInspectorFoldout = EditorGUILayout.Foldout(data.baseInspectorFoldout, "Base Inspector");
                if (data.baseInspectorFoldout)
                {
                    EditorGUI.indentLevel++;
                    base.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.ObjectField("AEAData", data, typeof(AETData), true);
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

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            var data = Data;
            // UpdateFields();
            UpdateLayoutInfos();
            ButtonInfo.UpdateButtonMethods(target, ref data);
        }

        int rootIndentLevel;
        private void PaintDefaultFields()
        {
            var data = Data;

            // Paint first property disabled (script field)
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
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
                var field = property;
                // if (field.isUsingRealTarget && field.isUsingRealContainer) field.SerializedObject = serializedObject;

                bool fieldHasBeenConsidered = false;

                if (layoutInfo != null)
                {
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
                                        EditorGUILayout.BeginHorizontal(GetWindowStyle(beginColumnAreaInfo.windowStyle));
                                        CorrectEmptyColumnAutoWidth(layoutInfoIdx, ref data);
                                        EditorGUILayout.BeginVertical(GetContainerStyle(columnStyles.Peek(), beginColumnAreaInfo.columnWidth));
                                        CompensateColumnIndent(ref columnIndentsCompensated);
                                        break;
                                    case NewColumnInfo newColumnInfo:
                                        EditorGUILayout.EndVertical();
                                        CorrectEmptyColumnAutoWidth(layoutInfoIdx, ref data);
                                        var columnStyle = newColumnInfo.columnStyle ?? columnStyles.Peek();
                                        EditorGUILayout.BeginVertical(GetContainerStyle(columnStyle, newColumnInfo.columnWidth));

                                        DecompensateColumnIndent(ref columnIndentsCompensated);
                                        CompensateColumnIndent(ref columnIndentsCompensated);
                                        break;
                                    case NewEmptyColumnInfo newEmptyColumnInfo:
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.BeginVertical(GetEmptyContainerStyle(newEmptyColumnInfo.columnWidth));
                                        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                                        break;
                                    case EndColumnAreaInfo endColumnAreaInfo:
                                        if (endColumnAreaInfo.includeLast)
                                        {
                                            if (!fieldHasBeenConsidered)
                                            {
                                                PaintField(field);
                                                fieldHasBeenConsidered = true;
                                            }
                                        }
                                        columnStyles.Pop();
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.EndHorizontal();
                                        DecompensateColumnIndent(ref columnIndentsCompensated);

                                        break;
                                }
                                break;
                            case BeginFoldoutInfo foldoutInfo:
                                if (fieldMustBePainted)
                                {
                                    foldoutInfo.foldout = EditorGUILayout.Foldout(foldoutInfo.foldout, foldoutInfo.label);
                                    if (foldoutInfo.foldout)
                                        EditorGUI.indentLevel++;
                                    else
                                        fieldMustBePainted = false;
                                }
                                else
                                    foldoutHiddenCount++;
                                break;
                            case EndFoldoutInfo foldoutInfo:
                                if (foldoutHiddenCount > 0)
                                    foldoutHiddenCount--;
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
                            default:
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
                }

                if (fieldMustBePainted && !fieldHasBeenConsidered)
                    PaintField(field);
            }

            if (EditorGUI.indentLevel != rootIndentLevel)
                Debug.LogWarning($"Foldout scope not closed in GameObject '{target.name}'. Unexpected behaviour may occur.");
            EditorGUI.indentLevel = rootIndentLevel;

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
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
            if (columnIndentsCompensated.Count > 0)
                EditorGUI.indentLevel = columnIndentsCompensated.Pop();
            else
                EditorGUI.indentLevel = rootIndentLevel;
        }

        GUIStyle LabelStyle => EditorStyles.label;

        private void PaintField(SerializedProperty property)
        {
            SetLabelSpace(property);
            EditorGUILayout.PropertyField(property);
        }

        private void SetLabelSpace(SerializedProperty property)
        {
            var labelSize = GetMaximumLabelSize(property) + LABEL_MARGIN;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            Rect rect = GUILayoutUtility.GetLastRect();

            var suggestedLabelSize = LABEL_WIDTH_PROP * rect.width;
            var maxLabelSize = LABEL_MAX_WIDTH_PROP * rect.width;

            EditorGUIUtility.labelWidth = Mathf.Clamp(labelSize, suggestedLabelSize, maxLabelSize);
        }

        private float GetMaximumLabelSize(SerializedProperty prop)
        {
            var labelSize = GetLabelSize(prop);
            var childMaxLabelSize = GetMaximumLabelSizeRec(prop.Copy());

            return Mathf.Max(labelSize, childMaxLabelSize);
        }

        private float GetMaximumLabelSizeRec(SerializedProperty prop)
        {
            if (prop.hasVisibleChildren)
            {
                List<float> maxLabelSizes = new();
                EditorGUI.indentLevel++;
                var endOfChildrenIteration = prop.GetEndProperty();
                while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endOfChildrenIteration))
                {
                    maxLabelSizes.Add(GetMaximumLabelSizeRec(prop));
                }
                EditorGUI.indentLevel--;
                return maxLabelSizes.Count > 0 ? maxLabelSizes.Max() : 0;
            }
            return GetLabelSize(prop);
        }

        private float GetLabelSize(SerializedProperty prop) => GetLabelSize(prop.displayName);
        private float GetLabelSize(string label) => LabelStyle.CalcSize(new GUIContent(label)).x + (EditorGUI.indentLevel + 1) * INDENT_WIDTH;

        private void CorrectEmptyColumnAutoWidth(int layoutInfoIdx, ref AETData data)
        {
            if (layoutInfoIdx + 1 < data.layoutInfos.Count)
            {
                var nextLayout = data.layoutInfos[layoutInfoIdx + 1];
                if (nextLayout is BeginFoldoutInfo foldoutInfo && !foldoutInfo.foldout)
                    EditorGUIUtility.fieldWidth = LabelStyle.CalcSize(new GUIContent(foldoutInfo.label)).x;
            }
            else
                EditorGUIUtility.fieldWidth = FIELD_WIDTH;
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
            var strStyle = style.GetString();
            GUIStyle customWindowStyle = strStyle == null ? new() : new(strStyle);
            customWindowStyle.padding = new RectOffset(2, 2, 2, 2);
            customWindowStyle.margin = new RectOffset(2, 2, 2, 2);

            return customWindowStyle;
        }

        private GUIStyle GetContainerStyle(LayoutStyle style, float? columnWidth)
        {
            var strStyle = style.GetString();
            GUIStyle customBoxStyle = strStyle == null ? new() : new(strStyle);
            customBoxStyle.padding = new RectOffset(1, 1, 3, 3);
            customBoxStyle.margin = new RectOffset(0, INDENT_WIDTH, 0, 0);

            if (columnWidth.HasValue)
                customBoxStyle.fixedWidth = GetAvailableWidth() * columnWidth.Value * 0.95f;

            return customBoxStyle;
        }

        private GUIStyle GetEmptyContainerStyle(float width)
        {
            GUIStyle customBoxStyle = new();
            customBoxStyle.padding = new RectOffset(0, 0, 0, 0);
            customBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            customBoxStyle.fixedWidth = GetAvailableWidth() * width * 0.95f;
            customBoxStyle.fixedHeight = 0;
            return customBoxStyle;
        }

        private float GetAvailableWidth() => EditorGUIUtility.currentViewWidth - (EditorGUI.indentLevel + 1) * INDENT_WIDTH;

        private void UpdateLayoutInfos()
        {
            var data = Data;
            var fields = FieldsFound;
            var newLayoutInfos = new List<LayoutInfo>();
            foreach (var fieldInfo in fields)
            {
                var attributtes = fieldInfo.GetCustomAttributes(typeof(LayoutAttribute), true);
                foreach (var attr in attributtes)
                {
                    LayoutInfo layoutInfo = attr switch
                    {
                        FoldoutAttribute foldoutAttr =>
                            foldoutAttr is BeginFoldoutAttribute ?
                                new BeginFoldoutInfo() { label = (foldoutAttr as BeginFoldoutAttribute).label } :
                                new EndFoldoutInfo() { includeLast = (foldoutAttr as EndFoldoutAttribute).includeLast },
                        ColumnAttribute columnAttr =>
                            columnAttr is EndColumnAreaAttribute ?
                                new EndColumnAreaInfo() { includeLast = (columnAttr as EndColumnAreaAttribute).includeLast } :
                            columnAttr is BeginColumnAreaAttribute ?
                                new BeginColumnAreaInfo()
                                {
                                    windowStyle = (columnAttr as BeginColumnAreaAttribute).areaStyle,
                                    columnStyle = (columnAttr as BeginColumnAreaAttribute).columnStyle,
                                    columnWidth = (columnAttr as BeginColumnAreaAttribute).columnWidth
                                } :
                            columnAttr is NewColumnAttribute ?
                                new NewColumnInfo()
                                {
                                    columnStyle = (columnAttr as NewColumnAttribute).columnStyle,
                                    columnWidth = (columnAttr as NewColumnAttribute).columnWidth
                                } :
                                new NewEmptyColumnInfo() { columnWidth = (columnAttr as NewEmptyColumnAttribute).columnWidth },
                        _ => throw new System.Exception("Invalid Layout Attribute found"),
                    };
                    layoutInfo.fieldName = fieldInfo.Name;
                    layoutInfo.fieldTypeName = fieldInfo.FieldType.AssemblyQualifiedName;
                    newLayoutInfos.Add(layoutInfo);
                }
            }

            data.layoutInfos = newLayoutInfos.MatchEnumerables(data.layoutInfos, false).ToList();
        }
    }
}
