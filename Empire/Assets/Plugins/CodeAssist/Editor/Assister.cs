#nullable enable


using System.Collections;
using System.Linq;
using Meryel.UnityCodeAssist.Editor.EditorCoroutines;
using Serilog;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Meryel.UnityCodeAssist.Editor
{
    public class Assister
    {
        public const string Version = "1.1.6";

#if MERYEL_UCA_LITE_VERSION
        public const string Title = "Code Assist Lite";
#else
        public const string Title = "Code Assist";
#endif

        [MenuItem("Tools/" + Title + "/Status", false, 1)]
        private static void DisplayStatusWindow()
        {
            StatusWindow.Display();
        }


        [MenuItem("Tools/" + Title + "/Synchronize", false, 2)]
        private static void Sync()
        {
            EditorCoroutineUtility.StartCoroutine(SyncAux(), NetMQInitializer.Publisher);

            //NetMQInitializer.Publisher.SendConnect();
            //Serilog.Log.Information("Code Assist is looking for more IDEs to connect to...");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "Synchronize_MenuItem");
        }


        [MenuItem("Tools/" + Title + "/Report error", false, 51)]
        private static void DisplayFeedbackWindow()
        {
            FeedbackWindow.Display();
        }

        [MenuItem("Tools/" + Title + "/About", false, 52)]
        private static void DisplayAboutWindow()
        {
            AboutWindow.Display();
        }

#if MERYEL_UCA_LITE_VERSION
        [MenuItem("Tools/" + Title + "/Compare versions", false, 31)]
        private static void CompareVersions()
        {
            Application.OpenURL("http://unitycodeassist.netlify.app/compare");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "CompareVersions_MenuItem");
        }

        [MenuItem("Tools/" + Title + "/Get full version", false, 32)]
        private static void GetFullVersion()
        {
            Application.OpenURL("http://u3d.as/2N2H");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "FullVersion_MenuItem");
        }
#endif // MERYEL_UCA_LITE_VERSION


        private static IEnumerator SyncAux()
        {
            int clientCount = NetMQInitializer.Publisher?.clients.Count ?? 0;
            NetMQInitializer.Publisher?.SendConnect();
            Log.Information("Code Assist is looking for more IDEs to connect to...");

            //yield return new WaitForSeconds(3);
            yield return new EditorWaitForSeconds(3);

            int newClientCount = NetMQInitializer.Publisher?.clients.Count ?? 0;

            int dif = newClientCount - clientCount;

            if (dif <= 0)
                Log.Information("Code Assist couldn't find any new IDE to connect to.");
            else
                Log.Information("Code Assist is connected to {Dif} new IDE(s).", dif);
        }

#if MERYEL_DEBUG
        [MenuItem("Code Assist/Binary2Text")]
        static void Binary2Text()
        {
            var filePath = CommonTools.GetInputManagerFilePath();
            var hash = Input.UnityInputManager.GetMD5Hash(filePath);
            var convertedPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"UCA_IM_{hash}.txt");
            
            var b = new Input.Binary2TextExec();
            b.Exec(filePath, convertedPath, detailed: false, largeBinaryHashOnly: false, hexFloat: false);
        }

        [MenuItem("Code Assist/Bump InputManager")]
        static void BumpInputManager()
        {
            Input.InputManagerMonitor.Instance.Bump();
        }


        [MenuItem("Code Assist/Layer Check")]
        static void UpdateLayers()
        {
            var names = UnityEditorInternal.InternalEditorUtility.layers;
            var indices = names.Select(l => LayerMask.NameToLayer(l).ToString()).ToArray();
            NetMQInitializer.Publisher?.SendLayers(indices, names);

            var sls = SortingLayer.layers;
            var sortingNames = sls.Select(sl => sl.name).ToArray();
            var sortingIds = sls.Select(sl => sl.id.ToString()).ToArray();
            var sortingValues = sls.Select(sl => sl.value.ToString()).ToArray();

            NetMQInitializer.Publisher?.SendSortingLayers(sortingNames, sortingIds, sortingValues);

            /*
            for (var i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                {
                    Debug.Log(i + ":" + name);
                }
            }

            if (ScriptFinder.FindGameObjectOfType("Deneme", out var go))
                NetMQInitializer.Publisher.SendGameObject(go);
            */
        }

        [MenuItem("Code Assist/Tag Check")]
        static void UpdateTags()
        {
            Serilog.Log.Debug("Listing tags {Count}", UnityEditorInternal.InternalEditorUtility.tags.Length);

            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    Serilog.Log.Debug("{Tag}", tag);
                }
            }

            NetMQInitializer.Publisher?.SendTags(UnityEditorInternal.InternalEditorUtility.tags);

        }

        [MenuItem("Code Assist/GO Check")]

        static void TestGO()
        {

            var go = GameObject.Find("Deneme");
            //var go = MonoBehaviour.FindObjectOfType<Deneme>().gameObject;

            NetMQInitializer.Publisher?.SendGameObject(go);
        }

        [MenuItem("Code Assist/Undo Records Test")]
        static void UndoTest()
        {
            var undos = new List<string>();
            var redos = new List<string>();

            var type = typeof(Undo);
            System.Reflection.MethodInfo dynMethod = type.GetMethod("GetRecords",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            dynMethod.Invoke(null, new object[] { undos, redos });

            Serilog.Log.Debug("undos:{UndoCount},redos:{RedoCount}", undos.Count, redos.Count);

            var last = undos.LastOrDefault();
            if (last != null)
            {
                Serilog.Log.Debug("last:{Last}", last);
                Serilog.Log.Debug("group:{UndoCurrentGroup},{UndoCurrentGroupName}",
                    Undo.GetCurrentGroup(), Undo.GetCurrentGroupName());
            }
        }


        [MenuItem("Code Assist/Undo List Test")]
        static void Undo2Test()
        {

            //List<string> undoList, out int undoCursor
            var undoList = new List<string>();
            int undoCursor = int.MaxValue;
            var type = typeof(Undo);
            System.Reflection.MethodInfo dynMethod = type.GetMethod("GetUndoList",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            dynMethod = type.GetMethod("GetUndoList",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                null,
                new System.Type[] { typeof(List<string>), typeof(int).MakeByRefType() },
                null);


            dynMethod.Invoke(null, new object[] { undoList, undoCursor });

            Serilog.Log.Debug("undo count: {Count}", undoList.Count);

        }

        [MenuItem("Code Assist/Reload Domain")]
        static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();

        }


        /*
        [MenuItem("Code Assist/TEST")]
        static void TEST()
        {
            //if (ScriptFinder.FindGameObjectOfType("Deneme_OtherScene", out var go))
            if (ScriptFinder.FindInstanceOfType("Deneme_SO", out var go, out var so))
            {
                NetMQInitializer.Publisher.SendScriptableObject(so);
            }

            ScriptFinder.DENEMEEEE();



        }
        */

#endif // MERYEL_DEBUG


        public static void SendTagsAndLayers()
        {
            Log.Debug(nameof(SendTagsAndLayers));

            string[]? tags = InternalEditorUtility.tags;
            NetMQInitializer.Publisher?.SendTags(tags);

            string[]? names = InternalEditorUtility.layers;
            string[]? indices = names.Select(l => LayerMask.NameToLayer(l).ToString()).ToArray();
            NetMQInitializer.Publisher?.SendLayers(indices, names);

            var sls = SortingLayer.layers;
            string[]? sortingNames = sls.Select(sl => sl.name).ToArray();
            string[]? sortingIds = sls.Select(sl => sl.id.ToString()).ToArray();
            string[]? sortingValues = sls.Select(sl => sl.value.ToString()).ToArray();
            NetMQInitializer.Publisher?.SendSortingLayers(sortingNames, sortingIds, sortingValues);
        }
    }
}