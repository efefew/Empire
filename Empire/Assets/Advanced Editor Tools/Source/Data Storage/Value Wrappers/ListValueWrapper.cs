#if UNITY_EDITOR

#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class ListValueWrapper : EnumerableValueWrapper
    {
        public override ValueWrapper Init(Type type)
        {
            elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName;
            return this;
        }

        public override object Unwrap()
        {
            Type elementType = Type.GetType(elementTypeName);
            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList valuedList = (IList)Activator.CreateInstance(listType);
            foreach (ValueWrapper wrapper in ListWrapped)
                valuedList.Add(wrapper.Unwrap());
            return valuedList;
        }
    }
}
#endif