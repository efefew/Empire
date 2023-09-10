using AdvancedEditorTools.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AdvancedEditorTools
{
    public class FoldoutsExample : MonoBehaviour
    {
        // Collapsable sections can be quite useful to organise
        // your variables and display in the inspector only the
        // ones you are using frequently


        // To define a collapsable section use the atttributes
        // [BeginFoldout(<label>)] and [EndFoldout]. Make sure
        // to close your scopes, as unexpected behaviour may
        // occur.
        [BeginFoldout("Important variables")]
        public int test1;
        public int test2;
        public int test3;
        public int test4;
        [EndFoldout]


        // Notice the "includeLast = true" flag. This is required when there are
        // no fields left in the script in which the attribute can be applied.
        [BeginFoldout("Other variables")]
        public int test5;
        [BeginFoldout("Having a mess of variables")]
        public int test6;
        [BeginFoldout("which belong to several categories")]
        public int test7;
        [BeginFoldout("can be fixed if")]
        public int test8;
        [BeginFoldout("we simply use")]
        public int test9;
        [BeginFoldout("foldouts")]
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        [EndFoldout(includeLast = true)]
        public int test10;
    }
}
