﻿using UnityEditor;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(Array2DFloat))]
    public class Array2DFloatDrawer : Array2DDrawer
    {
        protected override object GetDefaultCellValue()
        {
            return 0f;
        }

        protected override object GetCellValue(SerializedProperty cell)
        {
            return cell.floatValue;
        }

        protected override void SetValue(SerializedProperty cell, object obj)
        {
            cell.floatValue = (float)obj;
        }
    }
}