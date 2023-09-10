using System;
using UnityEngine;

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}
