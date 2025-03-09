using System;
using System.IO;
using UnityEngine;
using static System.IO.Path;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    public static class CommonTools
    {
        public static string GetTagManagerFilePath()
        {
            string? projectPath = GetProjectPathRaw();
            string? tagManagerPath = Combine(projectPath, "ProjectSettings/TagManager.asset");
            return tagManagerPath;
        }

        public static string GetInputManagerFilePath()
        {
            string? projectPath = GetProjectPathRaw();
            string? inputManagerPath = Combine(projectPath, "ProjectSettings/InputManager.asset");
            return inputManagerPath;
        }

        public static string GetProjectPath()
        {
            string rawPath = GetProjectPathRaw();
            OSPath osPath = new(rawPath);
            string unixPath = osPath.Unix;
            string trimmed = unixPath.TrimEnd('\\', '/');
            return trimmed;
        }

        /// <summary>
        ///     Get the path to the project folder.
        /// </summary>
        /// <returns>The project folder path</returns>
        private static string GetProjectPathRaw()
        {
            // Application.dataPath returns the path including /Assets, which we need to strip off
            string? path = Application.dataPath;
            DirectoryInfo directory = new(path);
            DirectoryInfo? parent = directory.Parent;
            if (parent != null)
                return parent.FullName;

            return path;
        }

        public static int GetHashOfPath(string path)
        {
            OSPath osPath = new(path);
            string unixPath = osPath.Unix;
            string trimmed = unixPath.TrimEnd('\\', '/');
            int hash = trimmed.GetHashCode();

            if (hash < 0) // Get rid of the negative values, so there will be no '-' char in file names
            {
                hash++;
                hash = Math.Abs(hash);
            }

            return hash;
        }
    }

    // https://github.com/dmitrynogin/cdsf/blob/master/Cds.Folders/OSPath.cs
    internal class OSPath
    {
        public static readonly OSPath Empty = "";

        public OSPath(string text)
        {
            Text = text.Trim();
        }

        public static bool IsWindows => DirectorySeparatorChar == '\\';

        protected string Text { get; }

        public string Normalized => IsWindows ? Windows : Unix;

        public string Windows => Text.Replace('/', '\\');

        //public string Unix => Simplified.Text.Replace('\\', '/');
        public string Unix => Text.Replace('\\', '/');

        public OSPath Relative => Simplified.Text.TrimStart('/', '\\');
        public OSPath Absolute => IsAbsolute ? this : "/" + Relative;

        public bool IsAbsolute => IsRooted || HasVolume;
        public bool IsRooted => Text.Length >= 1 && (Text[0] == '/' || Text[0] == '\\');
        public bool HasVolume => Text.Length >= 2 && Text[1] == ':';
        public OSPath Simplified => HasVolume ? Text.Substring(2) : Text;

        public OSPath Parent => GetDirectoryName(Text);

        public static implicit operator OSPath(string text)
        {
            return new OSPath(text);
        }

        public static implicit operator string(OSPath path)
        {
            return path.Normalized;
        }

        public override string ToString()
        {
            return Normalized;
        }

        public bool Contains(OSPath path)
        {
            return Normalized.StartsWith(path);
        }

        public static OSPath operator +(OSPath left, OSPath right)
        {
            return new OSPath(Combine(left, right.Relative));
        }

        public static OSPath operator -(OSPath left, OSPath right)
        {
            return left.Contains(right)
                ? new OSPath(left.Normalized.Substring(right.Normalized.Length)).Relative
                : left;
        }
    }
}