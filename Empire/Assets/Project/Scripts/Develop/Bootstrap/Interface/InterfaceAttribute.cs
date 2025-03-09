#region

using System;
using UnityEngine;

#endregion

public class InterfaceAttribute : PropertyAttribute
{
    public InterfaceAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }
}