#nullable enable


using System.Diagnostics;
using Meryel.UnityCodeAssist.Editor.Logger;
using Serilog;
using UnityEditor;
using UnityEngine;

namespace Meryel.UnityCodeAssist.Editor
{
    public class FeedbackWindow : EditorWindow
    {
        private GUIStyle? styleLabel;


        private void OnEnable()
        {
            //**--icon
            //var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png");
            //titleContent = new GUIContent("Code Assist", icon);
            titleContent = new GUIContent("Code Assist Feedback");
        }

        private void OnGUI()
        {
            int errorCount = ELogger.GetErrorCountInInternalLog();
            int warningCount = ELogger.GetWarningCountInInternalLog();
            string logContent = ELogger.GetInternalLogContent();
            if (!string.IsNullOrEmpty(ELogger.VsInternalLog))
                logContent += ELogger.VsInternalLog;

            styleLabel ??= new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            if (errorCount > 0)
                EditorGUILayout.LabelField(
                    $"{errorCount} error(s) found in logs. Please submit a feedback (via e-mail, Discord or GitHub) with the logs if possible.",
                    styleLabel, GUILayout.ExpandWidth(true));
            else if (warningCount > 0)
                EditorGUILayout.LabelField(
                    $"{warningCount} warnings(s) found in logs. Please submit a feedback (via e-mail, Discord or GitHub) with the logs if possible.",
                    styleLabel, GUILayout.ExpandWidth(true));
            else
                EditorGUILayout.LabelField(
                    "No errors found in logs. Please submit a feedback (via e-mail, Discord or GitHub) describing what went wrong or unexpected.",
                    styleLabel, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Send e-mail"))
            {
                string uri = "mailto:merryyellow@outlook.com";
                Process.Start(new ProcessStartInfo(uri));
            }

            if (GUILayout.Button("Send Discord message"))
            {
                //var uri = "discord://invites/2CgKHDq";
                string uri = "https://discord.gg/2CgKHDq";
                Process.Start(new ProcessStartInfo(uri));
            }

            if (GUILayout.Button("Submit GitHub issue"))
            {
                string uri = "https://github.com/merryyellow/Unity-Code-Assist/issues/new/choose";
                Application.OpenURL(uri);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("View Unity full log"))
            {
                string? filePath = ELogger.FilePath;
                Process.Start(filePath);
            }

            if (GUILayout.Button("View Visual Studio full log"))
            {
                string? filePath = ELogger.VSFilePath;
                Process.Start(filePath);
            }

            if (GUILayout.Button("Copy recent logs to clipboard")) GUIUtility.systemCopyBuffer = logContent;

            EditorGUILayout.LabelField("Recent logs:", styleLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.SelectableLabel(logContent, EditorStyles.textArea, GUILayout.ExpandHeight(true));
        }

        public static void Display()
        {
            NetMQInitializer.Publisher?.SendRequestInternalLog();

            // Get existing open window or if none, make a new one:
            FeedbackWindow? window = GetWindow<FeedbackWindow>();
            window.Show();

            Log.Debug("Displaying feedback window");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "FeedbackWindow_Display");
        }
    }
}