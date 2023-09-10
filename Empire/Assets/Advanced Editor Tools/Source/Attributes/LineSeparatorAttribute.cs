using System;
using UnityEngine;

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LineSeparatorAttribute : PropertyAttribute
    {
        public int spacing;

        /// <param name="spacing">Space in pixels between the previous property and the next one. The line will be painted in the middle.</param>
        public LineSeparatorAttribute(int spacing = 10) { this.spacing = spacing; }
    }
}