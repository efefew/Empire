using AdvancedEditorTools.Attributes;
using System;

namespace AdvancedEditorTools
{
    [Serializable]
    public abstract class ColumnInfo : LayoutInfo { }

    [Serializable]
    public class BeginColumnAreaInfo : ColumnInfo
    {
        public LayoutStyle windowStyle;
        public LayoutStyle columnStyle;
        public float? columnWidth;
    }

    [Serializable]
    public class NewColumnInfo : ColumnInfo
    {
        public LayoutStyle? columnStyle;
        public float? columnWidth;
    }

    [Serializable]
    public class NewEmptyColumnInfo : ColumnInfo
    {
        public float columnWidth;
    }

    [Serializable]
    public class EndColumnAreaInfo : ColumnInfo
    {
        public bool includeLast;
    }
}
