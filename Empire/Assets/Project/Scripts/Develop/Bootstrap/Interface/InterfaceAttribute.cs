using System;

using UnityEngine;

public class InterfaceAttribute : PropertyAttribute
{
    public Type InterfaceType { get; }

    public InterfaceAttribute(Type interfaceType) => InterfaceType = interfaceType;
}