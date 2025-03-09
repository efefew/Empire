#if UNITY_EDITOR

#region

using System;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class GradientValueWrapper : GenericValueWrapperReference<Gradient>
    {
        public override object Clone()
        {
            if (value == null)
                return new Gradient();
            return new Gradient
            {
                alphaKeys = value.alphaKeys.Clone() as GradientAlphaKey[],
                colorKeys = value.colorKeys.Clone() as GradientColorKey[],
                colorSpace = value.colorSpace,
                mode = value.mode
            };
        }
    }
}
#endif