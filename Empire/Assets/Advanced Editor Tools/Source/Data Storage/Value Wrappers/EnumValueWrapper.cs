#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
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
            this.EnumType = type;
            this.typeName = type.AssemblyQualifiedName;
            this.enumVal = Enum.GetNames(type)[0];
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
            var serializedField = SerializedField;
            var oldVal = (Enum)Unwrap();
            var newVal = EditorGUI.EnumPopup(rect, label, oldVal);

            if (!oldVal.Equals(newVal))
            {
                serializedField.SetValue(newVal);
            }
        }
    }
}
#endif