﻿#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class SerializableDictionaryExample : MonoBehaviour
{
    // The dictionaries can be accessed throught a property
    [SerializeField] private StringStringDictionary m_stringStringDictionary;

    public ObjectColorDictionary m_objectColorDictionary;
    public StringColorArrayDictionary m_stringColorArrayDictionary;
#if NET_4_6 || NET_STANDARD_2_0
    public StringHashSet m_stringHashSet;
#endif
    public IDictionary<string, string> StringStringDictionary
    {
        get => m_stringStringDictionary;
        set => m_stringStringDictionary.CopyFrom(value);
    }

    private void Reset()
    {
        // access by property
        StringStringDictionary = new Dictionary<string, string>
            { { "first key", "value A" }, { "second key", "value B" }, { "third key", "value C" } };
        m_objectColorDictionary = new ObjectColorDictionary { { gameObject, Color.blue }, { this, Color.red } };
    }
}