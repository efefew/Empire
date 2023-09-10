using AdvancedEditorTools.Attributes;
using UnityEngine;

namespace AdvancedEditorTools
{
    public class OtherExamples : MonoBehaviour
    {
        // This attribute draws a horizontal line before the field is
        // drawn on the inspector.
        public int myVariable;
        [LineSeparator]
        public int myOtherVariable;

        // The ReadOnly attribute keeps a variable visible in the inspector
        // but it cannot be manually changed.
        [ReadOnly]
        public string importantMessage = "Do not change me";

    }
}
