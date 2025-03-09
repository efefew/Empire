#region

using System;

#endregion

#if UNITY_EDITOR
namespace AdvancedEditorTools
{
    [Serializable]
    public class IntValueWrapper : GenericValueWrapper<int>
    {
    }
}
#endif