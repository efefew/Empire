#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class ListValueWrapper : EnumerableValueWrapper
    {
        public override ValueWrapper Init(Type type)
        {
            elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName;
            return this;
        }
        public override object Unwrap()
        {
            var elementType = Type.GetType(elementTypeName);
            var listType = typeof(List<>).MakeGenericType(elementType);
            var valuedList = (IList)Activator.CreateInstance(listType);
            foreach (var wrapper in ListWrapped)
                valuedList.Add(wrapper.Unwrap());
            return valuedList;
        }
    }
}
#endif