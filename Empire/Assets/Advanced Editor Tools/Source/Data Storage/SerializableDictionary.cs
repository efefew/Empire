using System.Collections.Generic;
using UnityEngine;

namespace AdvancedEditorTools.DataTypes
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        TKey[] keys = new TKey[0];
        [SerializeField]
        TValue[] values = new TValue[0];

        public void OnAfterDeserialize()
        {
            if (keys.Length != values.Length)
            {
                Debug.LogError("Invalid dictionary state");
                return;
            }
            this.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                this.TryAdd(keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            keys = new TKey[Keys.Count];
            values = new TValue[Values.Count];
            this.Keys.CopyTo(keys, 0);
            this.Values.CopyTo(values, 0);
        }
    }
}
