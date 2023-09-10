using System;

namespace AdvancedEditorTools
{
    [Serializable]
    public class BeginFoldoutInfo : LayoutInfo
    {
        public string label;
        public bool foldout;

        public override bool Matches(LayoutInfo obj)
        {
            if (obj is BeginFoldoutInfo oldObj)
                return base.Matches(obj) && this.label == oldObj.label;
            return false;
        }

        public override bool PartiallyMatches(LayoutInfo obj)
        {
            if (obj is BeginFoldoutInfo oldObj)
                return LayoutTypeMatches(obj) && (this.fieldName.Equals(obj.fieldName) || this.label == oldObj.label);
            return false;
        }

        public override LayoutInfo UpdateWith(LayoutInfo obj)
        {
            if (obj is BeginFoldoutInfo oldObj)
                this.foldout = oldObj.foldout;
            return this;
        }
    }

    [Serializable]
    public class EndFoldoutInfo : LayoutInfo
    {
        public bool includeLast;
    }
}
