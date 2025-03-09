#region

using System;
using UnityEngine;

#endregion

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}