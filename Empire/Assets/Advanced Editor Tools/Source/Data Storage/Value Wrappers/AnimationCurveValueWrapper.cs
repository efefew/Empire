#if UNITY_EDITOR
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class AnimationCurveValueWrapper : GenericValueWrapperReference<AnimationCurve>
    {
        public override object Clone()
        {
            if (value == null)
                return new AnimationCurve();
            return new AnimationCurve()
            {
                keys = value.keys != null ? value.keys.Clone() as Keyframe[] : null,
                preWrapMode = value.preWrapMode,
                postWrapMode = value.postWrapMode
            };
        }
    }
}
#endif