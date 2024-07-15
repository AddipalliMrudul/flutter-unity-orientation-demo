using System;
using JungleeGames;
using UnityEngine;
using UnityEngine.SceneManagement;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
#if UNITY_IOS && !UNITY_EDITOR
using FlutterUnityIntegration;
#endif

namespace XcelerateGames.FlutterWidget
{
    public class UnityMessageManager : BaseBehaviour
    {
        [SerializeField] bool _SendExceptionsToFlutter = true;

#region Signals        
        [InjectSignal] private SigOnFlutterMessage mSigOnFlutterMessage = null;
#if !UNITY_STANDALONE
        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null;
        [InjectSignal] private SigSendLogToFlutter mSigSendLogToFlutter = null;
        #endif
#endregion

        public static UnityMessageManager Instance { get; private set; }
        public bool BackToLobby { get; set; }

#if UNITY_ANDROID
        private AndroidJavaClass mPluginClass = null;
        private AndroidJavaClass mPluginClassOld = null;
#endif
        protected override void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                //Debug.Log($"LTS: UnityMessageManager awake started : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
                Instance = this;
                base.Awake();
                DontDestroyOnLoad(gameObject);
#if !UNITY_STANDALONE
                Application.logMessageReceived += OnDebugLogCallbackHandler;
#endif

#if UNITY_ANDROID
                XDebug.Log("no notification currently been shown on screen", UnityMessageManager.getSDKInt().ToString());
                if (UnityMessageManager.getSDKInt() <= 24)
                {
                    mPluginClassOld = new AndroidJavaClass("com.xraph.plugins.flutterunitywidgetold.UnityUtils");
                }
                else
                {
                    string pluginPath = "com.xraph.plugins.flutterunitywidget.UnityUtils";

#if FLUTTER_WIDGET_4_AND_ABOVE
                    pluginPath = "com.xraph.plugin.flutter_unity_widget.UnityPlayerUtils";
#endif //FLUTTER_WIDGET_4_AND_ABOVE
                    mPluginClass = new AndroidJavaClass(pluginPath);
                    if (mPluginClass == null)
                        XDebug.LogException($"Failed to find android {pluginPath}");
                }
#endif
                //Debug.Log($"LTS: UnityMessageManager awake ended : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
#if !UNITY_STANDALONE
            mSigSendMessageToFlutter.AddListener(SendMessageToFlutter);
            mSigSendLogToFlutter.AddListener(SendLogToFlutter);
#endif
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
#if !UNITY_STANDALONE
                mSigSendLogToFlutter.RemoveListener(SendLogToFlutter);
                mSigSendMessageToFlutter.RemoveListener(SendMessageToFlutter);
                Application.logMessageReceived -= OnDebugLogCallbackHandler;
#endif
                base.OnDestroy();
            }
        }

        static int getSDKInt()
        {
#if UNITY_EDITOR
            return 30;
#else
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
#endif
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
#if UNITY_ANDROID
            try
            {
                //Debug.Log($"LTS: UnityMessageManager OnSceneLoaded started : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
                if (mPluginClass != null)
                {
                    mPluginClass.CallStatic("onUnitySceneLoaded", scene.name, scene.buildIndex, scene.isLoaded, scene.IsValid());
                }
                if (mPluginClassOld != null)
                {
                    mPluginClassOld.CallStatic("onUnitySceneLoaded", scene.name, scene.buildIndex, scene.isLoaded, scene.IsValid());
                }
                //Debug.Log($"LTS: UnityMessageManager OnSceneLoaded ended : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
            }
            catch (Exception e)
            {
                XDebug.LogException(e.Message);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            NativeAPI.OnUnitySceneLoaded(scene.name, scene.buildIndex, scene.isLoaded, scene.IsValid());
#endif
        }

        void SendMessageToFlutter(FlutterMessage flutterMessage)
        {
            Debug.Log("DevLogs :: SendMessageToFlutter :: " + flutterMessage.type);

            if (ShouldLoadLobby(flutterMessage))
            {
                BackToLobby = true;
#if STANDALONE_GAME
                //Do nothing
                SendMessageToFlutter(new FlutterMessage() { type = FlutterMessageType.GameEnd }.ToJson());
                Debug.Log($"{DateTime.Now.ToLongTimeString()} : SendMessageToFlutter: GameEnd");
                flutterMessage.type = FlutterMessageType.BackToLobby;
#elif UMBRELLA
                ResourceManager.LoadScene("startup");
#endif
            }
            var message = flutterMessage.ToJson();
            if(XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"{DateTime.Now.ToLongTimeString()} : SendMessageToFlutter: {flutterMessage.type} \"{message}\"", XDebug.Mask.Game);
            SendMessageToFlutter(message);
        }


        bool ShouldLoadLobby(FlutterMessage flutterMessage)
        {
            bool loadLobby = false;
            switch (flutterMessage.type)
            {
                case FlutterMessageType.GameEnd:
                case FlutterMessageType.GameTableFTUESkipped:
                case FlutterMessageType.GameTableFTUEComplete:
                case FlutterMessageType.GameTableFTUERewardFailed:
                case FlutterMessageType.FillDetails:
                case FlutterMessageType.TableQuited:
                case FlutterMessageType.ShowUI:
                    loadLobby = true;
                    break;
                case FlutterMessageType.ShowUiOverlay:
                case FlutterMessageType.ShowReportProblem:
                    break;
            }

            return loadLobby;
        }

        private void SendMessageToFlutter(string message)
        {
#if UNITY_ANDROID
            try
            {
                if (mPluginClass != null)
                {
                    mPluginClass.CallStatic("onUnityMessage", message);
                }
                if (mPluginClassOld != null)
                {
                    mPluginClassOld.CallStatic("onUnityMessage", message);
                }
            }
            catch (Exception e)
            {
                XDebug.LogException(e.Message);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            NativeAPI.OnUnityMessage(message);
#endif
        }

        private void SendLogToFlutter(FlutterMessage flutterMessage)
        {
            SendMessageToFlutter(flutterMessage.ToJson());
        }

        private void OnDebugLogCallbackHandler(string logString, string inStack, LogType inType)
        {
            if (_SendExceptionsToFlutter && inType == LogType.Exception)
            {
                SendMessageToFlutter(new FlutterMessage()
                {
                    type = "UnityException",
                    data = $"{logString}\n{inStack}"
                });
            }
        }

        void onMessage(string message)
        {
            if(XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"{DateTime.Now.ToLongTimeString()} : MessageFromFlutter: \"{message}\"", XDebug.Mask.Game);
            mSigOnFlutterMessage.Dispatch(message.FromJson<FlutterMessage>());
        }

        void pause()
        {
            Debug.Log($"pause called from flutter");
        }

        void resume()
        {
            Debug.Log($"resume called from flutter");
        }

        void quitPlayer()
        {
            Debug.Log($"quitPlayer called from flutter");
        }

        void silentQuitPlayer()
        {
            Debug.Log($"silentQuitPlayer called from flutter");
        }

        void dispose()
        {
            Debug.Log($"dispose called from flutter");
        }
    }
}