using AdvancedEditorTools.Attributes;
using UnityEngine;

namespace AdvancedEditorTools
{
    public class ColumnsExample : MonoBehaviour
    {
        // To begin a column section use [BeginColumnArea]
        // The following fields will belong to the 1st column
        [Header("Health")]
        [BeginColumnArea(columnWidth: 0.25f)]
        public int health;
        public int maxHealth;

        // To jump to the next column use [NewColumn]
        [Header("Attack")]
        [NewColumn(columnWidth: 0.45f)]
        public int attackDmg;
        [Range(0, 1)]
        public float critProp;
        public int secondaryAttackDmg;
        public int magicAttackDmg;

        // To finalize a column section you can use [EndColumnArea]
        [Header("Defense")]
        [NewColumn]
        public int baseDefense;
        public int magicDefense;
        public int fireDefense;
        [EndColumnArea]


        // #################################
        // #################################
        // #################################


        // You can also nest column sections, and define the proportion of the
        // inspector window width they will occupy. Make sure that the addition
        // of the proportions you write do not surpass 1
        [BeginColumnArea(columnWidth: 0.5f)]
        // You can change the style of the column area and each individual column 
        // with the styles provided in the LayoutStyle enum.
        // If you define a column style in the BeginColumnArea attribute it will
        // be applied to all the columns in the group.
        [BeginColumnArea(columnWidth: 0.225f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.Bevel)]
        public int test1;
        [NewColumn(columnWidth: 0.225f)]
        public int test2;
        [EndColumnArea]
        public int test3;
        // You can introduce empty columns to create space between columns
        [NewEmptyColumn(0.15f)]
        // You can change the default style of the column individually
        [NewColumn(columnStyle: LayoutStyle.BevelCyan)]
        public int test4;
        // Notice the "includeLast = true" flag. This is required when there are
        // no other fields left in the script
        [EndColumnArea(includeLast = true)]
        public int test5;

    }
}
