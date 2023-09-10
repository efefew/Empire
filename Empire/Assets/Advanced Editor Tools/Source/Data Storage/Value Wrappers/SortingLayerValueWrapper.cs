#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    [System.Serializable]
    public class SortingLayerValueWrapper : GenericValueWrapper<string>
    {
        public override object Unwrap()
        {
            var layers = SortingLayer.layers;
            for (var i = 0; i < layers.Length; i++)
            {
                if (layers[i].name.Equals(value))
                    return layers[i];
            }

            for (var i = 0; i < layers.Length; i++)
            {
                if (layers[i].name.Equals("Default"))
                    return layers[i];
            }

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
            var names = new string[layers.Length];
            int layerIdx = 0;
            for (var i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                if (layers[i].name == layerName)
                    layerIdx = i;
            }

            layerIdx = EditorGUI.Popup(rect, label, layerIdx, names);
            var newLayer = layers[layerIdx];
            if (SerializedField != null)
                SerializedField.SetValue(newLayer);
            else
                this.value = newLayer.name;

            if (layerName != newLayer.name)
            {
                // Debug.Log("Change detected in sorting layer");
                SerializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif