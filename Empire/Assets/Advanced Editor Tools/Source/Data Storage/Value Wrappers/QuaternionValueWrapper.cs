#if UNITY_EDITOR

#region

using System;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
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
                Quaternion quaternion = (Quaternion)obj;
                if (!Quaternion.Euler(value).Equals(quaternion))
                {
                    Vector3 ea = quaternion.eulerAngles;
                    value = new Vector3(
                        (float)Math.Round(ea.x, 2),
                        (float)Math.Round(ea.y, 2),
                        (float)Math.Round(ea.z, 2)
                    );
                }
            }
        }

        public override object Unwrap()
        {
            return Quaternion.Euler(value);
        }

        public override void OnInspector(Rect rect, string label)
        {
            Quaternion oldValue = Quaternion.Euler(value);
            base.OnInspector(rect, label);
            Quaternion newValue = Quaternion.Euler(SerializedField.SerializedProperty.vector3Value);
            if (!oldValue.Equals(newValue)) SerializedField.SetValue(newValue);
        }
    }
}
#endif