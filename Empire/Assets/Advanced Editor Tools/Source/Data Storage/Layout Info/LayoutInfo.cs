using System;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public abstract class LayoutInfo : IEnumerableMatcheable<LayoutInfo>
    {
        public string fieldName;
        public string fieldTypeName;
        public Type FieldType => Type.GetType(fieldTypeName);

        public virtual bool Matches(LayoutInfo obj) => LayoutTypeMatches(obj) && this.fieldName.Equals(obj.fieldName) && this.fieldTypeName.Equals(obj.fieldTypeName);
        public virtual bool PartiallyMatches(LayoutInfo obj) => LayoutTypeMatches(obj) && this.fieldName.Equals(obj.fieldName);
        public virtual LayoutInfo UpdateWith(LayoutInfo obj) => this;

        protected bool LayoutTypeMatches(LayoutInfo obj) => this.GetType().Equals(obj.GetType());
    }
}
