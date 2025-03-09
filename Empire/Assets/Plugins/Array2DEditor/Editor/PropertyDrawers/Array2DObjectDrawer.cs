using UnityEditor;
using UnityEngine;

namespace Array2DEditor
{
    public abstract class Array2DObjectDrawer<T> : Array2DDrawer where T : Object
    {
        protected override Vector2Int GetDefaultCellSizeValue()
        {
            return new Vector2Int(64, 64);
        }

        protected override object GetDefaultCellValue()
        {
            return null;
        }

        protected override object GetCellValue(SerializedProperty cell)
        {
            return cell.objectReferenceValue;
        }

        protected override void SetValue(SerializedProperty cell, object obj)
        {
            cell.objectReferenceValue = (T)obj;
        }
    }
}