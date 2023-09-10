using System;
using UnityEngine;

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public abstract class LayoutAttribute : PropertyAttribute { }

    public enum LayoutStyle
    {
        None,
        Box,
        Window,

        Bevel,
        BevelBlue,
        BevelCyan,
        BevelGreen,
        BevelYellow,
        BevelOrange,
        BevelRed,

        BoxDark,
        BoxLight,
        BoxOutline,
        BoxRound,
    }

    public static class LayoutStyleMethods
    {
        public static string GetString(this LayoutStyle style)
        {
            return style switch
            {
                LayoutStyle.Box => "box",
                LayoutStyle.Window => "window",

                LayoutStyle.Bevel => "flow node 0",
                LayoutStyle.BevelBlue => "flow node 1",
                LayoutStyle.BevelCyan => "flow node 2",
                LayoutStyle.BevelGreen => "flow node 3",
                LayoutStyle.BevelYellow => "flow node 4",
                LayoutStyle.BevelOrange => "flow node 5",
                LayoutStyle.BevelRed => "flow node 6",

                LayoutStyle.BoxDark => "ShurikenEffectBg",
                LayoutStyle.BoxLight => "hostview",
                LayoutStyle.BoxOutline => "Wizard Box",
                LayoutStyle.BoxRound => "NotificationBackground",
                _ => null,
            };
        }
    }
}
