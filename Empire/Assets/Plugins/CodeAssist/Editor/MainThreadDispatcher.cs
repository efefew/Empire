#nullable enable


using System;
using System.Collections.Concurrent;
using UnityEditor;

namespace Meryel.UnityCodeAssist.Editor
{
    [InitializeOnLoad]
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentBag<Action> actions;

        static MainThreadDispatcher()
        {
            actions = new ConcurrentBag<Action>();
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            while (actions.TryTake(out Action? action)) action.Invoke();
        }

        public static void Add(Action action)
        {
            actions.Add(action);
        }
    }
}