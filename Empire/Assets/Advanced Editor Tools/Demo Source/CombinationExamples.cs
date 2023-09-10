using AdvancedEditorTools.Attributes;
using UnityEngine;

namespace AdvancedEditorTools
{
    public class CombinationExamples : MonoBehaviour
    {
        // This example shows the complexity of a layout that can
        // be achieved with these attributes
        [BeginFoldout("Coords")]
        [BeginColumnArea(columnStyle = LayoutStyle.Bevel)]
        [TextArea(5, 6)]
        public string rules;
        [NewEmptyColumn]
        [NewColumn(columnWidth: 0.4f, columnStyle: LayoutStyle.None)]
        [BeginColumnArea(areaStyle = LayoutStyle.BevelOrange, columnStyle = LayoutStyle.None)]
        public bool A1;
        public bool A2;
        public bool A3;
        public bool A4;
        [NewColumn]
        public bool B1;
        public bool B2;
        public bool B3;
        public bool B4;
        [NewColumn]
        public bool C1;
        public bool C2;
        public bool C3;
        public bool C4;
        [NewColumn]
        public bool D1;
        public bool D2;
        public bool D3;
        public bool D4;
        [EndColumnArea]
        [EndColumnArea]
        [EndFoldout]


        // #################################
        // #################################
        // #################################


        // With this precise configuration you can hide the second column
        // by wrapping it inside a foldout scope. However, beware. Mixing
        // columns and foldouts scopes can become convoluted and can lead
        // to layouting errors. Handle your scopes with caution.
        [BeginFoldout("Columns within foldouts")]
        [BeginColumnArea]
        public Color color1;
        public Color color2;
        [BeginFoldout("Second column hidden")]

        [NewColumn]
        public Color color3;
        public Color color4;
        [EndFoldout]
        [EndColumnArea]

        // You can create collapsable columns by including all of its contents
        // inside a foldout.
        // Collapsable columns that do not define a strict width will be
        // automatically adjusted to their available space. Try collapsing
        // and expanding these two columns
        [BeginColumnArea]
        [BeginFoldout("Other color sets")]
        public Color color5;
        public Color color6;
        [EndFoldout]

        [NewColumn]
        [BeginFoldout("More color sets")]
        public Color color7;
        [EndFoldout(includeLast = true)]
        [EndColumnArea(includeLast = true)]
        [EndFoldout(includeLast = true)]
        public Color color8;
    }
}
