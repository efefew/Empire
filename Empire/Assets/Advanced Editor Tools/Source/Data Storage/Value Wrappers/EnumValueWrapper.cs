#if UNITY_EDITOR

#region

using System;
using UnityEditor;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class EnumValueWrapper : GenericValueWrapperReference<Enum>
    {
        public string typeName;
        public string enumVal;

        public Type _enumType;

        public Type EnumType
        {
            get
            {
                if (_enumType == null)
                    _enumType = Type.GetType(typeName);
                return _enumType;
            }
            set => _enumType = value;
        }

        public override ValueWrapper Init(Type type)
        {
            EnumType = type;
            typeName = type.AssemblyQualifiedName;
            enumVal = Enum.GetNames(type)[0];
            return this;
        }

        public override void SetValue(object obj)
        {
            if (obj is string name)
                enumVal = name;
            else
                enumVal = ((Enum)obj).ToString();
        }

        public override object Unwrap()
        {
            return Enum.Parse(EnumType, enumVal);
        }

        public override void OnInspector(Rect rect, string label)
        {
            SerializedField serializedField = SerializedField;
            Enum oldVal = (Enum)Unwrap();
            Enum newVal = EditorGUI.EnumPopup(rect, label, oldVal);

            if (!oldVal.Equals(newVal)) serializedField.SetValue(newVal);
        }
    }
}
#endif