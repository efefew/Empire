//
// Custom Attributes
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

#region

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

namespace OmniSARTechnologies.Helper
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayNameAttribute : PropertyAttribute
    {
        public readonly string displayName;

        public DisplayNameAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class WhatsThisAttribute : PropertyAttribute
    {
        public readonly string message;
#if UNITY_EDITOR
        public readonly MessageType messageType;
#endif

#if UNITY_EDITOR
        public WhatsThisAttribute(string message, MessageType messageType = MessageType.None)
        {
            this.message = message;
            this.messageType = messageType;
        }
#else
        public WhatsThisAttribute(string message, params object[] unused)
        {
            this.message = message;
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DocsAttribute : PropertyAttribute
    {
        public readonly string text;

        public DocsAttribute(string text = default)
        {
            this.text = text;
        }
    }
}