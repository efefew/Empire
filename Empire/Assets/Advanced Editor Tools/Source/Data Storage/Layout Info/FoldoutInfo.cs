#region

using System;

#endregion

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
                return base.Matches(obj) && label == oldObj.label;
            return false;
        }

        public override bool PartiallyMatches(LayoutInfo obj)
        {
            if (obj is BeginFoldoutInfo oldObj)
                return LayoutTypeMatches(obj) && (fieldName.Equals(obj.fieldName) || label == oldObj.label);
            return false;
        }

        public override LayoutInfo UpdateWith(LayoutInfo obj)
        {
            if (obj is BeginFoldoutInfo oldObj)
                foldout = oldObj.foldout;
            return this;
        }
    }

    [Serializable]
    public class EndFoldoutInfo : LayoutInfo
    {
        public bool includeLast;
    }
}