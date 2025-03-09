#if UNITY_EDITOR

#region

using System;
using UnityEditor;
using UnityEngine;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class SortingLayerValueWrapper : GenericValueWrapper<string>
    {
        public override object Unwrap()
        {
            var layers = SortingLayer.layers;
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].name.Equals(value))
                    return layers[i];

            for (int i = 0; i < layers.Length; i++)
                if (layers[i].name.Equals("Default"))
                    return layers[i];

            return layers[0];
        }

        public override void SetValue(object obj)
        {
            if (obj is string name)
                value = name;
            else
                value = ((SortingLayer)obj).name;
        }

        public override void OnInspector(Rect rect, string label)
        {
            string layerName = value;

            var layers = SortingLayer.layers;
            string[] names = new string[layers.Length];
            int layerIdx = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                if (layers[i].name == layerName)
                    layerIdx = i;
            }

            layerIdx = EditorGUI.Popup(rect, label, layerIdx, names);
            SortingLayer newLayer = layers[layerIdx];
            if (SerializedField != null)
                SerializedField.SetValue(newLayer);
            else
                value = newLayer.name;

            if (layerName != newLayer.name)
                // Debug.Log("Change detected in sorting layer");
                SerializedObject.ApplyModifiedProperties();
        }
    }
}
#endif