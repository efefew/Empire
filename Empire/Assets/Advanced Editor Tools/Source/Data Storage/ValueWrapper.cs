#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public abstract class ValueWrapper : ScriptableObject
    {
        public SerializedField SerializedField;

        private SerializedObject _serializedObject;
        public SerializedObject SerializedObject
        {
            get
            {
                if (_serializedObject == null)
                    _serializedObject = new SerializedObject(this);
                return _serializedObject;
            }
        }

        private SerializedProperty _serializedProperty;
        public SerializedProperty SerializedProperty
        {
            get
            {
                if (_serializedProperty == null)
                    _serializedProperty = SerializedObject.FindProperty("value");
                return _serializedProperty;
            }
            set => _serializedProperty = value;
        }

        public virtual ValueWrapper Init(System.Type type) => this;

        public abstract object GetValue();
        public virtual object Unwrap()
        {
            if (SerializedField != null && SerializedField.isUsingRealTarget)
                return SerializedField.GetValue();

            return Clone();
        }

        public abstract void SetValue(object obj);

        public virtual object Clone() => GetValue();

        public void OnInspector(Rect rect)
        {
            OnInspector(rect, SerializedField.DisplayName);
        }

        public virtual void OnInspector(Rect rect, string label)
        {
            EditorGUI.PropertyField(rect, SerializedProperty, new GUIContent(label), true);
            if (SerializedProperty.serializedObject.hasModifiedProperties)
                SerializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public ValueWrapper UpdateWrapper(ValueWrapper oldWrapper)
        {
            oldWrapper.SerializedField = this.SerializedField;
            return oldWrapper;
        }
    }
    [System.Serializable]
    public class GenericValueWrapper<T> : ValueWrapper
    {
        public T value;

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object obj)
        {
            if (obj != null)
                value = (T)obj;
            else value = default;
        }
    }
    [System.Serializable]
    public class GenericValueWrapperReference<T> : ValueWrapper
    {
        [SerializeReference]
        public T value;

        public override object GetValue()
        {
            return value;
        }
        public override void SetValue(object obj)
        {
            if (obj != null)
                value = (T)obj;
            else value = default;
        }
    }

    //////////////////////////////////////////

    /*
    [System.Serializable]
    public class CustomTypeValueWrapper : GenericValueWrapper<object>
    {
        public string typeName;
        [SerializeReference]
        public List<SerializedField> fields;
        public bool foldout;
        public bool isInstantiated;
        public Type Type { get => Type.GetType(typeName); }
        public CustomTypeValueWrapper(Type type, object customValue) : base(customValue)
        {
            typeName = type.AssemblyQualifiedName;
            foldout = true;
            SetValue(customValue);
        }

        public override void SetValue(object obj)
        {
            if (obj is null || obj.Equals(null))
            {
                fields = null; 
                isInstantiated = false;
            }
            else if (obj.GetType().Equals(typeof(List<SerializedField>)))
            {
                var newFields = GenerateDefaultFields();
                var oldFields = (List<SerializedField>)obj;

                if(oldFields.Count == 0 && !isInstantiated)
                {
                    fields = null;
                }
                else
                {
                    fields =  newFields.MatchEnumerables(oldFields, true).ToList();
                    isInstantiated = true;
                }                
            }
            else
            {
                fields = new();
                foreach (var fieldInfo in GetFields())
                    fields.Add(new SerializedField(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.GetValue(obj)));
                isInstantiated = true;
            }            
        }

        public List<SerializedField> GenerateDefaultFields()
        {
            var newFields = new List<SerializedField>();
            foreach (var fieldInfo in GetFields())
                newFields.Add(new SerializedField(fieldInfo.Name, fieldInfo.FieldType, DefaultSystem.SerializableTypes.GetDefaultElement(fieldInfo.FieldType)));
            return newFields;
        }

        public override ValueWrapper UpdateWrapper(ValueWrapper oldWrapper)
        {
            var oldCustomTypeWrapper = oldWrapper as CustomTypeValueWrapper;
            if (oldCustomTypeWrapper != null)
            {
                this.SetValue(oldCustomTypeWrapper.fields);
                this.foldout = oldCustomTypeWrapper.foldout;
                this.typeName = oldCustomTypeWrapper.typeName;
                this.isInstantiated = oldCustomTypeWrapper.isInstantiated;
            }
            else
                throw new TypeAccessException(); // Debug.Log("Error");
            return this;
        }

        public override object GetValue()
        {
            if (fields is null || fields.Count == 0 && !isInstantiated) return null; 

            var instance = Activator.CreateInstance(Type);            
            var newFieldsInfos = GetFields();
            var matchedFields = GenerateDefaultFields().MatchEnumerables(fields, true).ToList();
            var i = 0;
            foreach (var fieldInfo in newFieldsInfos)
            {
                var fieldValue = matchedFields[i].valueWrapper.GetValue();
                fieldInfo.SetValue(instance, fieldValue is null || fieldValue.Equals(null) ? null : fieldValue);
                i++;
            }

            return instance;
        }

        public List<SerializedField> GetFieldsUpdated()
        {
            if (fields is null) return new List<SerializedField>(); 
            fields = GenerateDefaultFields().MatchEnumerables(fields, true).ToList();
            return fields;
        }

        public object GetClone()
        {
            if (fields == null) return null;

            var instance = Activator.CreateInstance(Type);            
            var newFieldsInfos = GetFields();
            var matchedFields = GenerateDefaultFields().MatchEnumerables(fields, true).ToList();
            var manager = AEAManager.Instance;
            var i = 0;
            foreach (var fieldInfo in newFieldsInfos)
            {
                var elemTypeMethdos = manager.RetrieveTypeMethods(fieldInfo.FieldType);
                var valueClone = elemTypeMethdos.Clone(matchedFields[i].valueWrapper);
                fieldInfo.SetValue(instance, valueClone is null || valueClone.Equals(null) ? null : valueClone);
                i++;
            }

            return instance;
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            return Type.GetFields()
                    .Concat(Type
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(field => field.GetCustomAttribute<SerializeField>() != null ||
                                        field.GetCustomAttribute<SerializeReference>() != null));
        }

        public void CreateDefaultInstance()
        {
            fields = GenerateDefaultFields();
            isInstantiated = true;
        }
    }

    [System.Serializable]
    public class StructValueWrapper : CustomTypeValueWrapper
    { public StructValueWrapper(Type type, object structValue) : base(type, structValue) { } }

    [System.Serializable]
    public class ClassValueWrapper : CustomTypeValueWrapper
    { public ClassValueWrapper(Type type, object classValue) : base(type, classValue) { } }
    
    */
}
#endif