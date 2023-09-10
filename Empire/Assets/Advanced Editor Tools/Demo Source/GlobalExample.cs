using AdvancedEditorTools.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedEditorTools
{
    /// ### IMPORTANT NOTE ###
    /// This script is used to debug the full range of capabilities of the 
    /// tools provided by the package, and it is not recommended to be 
    /// examined by the user.

    [System.Serializable]
    public class TestClass
    {
        public int num;
        public List<string> frase;
        public Dictionary<int, string> dic;
        public (int, int) twoNums;
        public AnimationCurve curve;
        public int myNewNum;
        public Gradient myNewGradient;
        public List<List<List<MyStruct>>> dicRec;
        public TestClass Recurssion;
        public Texture img;
        private int iShouldNotBeSerialized;
        [SerializeField]
        private int iShouldBeSerialized;
        [SerializeReference]
        private GlobalExample iShouldBeSerializedToo;

        public TestClass() { }
        public TestClass(int num) { this.num = num; }
    }

    public enum TestEnum
    {
        test, test2, test3
    }

    [System.Serializable]
    public struct MyStruct
    {
        public string name;
        public float id;
        public Vector4 spatial4DCoordss;
        public BoundsInt bounds;
        public Gradient colorScheme;
        public List<int> nums;
        public TestClass nestedClass;
        // public Dictionary<int, string> dickens;
    }

    [ExecuteInEditMode]
    public class GlobalExample : MonoBehaviour
    {
        [BeginColumnArea(columnStyle = LayoutStyle.BevelGreen)]
        public int thisIsMyName;
        public int thisIsMyLongName;
        public int thisIsMyExtraLongName;
        public int num2;
        public int num3;
        [NewColumn(LayoutStyle.BevelRed)]
        public int num4;
        public int num5;
        [BeginFoldout("My fold")]
        public int num6;
        [BeginFoldout("My fold 2")]
        public int num7;
        [EndFoldout]
        [EndFoldout]
        [NewColumn(LayoutStyle.BevelCyan)]
        public int num8;
        [ReadOnly]
        [Tooltip("This variable is public but cannot be mofified by hand")]
        public int num9;
        public int num10;
        [EndColumnArea]

        [BeginColumnArea()]
        [BeginFoldout("Basic types")]
        public byte byteVal2;
        public char charVal;
        public int intVal;
        [ReadOnly]
        [Range(0, 10)]
        public float floatVal;
        public bool boolVal;
        [EndFoldout]

        [NewColumn]
        [BeginFoldout("Unity Structs")]
        [TextArea]
        public string stringVal;
        public Color colorPickerrrr;
        public Vector2 v2;
        public Vector3 v3;
        public Vector4 v4;
        public Bounds bounds;
        public Rect rect;
        [BeginFoldout("Int-versioned Structs")]
        public Vector2Int vector2Int;
        public Vector3Int vector3Int;
        public BoundsInt boundsInt;
        public RectInt rectInt;
        [EndFoldout]
        public Hash128 hash128;
        public Quaternion quaternion;
        public LayerMask layerMask;
        public SortingLayer sortingLayer;
        [EndFoldout]
        [EndColumnArea]

        [BeginFoldout("Reference Types")]
        [BeginColumnArea]
        [Header("Custom Unity Classes")]
        public AnimationCurve curve;
        public Gradient gradient;
        [NewColumn]
        [Header("Object References")]
        public GlobalExample monoObj;
        public Texture obj;
        [EndColumnArea]
        [EndFoldout]

        [BeginFoldout("Advanced Data Types")]
        [BeginFoldout("Enumerables")]
        public int[] arrTest;
        public List<float> listTest = new();
        [EndFoldout]

        [BeginFoldout("Custom Types")]
        public TestEnum testEnum;
        public MyStruct structTest;
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        public TestClass myClass;

        [Button("PARAMETERS TEST", 25)]
        public void TestParams(
            byte byteTest, char character, int intNum, float decimalNum, bool eval, string exampleText,
            Color colorPicker, Vector2 v2, Vector3 v3, Vector4 v4, Bounds bounds, Rect rect,
            Vector2Int vector2Int, Vector3Int vector3Int, BoundsInt boundsInt, RectInt rectInt,
            Hash128 hash128, Quaternion quaternion, LayerMask layerMask, SortingLayer sortingLayer,
            AnimationCurve curve, Gradient gradient, TestEnum testEnum,
            GlobalExample monoObj, Texture obj, GameObject go,

            // Enumerable types test
            int[] integersArray, List<int> integersList,
            Color[] colorArray,
            List<SortingLayer> mySortingLayers,
            AnimationCurve[] curvesArray,
            List<Gradient> gradientsList,
            TestEnum[] enums,
            List<int[]> nestedNumbers,
            List<List<float>[]>[] macroNesting

        // TODO: These types will be implemented in the future
        // <Class types>        <struct types>
        // MyStruct structTest, TestClass testClass
        )
        {
            Debug.Log("Success");
        }
    }
}