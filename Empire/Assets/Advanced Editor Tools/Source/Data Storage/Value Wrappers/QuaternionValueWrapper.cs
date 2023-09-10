#if UNITY_EDITOR
using System;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class QuaternionValueWrapper : GenericValueWrapper<Vector3>
    {
        public override void SetValue(object obj)
        {
            if (obj.GetType().Equals(typeof(Vector3)))
            {
                value = (Vector3)obj;
            }
            else
            {
                var quaternion = (Quaternion)obj;
                if (!Quaternion.Euler(value).Equals(quaternion))
                {
                    var ea = quaternion.eulerAngles;
                    value = new Vector3(
                            (float)Math.Round(ea.x, 2),
                            (float)Math.Round(ea.y, 2),
                            (float)Math.Round(ea.z, 2)
                    );
                }
            }
        }

        public override object Unwrap() => Quaternion.Euler(value);

        public override void OnInspector(Rect rect, string label)
        {
            var oldValue = Quaternion.Euler(this.value);
            base.OnInspector(rect, label);
            var newValue = Quaternion.Euler(SerializedField.SerializedProperty.vector3Value);
            if (!oldValue.Equals(newValue))
            {
                SerializedField.SetValue(newValue);
            }
        }
    }
}
#endif