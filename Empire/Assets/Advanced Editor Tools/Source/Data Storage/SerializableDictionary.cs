#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace AdvancedEditorTools.DataTypes
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private TKey[] keys = new TKey[0];

        [SerializeField] private TValue[] values = new TValue[0];

        public void OnAfterDeserialize()
        {
            if (keys.Length != values.Length)
            {
                Debug.LogError("Invalid dictionary state");
                return;
            }

            Clear();
            for (int i = 0; i < keys.Length; i++) TryAdd(keys[i], values[i]);
        }

        public void OnBeforeSerialize()
        {
            keys = new TKey[Keys.Count];
            values = new TValue[Values.Count];
            Keys.CopyTo(keys, 0);
            Values.CopyTo(values, 0);
        }
    }
}