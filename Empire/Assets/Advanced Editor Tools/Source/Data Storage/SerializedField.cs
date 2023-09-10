#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedEditorTools
{
    [Serializable]
    public class SerializedField : IEnumerableMatcheable<SerializedField>
    {
        public string fieldName;
        public string DisplayName
        {
            get
            {
                return ObjectNames.NicifyVariableName(fieldName);

                //if (_displayName == null ||_displayName.Length == 0)
                //    _displayName = ObjectNames.NicifyVariableName(fieldName);
                //return _displayName;
            }
        }
        [SerializeField]
        private string fieldType;
        public Type Type => Type.GetType(fieldType);
        public bool isUsingRealContainer = true;
        public bool isUsingRealTarget = true;
        [SerializeReference]
        public Object target;
        [SerializeReference]
        public ValueWrapper containerTarget;


        private SerializedObject _serializedObject;
        public SerializedObject SerializedObject
        {
            get
            {
                if (_serializedObject == null || _serializedObject.Equals(null) || _serializedObject.targetObject == null || _serializedObject.targetObject.Equals(null))
                    _serializedObject = new SerializedObject(isUsingRealContainer ? target : containerTarget);

                return _serializedObject;
            }
            set
            {
                _serializedObject = value;
                _serializedProperty = null;
            }
        }

        private SerializedProperty _serializedProperty;
        public SerializedProperty SerializedProperty
        {
            get
            {
                if (_serializedProperty == null || _serializedProperty.Equals(null) || _serializedProperty.serializedObject == null || _serializedProperty.serializedObject.Equals(null))
                    _serializedProperty = SerializedObject.FindProperty(isUsingRealContainer ? fieldName : "value");
                return _serializedProperty;
            }
        }

        public SerializedField(Object target, string fieldName, Type fieldType, SerializedObject serializedObject = null)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType.AssemblyQualifiedName;
            this.target = target;

            SerializedProperty property;
            _serializedObject = serializedObject;
            property = SerializedObject.FindProperty(fieldName);

            var fieldTypeMethods = AETManager.Instance.RetrieveTypeMethods(fieldType);
            if (property == null) // || fieldTypeMethods.TypeMustBeWrapped(fieldType))
            {
                isUsingRealContainer = false;
                _serializedObject = null;
                var valueWrapper = fieldTypeMethods.GetValueWrapper(fieldType);
                var fieldInfo = target.GetType().GetField(fieldName);
                valueWrapper.SetValue(fieldInfo.GetValue(target));

                this.containerTarget = valueWrapper;
                valueWrapper.SerializedField = this;
            }
            else
                _serializedProperty = property;
        }

        public SerializedField(string fieldName, Type fieldType, ValueWrapper wrapper = null)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType.AssemblyQualifiedName;

            containerTarget = wrapper == null || wrapper.Equals(null) ?
                AETManager.Instance.RetrieveTypeMethods(fieldType).GetValueWrapper(fieldType) :
                wrapper;

            containerTarget.SerializedField = this;
            target = containerTarget;
            isUsingRealContainer = false;
            isUsingRealTarget = false;
            containerTarget.SerializedObject.Update();
        }

        public object GetValue()
        {
            if (isUsingRealTarget)
            {
                var fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                return fieldInfo.GetValue(target);
            }

            return containerTarget.Unwrap();
        }

        public void SetValue(object value)
        {
            if (isUsingRealTarget)
            {
                var fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
            var typeMethods = AETManager.Instance.RetrieveTypeMethods(Type);
            var thisRef = this;

            var so = SerializedObject;
            so.Update();
            typeMethods.OnInspector(ref thisRef, rect);

            if (!isUsingRealContainer && isUsingRealTarget)
            {
                var fieldInfo = target.GetType().GetField(fieldName);
                fieldInfo.SetValue(target, containerTarget.Unwrap());
            }
            if (so.hasModifiedProperties)
                so.ApplyModifiedProperties();
        }

        public bool Matches(SerializedField obj)
        {
            if (obj == null) return false;
            return this.fieldName == obj.fieldName && this.Type == obj.Type;
        }
        public bool PartiallyMatches(SerializedField obj)
        {
            if (obj == null) return false;

            return this.fieldType.Equals(obj.fieldType);
        }
        public SerializedField UpdateWith(SerializedField obj)
        {
            if (obj.isUsingRealContainer == this.isUsingRealContainer && !isUsingRealContainer)
            {
                this.containerTarget = containerTarget.UpdateWrapper(obj.containerTarget);
            }

            this.isUsingRealTarget = obj.isUsingRealTarget;
            this.target = obj.target;

            return this;
        }
    }
}
#endif