using AdvancedEditorTools.Attributes;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [CustomPropertyDrawer(typeof(LineSeparatorAttribute))]
    public class LineSeparatorPropertyDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect rect)
        {
            rect.y += rect.height / 2.0f;
            Color prevColor = Handles.color;
            Handles.color = GUI.backgroundColor;
            Handles.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin));
            Handles.color = prevColor;
        }

        public override float GetHeight()
        {
            return ((LineSeparatorAttribute)attribute).spacing;
        }
    }
}