
namespace AdvancedEditorTools.Attributes
{
    public abstract class ColumnAttribute : LayoutAttribute { }
    public class BeginColumnAreaAttribute : ColumnAttribute
    {
        /// <summary>
        /// Style this columns container will display
        /// </summary>
        public LayoutStyle areaStyle = LayoutStyle.None;
        /// <summary>
        /// Style that every column that belong to this group will display
        /// </summary>
        public LayoutStyle columnStyle = LayoutStyle.Box;
        public float? columnWidth = null;

        public BeginColumnAreaAttribute() { }
        /// <param name="columnWidth">Proportion of the inspector width the column will fill, in range [0,1]</param>
        public BeginColumnAreaAttribute(float columnWidth)
        {
            this.columnWidth = columnWidth;
        }
    }
    public class NewColumnAttribute : ColumnAttribute
    {
        public LayoutStyle? columnStyle = null;
        public float? columnWidth = null;

        public NewColumnAttribute() { }
        /// <param name="columnStyle">Style this column will display</param>
        public NewColumnAttribute(LayoutStyle columnStyle)
        {
            this.columnStyle = columnStyle;
        }

        /// <param name="columnWidth">Proportion of the inspector width the column will fill, in range [0,1]</param>
        public NewColumnAttribute(float columnWidth)
        {
            this.columnWidth = columnWidth;
        }

        /// <param name="columnWidth">Proportion of the inspector width the column will fill, in range [0,1]</param>
        /// <param name="columnStyle">Style this column will display</param>
        public NewColumnAttribute(float columnWidth, LayoutStyle columnStyle)
        {
            this.columnWidth = columnWidth;
            this.columnStyle = columnStyle;
        }
    }
    public class NewEmptyColumnAttribute : ColumnAttribute
    {
        public float columnWidth = 0.1f;
        public NewEmptyColumnAttribute() { }
        /// <param name="columnWidth">Proportion of the inspector width the column will fill, in range [0,1]</param>
        public NewEmptyColumnAttribute(float columnWidth)
        {
            this.columnWidth = columnWidth;
        }
    }

    public class EndColumnAreaAttribute : ColumnAttribute
    {
        public bool includeLast = false;
    }
}
