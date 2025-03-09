using System;
using UnityEngine;

namespace Zelude
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public const SliderFieldPosition DefaultMinFieldPosition = SliderFieldPosition.Left;
        public const SliderFieldPosition DefaultMaxFieldPosition = SliderFieldPosition.Right;
        public readonly string DisplayName;
        public readonly float Max;
        public readonly SliderFieldPosition MaxFieldPosition;
        public readonly string MaxVariableName;

        public readonly float Min;
        public readonly SliderFieldPosition MinFieldPosition;

        public MinMaxSliderAttribute(float min, float max, string maxVariableName, string displayName = null,
            SliderFieldPosition minFieldPosition = DefaultMinFieldPosition,
            SliderFieldPosition maxFieldPosition = DefaultMaxFieldPosition) : this(min, max, minFieldPosition,
            maxFieldPosition)
        {
            DisplayName = displayName;
            MaxVariableName = maxVariableName;
        }

        public MinMaxSliderAttribute(float min, float max,
            SliderFieldPosition minFieldPosition = DefaultMinFieldPosition,
            SliderFieldPosition maxFieldPosition = DefaultMaxFieldPosition)
        {
            Min = min;
            Max = max;
            MinFieldPosition = minFieldPosition;
            MaxFieldPosition = maxFieldPosition;
        }
    }

    public enum SliderFieldPosition
    {
        None = 0,
        Left = 1,
        Right = 2
    }
}