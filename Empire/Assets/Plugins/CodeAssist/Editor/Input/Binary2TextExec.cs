#nullable enable


using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using UnityEditor;

//namespace UTJ.UnityCommandLineTools
namespace Meryel.UnityCodeAssist.Editor.Input
{
    // <summary>
    // bin2textをUnityEditorから実行する為のClass
    // programed by Katsumasa.Kimura
    // </summary>
    public class Binary2TextExec : EditorToolExec
    {
        public Binary2TextExec() : base("binary2text")
        {
        }

        // <summary>
        // bin2text filePath outPath options
        // /summary>
        public int Exec(string filePath, string outPath, string options)
        {
            string? args = string.Format(@"""{0}"" ""{1}"" {2}", filePath, outPath, options);
            return Exec(args);
        }

        public int Exec(string filePath, string outPath, bool detailed = false, bool largeBinaryHashOnly = false,
            bool hexFloat = false)
        {
            //var args = string.Format(@"""{0}"" ""{1}"" {2}", filePath, outPath, options);
            string? args = string.Format(@"""{0}"" ""{1}""", filePath, outPath);

            if (detailed)
                args += " -detailed";
            if (largeBinaryHashOnly)
                args += " -largebinaryhashonly";
            if (hexFloat)
                args += " -hexfloat";

            return Exec(args);
        }
    }

    // <summary>
    // UnityEditorに含まれるコマンドラインツールを実行する為の基底Class
    // programed by Katsumasa.Kimura
    //</summary>
    public class EditorToolExec
    {
        // <value>
        // UnityEditorがインストールされているディレクトリへのパス
        // </value>
        protected string mEditorPath;

        // <value>
        // 実行ファイル名
        // </value>
        protected string mExecFname;

        // <value>
        // 実行ファイルへのフルパス
        // </value>
        protected string mExecFullPath;

        // <value>
        // 実行結果のOUTPUT
        // </value>

        // <value>
        // Toolsディレクトリへのパス
        // </value>
        protected string mToolsPath;

        // <summary>
        // コンストラクタ
        // <param>
        // mExecFname : 実行ファイル名
        // </param>
        // /summary>
        public EditorToolExec(string mExecFname)
        {
            mEditorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            mToolsPath = Path.Combine(mEditorPath, @"Data/Tools");
            this.mExecFname = mExecFname;
            //var files = Directory.GetFiles(mToolsPath, mExecFname, SearchOption.AllDirectories);
            string[]? files = Directory.GetFiles(mEditorPath, mExecFname + "*", SearchOption.AllDirectories);

            if (files.Length == 0)
                Log.Error("{App} app couldn't be found at {Path}", mExecFname, mEditorPath);

            mExecFullPath = files[0];
        }

        // <value>
        // 実行結果のOUTPUT
        // </value>
        public string? Output { get; private set; }

        // <summary> 
        // コマンドラインツールを実行する
        // <param> 
        // arg : コマンドラインツールに渡す引数 
        // </param>
        // </summary>
        public int Exec(string arg)
        {
            int exitCode = -1;

            try
            {
                using Process process = new();
                process.StartInfo.FileName = mExecFullPath;
                process.StartInfo.Arguments = arg;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                Output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                exitCode = process.ExitCode;
                process.Close();
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log(e);
                Log.Error(e, "Exception while running process at {Scope}.{Location}", nameof(EditorToolExec),
                    nameof(Exec));
            }

            return exitCode;
        }
    }
}