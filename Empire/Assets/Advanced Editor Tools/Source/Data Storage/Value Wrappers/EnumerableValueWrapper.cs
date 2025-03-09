#if UNITY_EDITOR

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public abstract class EnumerableValueWrapper : GenericValueWrapper<List<ValueWrapper>>
    {
        public bool foldout = true;
        public string elementTypeName;

        private ReorderableList _reorderableList;
        public Type ElementType => Type.GetType(elementTypeName);

        public ReorderableList ReorderableList
        {
            get
            {
                if (_reorderableList == null || _reorderableList.Equals(null))
                    _reorderableList = CreateReorderableList();
                return _reorderableList;
            }
        }

        public List<ValueWrapper> ListWrapped
        {
            get
            {
                if (value == null)
                    value = new List<ValueWrapper>();
                return value;
            }
            set => this.value = value;
        }

        public override void SetValue(object obj)
        {
            if (obj == null)
            {
                ListWrapped.Clear();
                return;
            }

            if (obj.GetType().Equals(typeof(List<ValueWrapper>)))
                ListWrapped = (List<ValueWrapper>)obj;
            else
                RegenWrappedList((IList)obj);
        }

        private void RegenWrappedList(IList newValuedList)
        {
            var newList = new List<ValueWrapper>();
            Type elemType = ElementType;

            int i = 0;
            foreach (object item in newValuedList)
            {
                SerializedField field = new("value", elemType);
                field.SetValue(item);

                newList.Add(field.containerTarget);
                i++;
            }

            ListWrapped = newList;
        }

        public override void OnInspector(Rect rect, string label)
        {
            SerializedField serializedField = SerializedField;
            Rect headerRect = rect;
            var list = ListWrapped;

            // Draw foldout 
            headerRect.height = EditorGUIUtility.singleLineHeight - 1;
            foldout = EditorGUI.Foldout(headerRect, foldout, new GUIContent(label));

            // Draw list count
            headerRect.width = 80;
            headerRect.x += rect.width - headerRect.width;
            int newListSize = EditorGUI.DelayedIntField(headerRect, list.Count);
            if (newListSize != list.Count)
                UpdateListSize(newListSize);

            EditorGUI.indentLevel++;
            if (foldout)
            {
                Rect newRect = EditorGUI.IndentedRect(rect);
                newRect.y += EditorGUIUtility.singleLineHeight + 1;

                ReorderableList.DoList(newRect);
            }

            EditorGUI.indentLevel--;

            if (serializedField.SerializedObject.hasModifiedProperties)
                serializedField.SerializedObject.ApplyModifiedProperties();
        }

        public ReorderableList CreateReorderableList()
        {
            SerializedField serializedField = SerializedField;
            ReorderableList reorderableList = new(serializedField.SerializedObject, serializedField.SerializedProperty,
                true, false, true, true);
            reorderableList.onAddCallback = OnAddCallback;
            reorderableList.onRemoveCallback = OnRemoveCallback;
            reorderableList.drawElementCallback = DrawElementCallback;
            reorderableList.elementHeightCallback = ElementHeightCallback;

            return reorderableList;
        }

        private void OnAddCallback(ReorderableList reorderable)
        {
            var actualList = ListWrapped;
            Type elemType = ElementType;
            TypeMethods elemTypeMethods = AETManager.Instance.RetrieveTypeMethods(elemType);

            object newValue = actualList.Count > 0 && elemTypeMethods.CanBeCloned()
                ? actualList[^1].Unwrap()
                : DefaultSerializableTypes.GetDefaultElement(elemType);

            SerializedField field = new("value", elemType);
            field.SetValue(newValue);

            ListWrapped.Add(field.containerTarget);
            reorderable.serializedProperty.serializedObject.Update();
            reorderable.Select(reorderable.count - 1);
        }

        public void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < ListWrapped.Count)
            {
                ValueWrapper elemWrapped = ListWrapped[index];

                rect.y += 1f;
                rect.height = EditorGUI.GetPropertyHeight(elemWrapped.SerializedProperty, true);
                elemWrapped.OnInspector(rect, $"Element {index}");
            }
        }

        public float ElementHeightCallback(int index)
        {
            if (index < ListWrapped.Count)
            {
                SerializedField field = ListWrapped[index].SerializedField;
                if (field != null)
                    return AETManager.Instance.RetrieveTypeMethods(ElementType).GetPropertyFieldSize(ref field);
            }

            return EditorGUIUtility.singleLineHeight;
        }

        private void OnRemoveCallback(ReorderableList reorderable)
        {
            if (ListWrapped.Count == 0)
                return;

            if (reorderable.selectedIndices.Count > 0)
            {
                foreach (int index in reorderable.selectedIndices.OrderByDescending(x => x))
                    RemoveElement(index);
                reorderable.ClearSelection();
                return;
            }

            RemoveElement(ListWrapped.Count - 1);
            reorderable.Select(ListWrapped.Count - 1);
        }

        private void RemoveElement(int index)
        {
            var actualList = ListWrapped;
            if (actualList.Count > index)
                actualList.RemoveAt(index);
            SerializedField.SerializedObject.Update();
            ReorderableList.serializedProperty.serializedObject.Update();
        }

        internal void UpdateListSize(int newSize)
        {
            SerializedProperty prop = SerializedField.SerializedProperty;
            newSize = Mathf.Max(newSize, 0);
            ReorderableList reorderableList = ReorderableList;

            if (newSize > ListWrapped.Count)
                while (newSize > prop.arraySize)
                    OnAddCallback(reorderableList);
            else
                while (newSize < ListWrapped.Count)
                    RemoveElement(ListWrapped.Count - 1);
            reorderableList.Select(ListWrapped.Count - 1);
        }

        /*

        public override void OnInspector(Rect rect)
        {
            // Draw Foldout
            var headerRect = rect;
            headerRect.width = Mathf.Max(headerRect.width / 5, 50);
            foldout = EditorGUI.Foldout(rect, foldout, new GUIContent(SerializedField.DisplayName));

            // Draw Element Count
            var list = ListWrapped;

            headerRect.width = 80;
            headerRect.x += rect.width - headerRect.width;
            headerRect.height = EditorGUIUtility.singleLineHeight;
            var newListSize = EditorGUI.DelayedIntField(headerRect, list.Count);
            if (newListSize != list.Count)
                UpdateListSize(newListSize);

            // Draw elements if active
            if (foldout)
            {
                EditorGUI.indentLevel++;
                var newRect = rect;
                newRect.y += EditorGUIUtility.singleLineHeight * 1;
                ReorderableList.DoList(newRect);
                EditorGUI.indentLevel--;
            }
        }

        private void OnAddCallback(ReorderableList reorderable)
        {
            IList actualList = (IList)serializedField.GetValue();
            var elemType = ElementType;
            var elemTypeMethods = AETManager.Instance.RetrieveTypeMethods(elemType);
            var newElementField = new SerializedField($"element{actualList.Count}", elemType);

            object newValue = actualList.Count > 0 ?
                actualList[^1] : // TODO Clonning may be required here
                DefaultSerializableTypes.GetDefaultElement(elemType);

            newElementField.SetValue(newValue);
            reorderable.list.Add(newElementField);
            actualList.Add(newValue);
            reorderable.Select(reorderableList.count - 1);
        }

        private void OnRemoveCallback(ReorderableList reorderable)
        {
            foreach (var index in reorderable.selectedIndices)
            {
                RemoveElement(reorderable, index);
            }
        }

        public float ElementHeightCallback(int index)
        {
            var elemTypeMethods = AETManager.Instance.RetrieveTypeMethods(ElementType);
            var valueRef = list[index];
            return elemTypeMethods.GetPropertyFieldSize(ref valueRef);
        }

        public void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            IList actualList = (IList)serializedField.GetValue();

            if(actualList.Count > index)
            {
                var elemField = list[index];
                elemField.SetValue(actualList[index]);

                var elemTypeMethods = AETManager.Instance.RetrieveTypeMethods(ElementType);
                rect.y += 1f;
                rect.height = elemTypeMethods.GetPropertyFieldSize(ref elemField);
                elemField.OnInspector(rect);
                actualList[index] = elemField.GetValue();
            }
            else
                reorderableList.list.RemoveAt(index);
        }

        */
    }
}
#endif