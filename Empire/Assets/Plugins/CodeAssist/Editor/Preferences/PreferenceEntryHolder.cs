#nullable enable


using System;
using System.Collections.Generic;
using UnityEngine;

//namespace BgTools.PlayerPrefsEditor
namespace Meryel.UnityCodeAssist.Editor.Preferences
{
    [Serializable]
    public class PreferenceEntryHolder : ScriptableObject
    {
        public List<PreferenceEntry>? userDefList;
        public List<PreferenceEntry>? unityDefList;

        private void OnEnable()
        {
            hideFlags = HideFlags.DontSave;
            userDefList ??= new List<PreferenceEntry>();
            unityDefList ??= new List<PreferenceEntry>();
        }

        public void ClearLists()
        {
            userDefList?.Clear();
            unityDefList?.Clear();
        }
    }

    [Serializable]
    public class PreferenceEntry
    {
        public enum PrefTypes
        {
            String = 0,
            Int = 1,
            Float = 2
        }

        public PrefTypes m_typeSelection;
        public string? m_key;

        // Need diffrend ones for auto type selection of serilizedProerty
        public string? m_strValue;
        public int m_intValue;
        public float m_floatValue;

        public string? ValueAsString()
        {
            return m_typeSelection switch
            {
                PrefTypes.String => m_strValue,
                PrefTypes.Int => m_intValue.ToString(),
                PrefTypes.Float => m_floatValue.ToString(),
                _ => string.Empty
            };
        }
    }
}