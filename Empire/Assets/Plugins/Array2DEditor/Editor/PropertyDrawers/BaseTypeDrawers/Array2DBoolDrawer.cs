using UnityEditor;
using UnityEngine;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(Array2DBool))]
    public class Array2DBoolDrawer : Array2DDrawer
    {
        protected override Vector2Int GetDefaultCellSizeValue()
        {
            return new Vector2Int(16, 16);
        }

        protected override object GetDefaultCellValue()
        {
            return false;
        }

        protected override object GetCellValue(SerializedProperty cell)
        {
            return cell.boolValue;
        }

        protected override void SetValue(SerializedProperty cell, object obj)
        {
            cell.boolValue = (bool)obj;
        }
    }
}