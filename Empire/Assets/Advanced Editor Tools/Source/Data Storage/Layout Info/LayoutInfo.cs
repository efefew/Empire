#region

using System;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public abstract class LayoutInfo : IEnumerableMatcheable<LayoutInfo>
    {
        public string fieldName;
        public string fieldTypeName;
        public Type FieldType => Type.GetType(fieldTypeName);

        public virtual bool Matches(LayoutInfo obj)
        {
            return LayoutTypeMatches(obj) && fieldName.Equals(obj.fieldName) && fieldTypeName.Equals(obj.fieldTypeName);
        }

        public virtual bool PartiallyMatches(LayoutInfo obj)
        {
            return LayoutTypeMatches(obj) && fieldName.Equals(obj.fieldName);
        }

        public virtual LayoutInfo UpdateWith(LayoutInfo obj)
        {
            return this;
        }

        protected bool LayoutTypeMatches(LayoutInfo obj)
        {
            return GetType().Equals(obj.GetType());
        }
    }
}