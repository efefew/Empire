#nullable enable


using System.Linq;
using Meryel.UnityCodeAssist.Synchronizer.Model;
using Serilog;
using UnityEditor;
using UnityEngine;

namespace Meryel.UnityCodeAssist.Editor
{
    public class StatusWindow : EditorWindow
    {
        private GUIStyle? styleLabel;

        private void OnEnable()
        {
            //**--icon
            //var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png");
            //titleContent = new GUIContent("Code Assist", icon);
            titleContent = new GUIContent(Assister.Title);
        }

        private void OnGUI()
        {
            bool hasAnyClient = NetMQInitializer.Publisher?.clients.Any() == true;

            styleLabel ??= new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft
            };

            if (hasAnyClient)
            {
                EditorGUILayout.LabelField("Code Assist is working!", styleLabel, GUILayout.ExpandWidth(true));

                foreach (Connect? client in NetMQInitializer.Publisher!.clients)
                    EditorGUILayout.LabelField($"Connected to {client.ContactInfo}", styleLabel,
                        GUILayout.ExpandWidth(true));
            }
            else
            {
                EditorGUILayout.LabelField("Code Assist isn't working!", styleLabel, GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField("No IDE found", styleLabel, GUILayout.ExpandWidth(true));
            }

#if MERYEL_UCA_LITE_VERSION

            EditorGUILayout.LabelField("", styleLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("This is the lite version of Code Assist with limited features.", styleLabel,
                GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("To unlock all of the features, get the full version.", styleLabel,
                GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Get full version")) Application.OpenURL("http://u3d.as/2N2H");

#endif // MERYEL_UCA_LITE_VERSION
        }

        public static void Display()
        {
            // Get existing open window or if none, make a new one:
            StatusWindow? window = GetWindow<StatusWindow>();
            window.Show();

            NetMQInitializer.Publisher?.SendConnectionInfo();

            Log.Debug("Displaying status window");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "StatusWindow_Display");
        }
    }
}