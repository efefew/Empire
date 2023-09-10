using AdvancedEditorTools.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedEditorTools
{
    public class ButtonsExample : MonoBehaviour
    {
        // This is one of the key tools in this package and, without any doubt, the most powerful one.
        // After this brief explanation there are some examples to show the potential of buttons.
        // To use buttons simply add the attribute [Button] on top of your method.
        // You must set a label. Optionally, you can set the font size.

        /// ### IMPORTANT NOTE ###
        /// Make sure the parameters of the method are serializable by Unity
        /// Non UnityEngine.Object classes and structs are not supported yet.
        /// If you require one of these invalid types you can extract them from the method
        /// and add them as a class field like so:

        ///     ### This invalid parameter
        ///     
        ///     public void MyMethod(MyInvalidParamType param1){
        ///         ...
        ///     } 

        ///     ### Can be extracted like this
        /// 
        ///     [SerializeField]
        ///     private MyInvalidParamType param1;
        ///     public void MyMethod(){
        ///         ...
        ///     } 


        // #################################
        // #################################
        // #################################


        // Buttons can be useful to run code from the editor without
        // including them in the main flow of the program, and without
        // adding the [ExecuteInEditMode] attribute to the class.
        public int[] listExample = new int[0];

        [Button("Create Random List", 15)]
        public void RandomizeList(int listSize)
        {
            if (listSize < 0 || listSize > 1000)
            {
                Debug.Log("List size readjusted to range [0,1000]");
                listSize = Mathf.Clamp(listSize, 0, 1000);
            }

            listExample = new int[listSize];
            for (int i = 0; i < listSize; i++)
                listExample[i] = i;

            var rnd = new System.Random();
            listExample = listExample.OrderBy(x => rnd.Next(1, listSize)).ToArray();
        }

        [Button("Sort List", 25)]
        public void SortList()
        {
            listExample = listExample.OrderBy(x => x).ToArray();
        }


        // #################################
        // #################################
        // #################################


        // Buttons can come quite handy to run algorithms or test how some code may
        // behave under certain conditions...
        public Gradient gradientExample;

        [Button("Randomize Gradient", 25)]
        public void GenerateRandomGradient()
        {
            gradientExample = new();
            var keyCount = Random.Range(2, 7);
            var colorKeys = new GradientColorKey[keyCount];
            var timeInterval = 1.0f / (keyCount - 1);
            for (int i = 0; i < keyCount; i++)
            {
                colorKeys[i] = new GradientColorKey()
                {
                    color = Random.ColorHSV(0, 1, 0.7f, 1, 0.8f, 1),
                    time = i * timeInterval
                };
            }

            gradientExample.colorKeys = colorKeys.ToArray();
            gradientExample.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey { alpha = 1 } };
        }


        // #################################
        // #################################
        // #################################


        // Asynchronous methods are supported, but be careful with them, as there is no
        // way to stop their execution once they have been started. You can run this method
        // several times to check how catastrophic things might become.

        // This example method will modify the value of the color variable asynchronously
        // by lerping through the colors provided in the array parameter
        public Color delayedColor;
        [Button("Lerp color delayed")]
        public async void LerpColorsAsync(int stepsPerColor, Color[] colors)
        {
            if (colors.Length < 2)
                return;

            delayedColor = colors[0];
            stepsPerColor = Mathf.Clamp(stepsPerColor, 1, 100);

            for (int i = 1; i < colors.Length; i++)
            {
                var prevColor = delayedColor;
                var newColor = colors[i];
                float stepCount = 0;
                while (stepCount < stepsPerColor)
                {
                    await System.Threading.Tasks.Task.Delay(1);
                    stepCount++;
                    delayedColor = Color.Lerp(prevColor, newColor, stepCount / stepsPerColor);
                }
            }

            delayedColor = colors[0];
        }
    }
}
