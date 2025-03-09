#if UNITY_EDITOR

#region

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class SerializedField : IEnumerableMatcheable<SerializedField>
    {
        public string fieldName;

        [SerializeField] private string fieldType;

        public bool isUsingRealContainer = true;
        public bool isUsingRealTarget = true;

        [SerializeReference] public Object target;

        [SerializeReference] public ValueWrapper containerTarget;


        private SerializedObject _serializedObject;

        private SerializedProperty _serializedProperty;

        public SerializedField(Object target, string fieldName, Type fieldType,
            SerializedObject serializedObject = null)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType.AssemblyQualifiedName;
            this.target = target;

            SerializedProperty property;
            _serializedObject = serializedObject;
            property = SerializedObject.FindProperty(fieldName);

            TypeMethods fieldTypeMethods = AETManager.Instance.RetrieveTypeMethods(fieldType);
            if (property == null) // || fieldTypeMethods.TypeMustBeWrapped(fieldType))
            {
                isUsingRealContainer = false;
                _serializedObject = null;
                ValueWrapper valueWrapper = fieldTypeMethods.GetValueWrapper(fieldType);
                FieldInfo fieldInfo = target.GetType().GetField(fieldName);
                valueWrapper.SetValue(fieldInfo.GetValue(target));

                containerTarget = valueWrapper;
                valueWrapper.SerializedField = this;
            }
            else
            {
                _serializedProperty = property;
            }
        }

        public SerializedField(string fieldName, Type fieldType, ValueWrapper wrapper = null)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType.AssemblyQualifiedName;

            containerTarget = wrapper == null || wrapper.Equals(null)
                ? AETManager.Instance.RetrieveTypeMethods(fieldType).GetValueWrapper(fieldType)
                : wrapper;

            containerTarget.SerializedField = this;
            target = containerTarget;
            isUsingRealContainer = false;
            isUsingRealTarget = false;
            containerTarget.SerializedObject.Update();
        }

        public string DisplayName => ObjectNames.NicifyVariableName(fieldName);

        //if (_displayName == null ||_displayName.Length == 0)
        //    _displayName = ObjectNames.NicifyVariableName(fieldName);
        //return _displayName;
        public Type Type => Type.GetType(fieldType);

        public SerializedObject SerializedObject
        {
            get
            {
                if (_serializedObject == null || _serializedObject.Equals(null) ||
                    _serializedObject.targetObject == null || _serializedObject.targetObject.Equals(null))
                    _serializedObject = new SerializedObject(isUsingRealContainer ? target : containerTarget);

                return _serializedObject;
            }
            set
            {
                _serializedObject = value;
                _serializedProperty = null;
            }
        }

        public SerializedProperty SerializedProperty
        {
            get
            {
                if (_serializedProperty == null || _serializedProperty.Equals(null) ||
                    _serializedProperty.serializedObject == null || _serializedProperty.serializedObject.Equals(null))
                    _serializedProperty = SerializedObject.FindProperty(isUsingRealContainer ? fieldName : "value");
                return _serializedProperty;
            }
        }

        public bool Matches(SerializedField obj)
        {
            if (obj == null) return false;
            return fieldName == obj.fieldName && Type == obj.Type;
        }

        public bool PartiallyMatches(SerializedField obj)
        {
            if (obj == null) return false;

            return fieldType.Equals(obj.fieldType);
        }

        public SerializedField UpdateWith(SerializedField obj)
        {
            if (obj.isUsingRealContainer == isUsingRealContainer && !isUsingRealContainer)
                containerTarget = containerTarget.UpdateWrapper(obj.containerTarget);

            isUsingRealTarget = obj.isUsingRealTarget;
            target = obj.target;

            return this;
        }

        public object GetValue()
        {
            if (isUsingRealTarget)
            {
                FieldInfo fieldInfo = target.GetType().GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                return fieldInfo.GetValue(target);
            }

            return containerTarget.Unwrap();
        }

        public void SetValue(object value)
        {
            if (isUsingRealTarget)
            {
                FieldInfo fieldInfo = target.GetType().GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(target, value);
            }

            if (!isUsingRealContainer)
            {
                containerTarget.SetValue(value);
                containerTarget.SerializedObject.Update();
            }
        }

        public void OnInspector(Rect? rect = null)
        {
            TypeMethods typeMethods = AETManager.Instance.RetrieveTypeMethods(Type);
            SerializedField thisRef = this;

            SerializedObject so = SerializedObject;
            so.Update();
            typeMethods.OnInspector(ref thisRef, rect);

            if (!isUsingRealContainer && isUsingRealTarget)
            {
                FieldInfo fieldInfo = target.GetType().GetField(fieldName);
                fieldInfo.SetValue(target, containerTarget.Unwrap());
            }

            if (so.hasModifiedProperties)
                so.ApplyModifiedProperties();
        }
    }
}
#endif