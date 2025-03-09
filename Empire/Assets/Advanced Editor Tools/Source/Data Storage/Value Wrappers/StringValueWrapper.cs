#region

using System;

#endregion

#if UNITY_EDITOR
namespace AdvancedEditorTools
{
    [Serializable]
    public class StringValueWrapper : GenericValueWrapper<string>
    {
    }
}
#endif