#if UNITY_EDITOR
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class LayerMaskValueWrapper : GenericValueWrapper<LayerMask>
    {
        public override void SetValue(object obj)
        {
            if (obj.GetType().Equals(typeof(LayerMask)))
            {
                value = ((LayerMask)obj).value;
            }
            else
            {
                value = (int)obj;
            }
        }
    }
}
#endif