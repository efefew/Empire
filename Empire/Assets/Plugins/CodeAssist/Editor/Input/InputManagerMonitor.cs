#nullable enable


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using UnityEditor;

namespace Meryel.UnityCodeAssist.Editor.Input
{
    public class InputManagerMonitor
    {
        private static readonly Lazy<InputManagerMonitor> _instance = new(() => new InputManagerMonitor());

        //UnityInputManager inputManager;
        private readonly string inputManagerFilePath;
        private DateTime previousTagManagerLastWrite;

        public InputManagerMonitor()
        {
            EditorApplication.update += Update;
            inputManagerFilePath = CommonTools.GetInputManagerFilePath();

            previousTagManagerLastWrite = File.GetLastWriteTime(inputManagerFilePath);
        }

        public static InputManagerMonitor Instance => _instance.Value;

        private void Update()
        {
            DateTime currentInputManagerLastWrite = File.GetLastWriteTime(inputManagerFilePath);
            if (currentInputManagerLastWrite != previousTagManagerLastWrite)
            {
                previousTagManagerLastWrite = currentInputManagerLastWrite;
                Bump();
            }
        }

        public void Bump()
        {
            Log.Debug("InputMonitor {Event}", nameof(Bump));

            UnityInputManager inputManager = new();
            inputManager.ReadFromPath(inputManagerFilePath);
            inputManager.SendData();
        }
    }


    public static partial class Extensions
    {
        public static string GetInfo(this List<InputAxis> axes, string? name)
        {
            if (name == null || string.IsNullOrEmpty(name))
                return string.Empty;

            //axis.descriptiveName
            var axesWithName = axes.Where(a => a.Name == name);

            int threshold = 80;

            StringBuilder sb = new();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveName))
                    sb.Append($"{axis.descriptiveName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveNegativeName))
                    sb.Append($"{axis.descriptiveNegativeName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.positiveButton))
                    sb.Append($"[{axis.positiveButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altPositiveButton))
                    sb.Append($"{{{axis.altPositiveButton}}} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.negativeButton))
                    sb.Append($"-[{axis.negativeButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (InputAxis? axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altNegativeButton))
                    sb.Append($"-{{{axis.altNegativeButton}}} ");

            return sb.ToString();
        }
    }
}