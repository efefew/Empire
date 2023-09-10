using System;

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string buttonName;
        public int buttonSize;

        /// <param name="buttonName">Text displayed on top of the button</param>
        /// <param name="fontSize">Font size of the button's name</param>
        public ButtonAttribute(string buttonName, int fontSize = 12)
        {
            this.buttonName = buttonName;
            this.buttonSize = fontSize;
        }
    }
}