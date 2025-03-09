using UnityEditor;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(Array2DInt))]
    public class Array2DIntDrawer : Array2DDrawer
    {
        protected override object GetDefaultCellValue()
        {
            return 0;
        }

        protected override object GetCellValue(SerializedProperty cell)
        {
            return cell.intValue;
        }

        protected override void SetValue(SerializedProperty cell, object obj)
        {
            cell.intValue = (int)obj;
        }
    }
}