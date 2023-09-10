#if UNITY_EDITOR

namespace AdvancedEditorTools
{
    public class ClassValueWrapper : GenericValueWrapperReference<object>
    {
        public bool isInstantiated = false;
    }
}
#endif