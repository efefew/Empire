#nullable enable


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Meryel.UnityCodeAssist.YamlDotNet.Core;
using Meryel.UnityCodeAssist.YamlDotNet.Serialization;
using Serilog;
using UnityEditor;

namespace Meryel.UnityCodeAssist.Editor.Input
{
    internal class UnityInputManager
    {
        private InputManager? inputManager;

        //string yamlPath;
        private TextReader? reader;

        public void ReadFromText(string text)
        {
            reader = new StringReader(text);
            ReadAux(false, out _);
        }

        public void ReadFromPath(string yamlPath)
        {
            switch (EditorSettings.serializationMode)
            {
                case SerializationMode.ForceText:
                {
                    reader = new StreamReader(yamlPath);
                    ReadAux(false, out _);
                }
                    break;

                case SerializationMode.ForceBinary:
                {
                    // this approach will work for InputManager since its file size is small and limited
                    // but in the future, we may need to switch to reading binary files for big files
                    // like this https://github.com/Unity-Technologies/UnityDataTools
                    // or this https://github.com/SeriousCache/UABE
                    string converted = GetOrCreateConvertedFile(yamlPath);
                    var rawLines = File.ReadLines(converted);
                    string yamlText = Text2Yaml.Convert(rawLines);
                    reader = new StringReader(yamlText);
                    ReadAux(false, out _);
                }
                    break;

                case SerializationMode.Mixed:
                {
                    reader = new StreamReader(yamlPath);
                    ReadAux(true, out bool hasSemanticError);
                    if (hasSemanticError)
                    {
                        string converted = GetOrCreateConvertedFile(yamlPath);
                        var rawLines = File.ReadLines(converted);
                        string yamlText = Text2Yaml.Convert(rawLines);
                        reader = new StringReader(yamlText);
                        ReadAux(false, out _);
                    }
                }
                    break;
            }
        }


        private void ReadAux(bool canHaveSemanticError, out bool hasSemanticError)
        {
            hasSemanticError = false;

            if (reader == null)
            {
                Log.Warning($"{nameof(UnityInputManager)}.{nameof(reader)} is null");
                return;
            }

            //var reader = new StreamReader(yamlPath);
            IDeserializer deserializer = new DeserializerBuilder()
                .WithTagMapping("tag:unity3d.com,2011:13", typeof(Class13Mapper))
                .IgnoreUnmatchedProperties()
                .Build();
            //serializer.Settings.RegisterTagMapping("tag:unity3d.com,2011:13", typeof(Class13));
            //serializer.Settings.ComparerForKeySorting = null;
            Class13Mapper? result;
            try
            {
                result = deserializer.Deserialize<Class13Mapper>(reader);
            }
            catch (SemanticErrorException semanticErrorException)
            {
                Log.Debug(semanticErrorException, "Couldn't parse InputManager.asset yaml file");
                if (!canHaveSemanticError)
                    Log.Error(semanticErrorException, "Couldn't parse InputManager.asset yaml file unexpectedly");

                hasSemanticError = true;
                return;
            }
            finally
            {
                reader.Close();
            }

            InputManagerMapper? inputManagerMapper = result?.InputManager;
            if (inputManagerMapper == null)
            {
                Log.Warning($"{nameof(inputManagerMapper)} is null");
                return;
            }

            inputManager = new InputManager(inputManagerMapper);
        }


        public void SendData()
        {
            if (inputManager == null)
                return;

            string[]? axisNames = inputManager.Axes.Select(a => a.Name!).Where(n => !string.IsNullOrEmpty(n)).Distinct()
                .ToArray();
            string[]? axisInfos = axisNames.Select(a => inputManager.Axes.GetInfo(a)).ToArray();
            if (!CreateBindingsMap(out string[]? buttonKeys, out string[]? buttonAxis))
                return;

            string[] joystickNames;
            try
            {
                joystickNames = UnityEngine.Input.GetJoystickNames();
            }
            catch (InvalidOperationException)
            {
                // Occurs if user have switched active Input handling to Input System package in Player Settings.
                joystickNames = new string[0];
            }

            NetMQInitializer.Publisher?.SendInputManager(axisNames, axisInfos, buttonKeys, buttonAxis, joystickNames);

            /*
            NetMQInitializer.Publisher?.SendInputManager(
                inputManager.Axes.Select(a => a.Name).Distinct().ToArray(),
                inputManager.Axes.Select(a => a.positiveButton).ToArray(),
                inputManager.Axes.Select(a => a.negativeButton).ToArray(),
                inputManager.Axes.Select(a => a.altPositiveButton).ToArray(),
                inputManager.Axes.Select(a => a.altNegativeButton).ToArray(),
                UnityEngine.Input.GetJoystickNames()
                );
            */
        }


        private bool CreateBindingsMap([NotNullWhen(true)] out string[]? inputKeys,
            [NotNullWhen(true)] out string[]? inputAxis)
        {
            if (inputManager == null)
            {
                inputKeys = null;
                inputAxis = null;
                return false;
            }

            var dict = new Dictionary<string, string?>();

            foreach (InputAxis? axis in inputManager.Axes)
                if (axis.altNegativeButton != null && !string.IsNullOrEmpty(axis.altNegativeButton))
                    dict[axis.altNegativeButton] = axis.Name;
            foreach (InputAxis? axis in inputManager.Axes)
                if (axis.negativeButton != null && !string.IsNullOrEmpty(axis.negativeButton))
                    dict[axis.negativeButton] = axis.Name;
            foreach (InputAxis? axis in inputManager.Axes)
                if (axis.altPositiveButton != null && !string.IsNullOrEmpty(axis.altPositiveButton))
                    dict[axis.altPositiveButton] = axis.Name;
            foreach (InputAxis? axis in inputManager.Axes)
                if (axis.positiveButton != null && !string.IsNullOrEmpty(axis.positiveButton))
                    dict[axis.positiveButton] = axis.Name;

            string[] keys = new string[dict.Count];
            string[] values = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            dict.Values.CopyTo(values, 0);

            inputKeys = keys;
            inputAxis = values;
            return true;
        }


        private static string GetOrCreateConvertedFile(string filePath)
        {
            string hash = GetMD5Hash(filePath);
            string convertedPath = Path.Combine(Path.GetTempPath(), $"UCA_IM_{hash}.txt");

            if (!File.Exists(convertedPath))
            {
                Log.Debug("Converting binary to text format of {File} to {Target}", filePath, convertedPath);
                Binary2TextExec converter = new();
                converter.Exec(filePath, convertedPath);
            }
            else
            {
                Log.Debug("Converted file already exists at {Target}", convertedPath);
            }

            return convertedPath;
        }

        /// <summary>
        ///     Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string filePath)
        {
            using MD5CryptoServiceProvider md5 = new();
            return GetHash(filePath, md5);
        }

        /// <summary>
        ///     Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(Stream s)
        {
            using MD5CryptoServiceProvider md5 = new();
            return GetHash(s, md5);
        }

        private static string GetHash(string filePath, HashAlgorithm hasher)
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return GetHash(fs, hasher);
        }

        private static string GetHash(Stream s, HashAlgorithm hasher)
        {
            byte[]? hash = hasher.ComputeHash(s);
            string? hashStr = Convert.ToBase64String(hash);
            //return hashStr.TrimEnd('=');
            string? hashStrAlphaNumeric = Regex.Replace(hashStr, "[^A-Za-z0-9]", "");
            return hashStrAlphaNumeric;
        }
    }

    public enum AxisType
    {
        KeyOrMouseButton = 0,
        MouseMovement = 1,
        JoystickAxis = 2
    }

#pragma warning disable IDE1006

    public class InputAxisMapper
    {
        public int serializedVersion { get; set; }

        public string? m_Name { get; set; }
        public string? descriptiveName { get; set; }
        public string? descriptiveNegativeName { get; set; }
        public string? negativeButton { get; set; }
        public string? positiveButton { get; set; }
        public string? altNegativeButton { get; set; }
        public string? altPositiveButton { get; set; }

        //public float gravity { get; set; }
        //public float dead { get; set; }
        //public float sensitivity { get; set; }
        public string? gravity { get; set; }
        public string? dead { get; set; }
        public string? sensitivity { get; set; }

        //public bool snap { get; set; }
        public int snap { get; set; }

        //public bool invert { get; set; }
        public int invert { get; set; }

        //public AxisType type { get; set; }
        public int type { get; set; }

        public int axis { get; set; }
        public int joyNum { get; set; }
    }

    public class InputAxis
    {
        private readonly InputAxisMapper map;

        public InputAxis(InputAxisMapper map)
        {
            this.map = map;
        }

        public int SerializedVersion
        {
            get => map.serializedVersion;
            set => map.serializedVersion = value;
        }

        public string? Name => map.m_Name;
        public string? descriptiveName => map.descriptiveName;
        public string? descriptiveNegativeName => map.descriptiveNegativeName;
        public string? negativeButton => map.negativeButton;
        public string? positiveButton => map.positiveButton;
        public string? altNegativeButton => map.altNegativeButton;
        public string? altPositiveButton => map.altPositiveButton;

        public float gravity => float.Parse(map.gravity); //**--format
        public float dead => float.Parse(map.dead); //**--format
        public float sensitivity => float.Parse(map.sensitivity); //**--format

        public bool snap => map.snap != 0;
        public bool invert => map.invert != 0;

        public AxisType type => (AxisType)map.type;

        public int axis => map.axis;
        public int joyNum => map.joyNum;
    }

    public class InputManagerMapper
    {
        public int m_ObjectHideFlags { get; set; }
        public int serializedVersion { get; set; }
        public int m_UsePhysicalKeys { get; set; }
        public List<InputAxisMapper>? m_Axes { get; set; }
    }

#pragma warning restore IDE1006

    public class InputManager
    {
        private readonly InputManagerMapper map;

        public InputManager(InputManagerMapper map)
        {
            this.map = map;
            Axes = new List<InputAxis>();

            if (map.m_Axes == null)
            {
                Log.Warning("map.m_Axes is null");
                return;
            }

            foreach (InputAxisMapper? a in map.m_Axes)
                Axes.Add(new InputAxis(a));
        }

        public int ObjectHideFlags
        {
            get => map.m_ObjectHideFlags;
            set => map.m_ObjectHideFlags = value;
        }

        public int SerializedVersion
        {
            get => map.serializedVersion;
            set => map.serializedVersion = value;
        }

        public bool UsePhysicalKeys
        {
            get => map.m_UsePhysicalKeys != 0;
            set => map.m_UsePhysicalKeys = value ? 1 : 0;
        }

        /*public List<InputAxisMapper> Axes
        {
            get { return map.m_Axes; }
            set { map.m_Axes = value; }
        }*/
        public List<InputAxis> Axes { get; }
    }

    public class Class13Mapper
    {
        public InputManagerMapper? InputManager { get; set; }
    }
}