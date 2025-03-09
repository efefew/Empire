#if UNITY_EDITOR

#region

using System;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class ArrayValueWrapper : EnumerableValueWrapper
    {
        public override ValueWrapper Init(Type type)
        {
            elementTypeName = type.GetElementType().AssemblyQualifiedName;
            return this;
        }

        public override object Unwrap()
        {
            var listWrapped = ListWrapped;
            Type elementType = Type.GetType(elementTypeName);
            Array valuedList = Array.CreateInstance(elementType, listWrapped.Count);
            for (int i = 0; i < listWrapped.Count; i++)
                valuedList.SetValue(listWrapped[i].Unwrap() ?? default, i);

            return valuedList;
        }
    }
}
#endif