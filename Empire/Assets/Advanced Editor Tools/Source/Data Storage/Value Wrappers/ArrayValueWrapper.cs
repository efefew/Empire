#if UNITY_EDITOR
using System;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class ArrayValueWrapper : EnumerableValueWrapper
    {
        public override ValueWrapper Init(Type type)
        {
            this.elementTypeName = type.GetElementType().AssemblyQualifiedName;
            return this;
        }

        public override object Unwrap()
        {
            var listWrapped = ListWrapped;
            var elementType = Type.GetType(elementTypeName);
            var valuedList = Array.CreateInstance(elementType, listWrapped.Count);
            for (int i = 0; i < listWrapped.Count; i++)
                valuedList.SetValue(listWrapped[i].Unwrap() ?? default, i);

            return valuedList;
        }
    }
}
#endif