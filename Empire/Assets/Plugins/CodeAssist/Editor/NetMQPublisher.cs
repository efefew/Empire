using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Meryel.UnityCodeAssist.Editor.Logger;
using Meryel.UnityCodeAssist.Editor.TinyJson;
using Meryel.UnityCodeAssist.Synchronizer.Model;
using NetMQ;
using NetMQ.Sockets;
using Serilog;
using UnityEngine;
using Task = System.Threading.Tasks.Task;
//using CancellationToken = System.Threading;
using Application = UnityEngine.Application;
using GameObject = UnityEngine.GameObject;


#nullable enable


//**--
// can also do this for better clear, sometimes it gets locked
// https://answers.unity.com/questions/704066/callback-before-unity-reloads-editor-assemblies.html#

namespace Meryel.UnityCodeAssist.Editor
{
    public class NetMQPublisher : IProcessor
    {
        private readonly string pubConnString;

        private readonly Manager syncMngr;

        private Connect? _self;

        public List<Connect> clients;

        private bool isBind;
        private PublisherSocket? pubSocket;

        private Task? pullTask;
        private CancellationTokenSource? pullTaskCancellationTokenSource;

        public NetMQPublisher()
        {
            // LogContext();

            Log.Debug("NetMQ server initializing, begin");

            InitializeSelf();

            clients = new List<Connect>();
            syncMngr = new Manager(this);

            (string pubSub, string pushPull) = Utilities.GetConnectionString(Self!.ProjectPath);
            pubConnString = pubSub;

            //NetMQConfig.Linger = new TimeSpan(0);

            //pub = new Publisher();
            pubSocket = new PublisherSocket();

            pubSocket.Options.SendHighWatermark = 1000;
            Log.Debug("NetMQ server initializing, Publisher socket binding... {PubConnString}", pubConnString);
            //pubSocket.Bind("tcp://127.0.0.1:12349");


            try
            {
                pubSocket.Bind(pubConnString);
                isBind = true;
                Log.Debug("NetMQ server initializing, Publisher socket bound");
            }
            catch (AddressAlreadyInUseException ex)
            {
                Log.Warning(ex, "NetMQ.AddressAlreadyInUseException");
                LogContext();
                Log.Warning("NetMQ.AddressAlreadyInUseException disposing pubSocket");
                pubSocket.Dispose();
                pubSocket = null;
                return;
            }
            catch (SocketException ex)
            {
                Log.Warning(ex, "Socket exception");
                LogContext();
                Log.Warning("Socket exception disposing pubSocket");
                pubSocket.Dispose();
                pubSocket = null;
                return;
            }


            //pubSocket.SendReady += PubSocket_SendReady;
            //SendConnect();

            pullTaskCancellationTokenSource = new CancellationTokenSource();
            //pullThread = new System.Threading.Thread(async () => await PullAsync(conn.pushPull, pullThreadCancellationTokenSource.Token));
            //pullThread = new System.Threading.Thread(() => InitPull(conn.pushPull, pullTaskCancellationTokenSource.Token));
            //pullThread.Start();
            //Task.Run(() => InitPullAsync());

            /*
            pullTask = Task.Factory.StartNew(
                () => InitPull(conn.pushPull, pullTaskCancellationTokenSource.Token), pullTaskCancellationTokenSource.Token,
                System.Threading.Tasks.TaskCreationOptions.LongRunning, System.Threading.Tasks.TaskScheduler.Current);
            */


            pullTask = Task.Factory.StartNew(
                () => InitPull(pushPull, pullTaskCancellationTokenSource.Token),
                TaskCreationOptions.LongRunning);

            //InitPull(conn.pushPull);

            Log.Debug("NetMQ server initializing, initialized");

            // need to sleep here, clients will take some time to start subscribing
            // https://github.com/zeromq/netmq/issues/482#issuecomment-182200323
            Thread.Sleep(1000);
            SendConnect();
        }

        private Connect Self => _self!;


        string IProcessor.Serialize<T>(T value)
        {
            //return System.Text.Json.JsonSerializer.Serialize<T>(value);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return SerializeObject(value);
        }

        T IProcessor.Deserialize<T>(string data)
        {
            //return System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data)!;
            return JsonParser.FromJson<T>(data)!;

            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
            //T val = OdinSerializer.SerializationUtility.DeserializeValue<T>(buffer, OdinSerializer.DataFormat.JSON);
            //return val;
        }

        //**--make sure all Synchronizer.Model.IProcessor.Process methods are thread-safe

        // a new client has connected
        void IProcessor.Process(Connect connect)
        {
            if (connect.ModelVersion != Self.ModelVersion)
            {
                Log.Error(
                    "Version mismatch with {ContactInfo}. Please update your asset and reinstall the Visual Studio extension. {ContactModel} != {SelfModel}",
                    connect.ContactInfo, connect.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connect.ProjectPath != Self.ProjectPath)
            {
                Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'", connect.ProjectName,
                    connect.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!clients.Any(c => c.ContactInfo == connect.ContactInfo)) clients.Add(connect);

            SendHandshake();
            if (ScriptFinder.GetActiveGameObject(out GameObject? activeGO))
                SendGameObject(activeGO);
            Assister.SendTagsAndLayers();
        }

        // a new client is online and requesting connection
        void IProcessor.Process(RequestConnect requestConnect)
        {
            SendConnect();
        }

        void IProcessor.Process(Disconnect disconnect)
        {
            Connect? client = clients.FirstOrDefault(c => c.ContactInfo == disconnect.ContactInfo);
            if (client == null)
                return;

            clients.Remove(client);
        }

        void IProcessor.Process(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.ModelVersion != Self.ModelVersion)
            {
                Log.Error(
                    "Version mismatch with {ContactInfo}. Please update your asset and reinstall the Visual Studio extension. {ContactModel} != {SelfModel}",
                    connectionInfo.ContactInfo, connectionInfo.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connectionInfo.ProjectPath != Self.ProjectPath)
            {
                Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'",
                    connectionInfo.ProjectName, connectionInfo.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!clients.Any(c => c.ContactInfo == connectionInfo.ContactInfo))
            {
                SendConnect();
            }
            else
            {
                SendHandshake();
                if (ScriptFinder.GetActiveGameObject(out GameObject? activeGO))
                    SendGameObject(activeGO);
                Assister.SendTagsAndLayers();
            }
        }

        void IProcessor.Process(RequestConnectionInfo requestConnectionInfo)
        {
            SendConnectionInfo();
        }

        /*
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Layers layers)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Tags tags)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.SortingLayers sortingLayers)
        {

        }*/
        void IProcessor.Process(StringArray stringArray)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArray)");
        }

        void IProcessor.Process(StringArrayContainer stringArrayContainer)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArrayContainer)");
        }

        void IProcessor.Process(Synchronizer.Model.GameObject gameObject)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.GameObject)");
        }

        void IProcessor.Process(ComponentData component)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ComponentData)");
        }

        void IProcessor.Process(RequestScript requestScript)
        {
            if (requestScript.DeclaredTypes == null || requestScript.DeclaredTypes.Length == 0)
                return;

            string? documentPath = requestScript.DocumentPath;

            foreach (string? declaredType in requestScript.DeclaredTypes)
                if (ScriptFinder.FindInstanceOfType(declaredType, documentPath, out GameObject? go,
                        out ScriptableObject? so))
                {
                    if (go != null)
                        SendGameObject(go);
                    else if (so != null)
                        SendScriptableObject(so);
                    else
                        Log.Warning("Invalid instance of type");
                }
                else
                {
                    SendScriptMissing(declaredType);
                }
        }

        void IProcessor.Process(ScriptMissing scriptMissing)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ScriptMissing)");
        }


        void IProcessor.Process(Handshake handshake)
        {
            // Do nothing
        }

        void IProcessor.Process(RequestInternalLog requestInternalLog)
        {
            SendInternalLog();
        }

        void IProcessor.Process(InternalLog internalLog)
        {
            ELogger.VsInternalLog = internalLog.LogContent;
        }

        void IProcessor.Process(AnalyticsEvent analyticsEvent)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.AnalyticsEvent)");
        }

        void IProcessor.Process(ErrorReport errorReport)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ErrorReport)");
        }

        void IProcessor.Process(RequestVerboseType requestVerboseType)
        {
            Log.Warning(
                "Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestVerboseType)");
        }

        void IProcessor.Process(RequestLazyLoad requestLazyLoad)
        {
            Monitor.LazyLoad(requestLazyLoad.Category);
        }

        private void InitializeSelf()
        {
            string projectPath = CommonTools.GetProjectPath();
            _self = new Connect
            {
                ModelVersion = Utilities.Version,
                ProjectPath = projectPath,
                ProjectName = getProjectName(),
                ContactInfo = $"Unity {Application.unityVersion}",
                AssemblyVersion = Assister.Version
            };

            string getProjectName()
            {
                string[] s = projectPath.Split('/');
                string projectName = s[s.Length - 2];
                //Logg("project = " + projectName);
                return projectName;
            }
        }


        public static void LogContext()
        {
            Log.Debug("LogginContext begin");

            //var context = typeof(NetMQConfig).GetProperty("Context", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
            object? context = typeof(NetMQConfig).GetField("s_ctx", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
            Log.Debug("context: {Context}", context);

            if (context == null)
                return;

            object? starting = context.GetType().GetField("m_starting", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(context);
            Log.Debug("starting: {Starting}", starting);

            object? terminating = context.GetType()
                .GetField("m_terminating", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context);
            Log.Debug("terminating: {Terminating}", terminating);

            object? sockets = context.GetType().GetField("m_sockets", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(context);
            //Logg("sockets:" + sockets);
            //var socketList = sockets as System.Collections.IList;
            if (sockets is IList socketList)
            {
                Log.Debug("socketList: {SocketList} [{Count}]", socketList, socketList.Count);

                foreach (object? socketItem in socketList) Log.Debug("socketItem: {SocketItem}", socketItem);
            }

            object? endPoints = context.GetType()
                .GetField("m_endpoints", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context);
            //Logg("endPoints:" + endPoints);
            //var endPointDict = endPoints as System.Collections.IDictionary;
            if (endPoints is IDictionary endPointDict)
            {
                Log.Debug("endPointDict: {EndPointDict} ,{Count}", endPointDict, endPointDict.Count);

                foreach (object? endPointDictKey in endPointDict.Keys)
                    Log.Debug("endPointDictKey: {EndPointDictKey} => {EndPointDictValue}", endPointDictKey,
                        endPointDict[endPointDictKey]);
            }

            Log.Debug("LogginContext end");
        }


        private void InitPull(string connectionString, CancellationToken cancellationToken)
        {
            using (NetMQRuntime runtime = new())
            {
                runtime.Run( //cancellationToken,
                    PullAsync(connectionString, cancellationToken)
                );
                Log.Debug("Puller runtime ended");
            }

            Log.Debug("Puller runtime disposed");
        }

        private async Task PullAsync(string connectionString, CancellationToken cancellationToken)
        {
            Log.Debug("Puller begin");
            using (PullSocket pullSocket = new(connectionString))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string header, content;
                    try
                    {
                        (string, bool) headerTuple = await pullSocket.ReceiveFrameStringAsync(cancellationToken);
                        (string, bool) contentTuple = await pullSocket.ReceiveFrameStringAsync(cancellationToken);
                        header = headerTuple.Item1;
                        content = contentTuple.Item1;
                    }
                    catch (TaskCanceledException)
                    {
                        // Cancellation (token) requested
                        break;
                    }

                    Log.Debug("Pulled: {Header}, {Content}", header, content);

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    //**--optimize here, pass only params
                    MainThreadDispatcher.Add(() => syncMngr.ProcessMessage(header, content));
                    //syncMngr.ProcessMessage(header.Item1, content.Item1);
                }

                Log.Debug("Puller closing");

                pullSocket.Unbind(connectionString);
                pullSocket.Close();

                Log.Debug("Puller closed");
            }

            Log.Debug("Puller disposed");
        }

        public void Clear()
        {
            // LogContext();

            Log.Debug("NetMQ clearing, begin 1, pullTaskCancellationTokenSource: {PullTaskCancellationTokenSource}",
                pullTaskCancellationTokenSource);
            pullTaskCancellationTokenSource?.Cancel();

            Log.Verbose("NetMQ clearing, begin 2, pubSocket: {PubSocket}", pubSocket);
            string? pubSocketDebugStr = pubSocket?.ToString();
            Log.Debug("NetMQ clearing, begin 3, isBind: {IsBind}", isBind);
            if (isBind)
            {
                pubSocket?.Unbind(pubConnString);
                isBind = false;
            }

            Log.Verbose("NetMQ clearing, begin 4");
            pubSocket?.Close();
            Log.Verbose("NetMQ clearing, begin 5");
            pubSocket?.Dispose();
            Log.Verbose("NetMQ clearing, begin 6");
            pubSocket = null;
            Log.Debug("NetMQ clearing, publisher closed pubSocketDebugStr: {PubSocketDebugStr}", pubSocketDebugStr);

            try
            {
                Log.Debug("NetMQ clearing, Task 1 begin");

                if (pullTask != null && !pullTask.Wait(1000))
                    Log.Warning("NetMQ clearing, pull task termination failed");

                Log.Verbose("NetMQ clearing, Task 2 waited");

                pullTask?.Dispose();
                pullTask = null;

                Log.Debug("NetMQ clearing, Task 3 disposed");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
                Log.Error(ex, "NetMQ clearing, pull task");
            }
            finally
            {
                pullTaskCancellationTokenSource?.Dispose();
                pullTaskCancellationTokenSource = null;
                Log.Debug("NetMQ clearing, task cancelled");
            }

            Log.Debug("NetMQ clearing, cleaning up");
            //pullSocket?.Close();
            NetMQConfig.Cleanup(
                false); // Must be here to work more than once. Also argument false is important, otherwise might freeze Unity upon exit or domain reload
            //pullThread?.Abort();
            Log.Debug("NetMQ clearing, cleared");
        }

        private string SerializeObject<T>(T obj)
            where T : class
        {
            // Odin cant serialize string arrays, https://github.com/TeamSirenix/odin-serializer/issues/26
            //var buffer = OdinSerializer.SerializationUtility.SerializeValue<T>(obj, OdinSerializer.DataFormat.JSON);
            //var str = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            // Newtonsoft works fine, but needs package reference
            //var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            // not working
            //var str = EditorJsonUtility.ToJson(obj);

            // needs nuget
            //System.Text.Json.JsonSerializer;

            string? str = JsonWriter.ToJson(obj);

            return str;
        }

        private void SendAux(IMessage message, bool logContent = true)
        {
            if (message == null)
                return;

            SendAux(message.GetType().Name, message, logContent);
        }

        private void SendAux(string messageType, object content, bool logContent = true)
        {
            if (logContent)
                Log.Debug("Publishing {MessageType} {@Content}", messageType, content);
            else
                Log.Debug("Publishing {MessageType}", messageType);

            PublisherSocket? publisher = pubSocket;
            if (publisher != null)
                publisher.SendMoreFrame(messageType).SendFrame(SerializeObject(content));
            else
                Log.Error("Publisher socket is null");
        }

        public void SendConnect()
        {
            Connect? connect = Self;

            SendAux(connect);
        }

        public void SendDisconnect()
        {
            Disconnect disconnect = new()
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion
            };

            SendAux(disconnect);
        }

        public void SendConnectionInfo()
        {
            ConnectionInfo connectionInfo = new()
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion
            };

            SendAux(connectionInfo);
        }

        public void SendHandshake()
        {
            Handshake handshake = new();

            SendAux(handshake);
        }

        public void SendRequestInternalLog()
        {
            RequestInternalLog requestInternalLog = new();

            SendAux(requestInternalLog);
        }

        public void SendInternalLog()
        {
            InternalLog internalLog = new()
            {
                LogContent = ELogger.GetInternalLogContent()
            };

            SendAux(internalLog, false);
        }


        private void SendStringArrayAux(string id, string[] array)
        {
            StringArray stringArray = new()
            {
                Id = id,
                Array = array
            };

            SendAux(stringArray);
        }

        private void SendStringArrayContainerAux(params (string id, string[] array)[] container)
        {
            StringArrayContainer stringArrayContainer = new()
            {
                Container = new StringArray[container.Length]
            };

            for (int i = 0; i < container.Length; i++)
                stringArrayContainer.Container[i] = new StringArray
                {
                    Id = container[i].id,
                    Array = container[i].array
                };

            SendAux(stringArrayContainer);
        }

        public void SendTags(string[] tags)
        {
            SendStringArrayAux(Ids.Tags, tags);
        }

        /*
        {

            var tags = new Synchronizer.Model.Tags()
            {
                TagArray = tagArray,
            };

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(tags);

            pubSocket.SendMoreFrame(nameof(Synchronizer.Model.Tags)).SendFrame(serialized);


        }*/

        public void SendLayers(string[] layerIndices, string[] layerNames)
        {
            /*
            var layers = new Synchronizer.Model.Layers()
            {
                LayerIndices = layerIndices,
                LayerNames = layerNames,
            };

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(layers);

            pubSocket.SendMoreFrame(nameof(Synchronizer.Model.Layers)).SendFrame(serialized);
            */

            //SendStringArrayAux(Synchronizer.Model.Ids.Layers, layerNames);
            //SendStringArrayAux(Synchronizer.Model.Ids.LayerIndices, layerIndices);
            SendStringArrayContainerAux(
                (Ids.Layers, layerNames),
                (Ids.LayerIndices, layerIndices)
            );
        }

        public void SendSortingLayers(string[] sortingLayers, string[] sortingLayerIds, string[] sortingLayerValues)
        {
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayers, sortingLayers);
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayerIds, sortingLayerIds);
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayerValues, sortingLayerValues);

            SendStringArrayContainerAux(
                (Ids.SortingLayers, sortingLayers),
                (Ids.SortingLayerIds, sortingLayerIds),
                (Ids.SortingLayerValues, sortingLayerValues)
            );
        }

        public void SendPlayerPrefs(string[] playerPrefKeys, string[] playerPrefValues,
            string[] playerPrefStringKeys, string[] playerPrefIntegerKeys, string[] playerPrefFloatKeys)
        {
            SendStringArrayContainerAux(
                (Ids.PlayerPrefKeys, playerPrefKeys),
                (Ids.PlayerPrefValues, playerPrefValues),
                (Ids.PlayerPrefStringKeys, playerPrefStringKeys),
                (Ids.PlayerPrefIntegerKeys, playerPrefIntegerKeys),
                (Ids.PlayerPrefFloatKeys, playerPrefFloatKeys)
            );
        }

        public void SendEditorPrefs(string[] editorPrefKeys, string[] editorPrefValues,
            string[] editorPrefStringKeys, string[] editorPrefIntegerKeys, string[] editorPrefFloatKeys,
            string[] editorPrefBooleanKeys)
        {
            SendStringArrayContainerAux(
                (Ids.EditorPrefKeys, editorPrefKeys),
                (Ids.EditorPrefValues, editorPrefValues),
                (Ids.EditorPrefStringKeys, editorPrefStringKeys),
                (Ids.EditorPrefIntegerKeys, editorPrefIntegerKeys),
                (Ids.EditorPrefFloatKeys, editorPrefFloatKeys),
                (Ids.EditorPrefBooleanKeys, editorPrefBooleanKeys)
            );
        }

        public void SendInputManager(string[] axisNames, string[] axisInfos, string[] buttonKeys, string[] buttonAxis,
            string[] joystickNames)
        {
            SendStringArrayContainerAux(
                (Ids.InputManagerAxes, axisNames),
                (Ids.InputManagerAxisInfos, axisInfos),
                (Ids.InputManagerButtonKeys, buttonKeys),
                (Ids.InputManagerButtonAxis, buttonAxis),
                (Ids.InputManagerJoystickNames, joystickNames)
            );
        }

        public void SendScriptMissing(string component)
        {
            ScriptMissing scriptMissing = new()
            {
                Component = component
            };

            SendAux(scriptMissing);
        }

        public void SendGameObject(GameObject go)
        {
            if (!go)
                return;

            Log.Debug("SendGO: {GoName}", go.name);

            Synchronizer.Model.GameObject? dataOfSelf = go.ToSyncModel(10000);
            if (dataOfSelf != null)
                SendAux(dataOfSelf);

            var dataOfHierarchy = go.ToSyncModelOfHierarchy();
            if (dataOfHierarchy != null)
                foreach (Synchronizer.Model.GameObject? doh in dataOfHierarchy)
                    SendAux(doh);

            var dataOfComponents = go.ToSyncModelOfComponents();
            if (dataOfComponents != null)
                foreach (ComponentData? doc in dataOfComponents)
                    SendAux(doc);
        }

        public void SendScriptableObject(ScriptableObject so)
        {
            Log.Debug("SendSO: {SoName}", so.name);

            ComponentData? dataOfSo = so.ToSyncModel();
            if (dataOfSo != null)
                SendAux(dataOfSo);
        }

        public void SendAnalyticsEvent(string type, string content)
        {
            AnalyticsEvent dataOfAe = new()
            {
                EventType = type,
                EventContent = content
            };
            SendAux(dataOfAe);
        }

        public void SendErrorReport(string errorMessage, string stack, string type)
        {
            ErrorReport dataOfER = new()
            {
                ErrorMessage = errorMessage,
                ErrorStack = stack,
                ErrorType = type
            };
            SendAux(dataOfER);
        }

        public void SendRequestVerboseType(string type, string docPath)
        {
            RequestVerboseType dataOfRVT = new()
            {
                Type = type,
                DocPath = docPath
            };
            SendAux(dataOfRVT);
        }
    }
}