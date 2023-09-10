#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace AdvancedEditorTools
{
    public static class DefaultSerializableTypes
    {
        public static readonly string[] typeNames = new[]
        {
            "Char",
            "Byte",
            "Int32",
            "Single",
            "Boolean",
            "String",
            "Color",
            "Vector2",
            "Vector3",
            "Vector4",
            "Bounds",
            "Rect",
            "Vector2Int",
            "Vector3Int",
            "BoundsInt",
            "RectInt",
            "Hash128",
            "Quaternion",
            "LayerMask",
            "SortingLayer",

            "AnimationCurve",
            "Gradient",
            "IEnumerable",
            "Object"
        };

        public static object GetDefaultElement(Type elementType)
        {
            if (elementType.IsValueType)
                return Activator.CreateInstance(elementType);
            return null;
        }

        // ###############################################

        public static Dictionary<string, TypeMethods> GetTypeMethodsDictionary()
        {
            Dictionary<string, TypeMethods> dict = new();

            dict.Add("Byte", new ByteTypeMethods());
            dict.Add("Char", new CharTypeMethods());
            dict.Add("Int32", new IntTypeMethods());
            dict.Add("Single", new FloatTypeMethods());
            dict.Add("Boolean", new BoolTypeMethods());
            dict.Add("String", new StringTypeMethods());
            dict.Add("Color", new ColorTypeMethods());
            dict.Add("Vector2", new Vector2TypeMethods());
            dict.Add("Vector3", new Vector3TypeMethods());
            dict.Add("Vector4", new Vector4TypeMethods());
            dict.Add("Bounds", new BoundsTypeMethods());
            dict.Add("Rect", new RectTypeMethods());
            dict.Add("Vector2Int", new Vector2IntTypeMethods());
            dict.Add("Vector3Int", new Vector3IntTypeMethods());
            dict.Add("BoundsInt", new BoundsIntTypeMethods());
            dict.Add("RectInt", new RectIntTypeMethods());
            dict.Add("Hash128", new Hash128TypeMethods());
            dict.Add("Quaternion", new QuaternionTypeMethods());
            dict.Add("LayerMask", new LayerMaskTypeMethods());
            dict.Add("SortingLayer", new SortingLayerTypeMethods());
            dict.Add("AnimationCurve", new AnimationCurveTypeMethods());
            dict.Add("Gradient", new GradientTypeMethods());

            // Non real type names, but labels
            dict.Add("Object", new ObjectTypeMethods());
            dict.Add("Enum", new EnumTypeMethods());
            dict.Add("Array", new ArrayTypeMethods());
            dict.Add("List", new ListTypeMethods());
            dict.Add("Struct", new StructTypeMethods());
            dict.Add("Class", new ClassTypeMethods());

            return dict;
        }
    }
}
#endif