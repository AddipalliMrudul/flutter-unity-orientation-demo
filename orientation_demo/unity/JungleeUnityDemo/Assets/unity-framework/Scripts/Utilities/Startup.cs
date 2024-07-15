using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.Audio;
using XcelerateGames.Debugging;
using XcelerateGames.IOC;
using XcelerateGames.Locale;
using XcelerateGames.Timer;

namespace XcelerateGames
{
    public class Startup : BaseBehaviour
    {
        #region Properties
        [SerializeField] protected bool _LoadSceneWhenDone = true;
        [SerializeField] protected string _SceneName = "game";
        [SerializeField] protected string _FBLoginSceneName = "fblogin";
        [SerializeField] protected string _AssetMappingAssetName = null;
        [SerializeField] protected float _CacheWaitTime = 3f;
        [SerializeField] protected float _FirebaseRemoteConfigWaitTime = 3f;
        [SerializeField] protected int _TargetFrameRate = 30;
        [SerializeField] protected string _MainActivity = "com.unity3d.player.UnityPlayer";
        [SerializeField] protected bool _MultiTouchEnabled = false;
        [SerializeField] protected bool _AutoInitFramework = true;
        [SerializeField] protected SleepTimeout _SleepTimeout = SleepTimeout.NeverSleep;
        [SerializeField] protected GameObjectData[] _GameObjectStateOnFrameworkInit = null;

        private float mElapsedTime = 0f;
#if USE_FIREBASE
        protected bool mFirebaseRemoteConfigTimedOut = false;
#endif //USE_FIREBASE
        #endregion //Properties

        #region Signals
        [InjectSignal] protected SigEngineReady mSigEngineReady = null;
        [InjectSignal] private SigFrameworkInited mSigFrameworkInited = null;
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods

        protected override void Awake()
        {
            Debug.Log($"Flags:{GetCompilerFlags().Printable(',')}");
            base.Awake();
            if (_AutoInitFramework)
                InitFramework();
        }

        protected virtual void InitFramework()
        {
            Screen.sleepTimeout = Utilities.GetSleepTimeoutValue(_SleepTimeout);
            Application.targetFrameRate = _TargetFrameRate;
            ServerTime.Init();
#if !USE_FLUTTER_TO_MAIL_LOGS
            DebugEmail.Init(_MainActivity);
#endif
            Vibration.Initialize(_MainActivity);
            Input.multiTouchEnabled = _MultiTouchEnabled;
            if (!_AssetMappingAssetName.IsNullOrEmpty())
                AssetConfigData.Init(_AssetMappingAssetName);
            StartCoroutine(WaitForCache());
        }

        private IEnumerator WaitForCache()
        {
            Debug.Log($"LTS: Waiting for cache to get ready : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
            FrameworkEventManager.LogEvent("unity_waiting_cache_ready");
            while (!Caching.ready)
            {
                if (mElapsedTime > _CacheWaitTime)
                {
                    Debug.Log($"LTS: Cache taking too long to get ready...not waiting anymore : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
                    break;
                }
                yield return null;
            }
            Debug.Log($"LTS: Cache ready... : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
            FrameworkEventManager.LogEvent("unity_cache_ready");
            OnCacheReady();
        }

        protected virtual void OnCacheReady()
        {
            ResourceManager.Init(OnResourceManagerReady);
        }

        protected virtual void OnResourceManagerReady()
        {
            FrameworkEventManager.LogEvent("unity_resourcemanager_init_complete");
            ResourceManager.UnregisterOnReadyCallback(OnResourceManagerReady);
            Localization.Init(OnLocalizationLoaded);
        }

        protected virtual void OnLocalizationLoaded(bool loaded)
        {
            FrameworkEventManager.LogEvent("unity_localization_init_completed");
            if (XDebug.CanLog(XDebug.Mask.Resources))
                Debug.Log("Localization Initialized");
            AudioController.Init();
            mSigEngineReady.Dispatch();
            mSigFrameworkInited.Dispatch();
            if (_LoadSceneWhenDone)
            {
                if (PlatformUtilities.IsFBSimulation())
                    ResourceManager.LoadScene(_FBLoginSceneName);
                else
                    ResourceManager.LoadScene(_SceneName);
            }
            if (!_GameObjectStateOnFrameworkInit.IsNullOrEmpty())
            {
                foreach (GameObjectData gameObjectData in _GameObjectStateOnFrameworkInit)
                {
                    if (gameObjectData != null)
                        gameObjectData.Apply();
                }
            }
        }
        #endregion //Private Methods

        #region Public Methods
        protected virtual void Update()
        {
            mElapsedTime += Time.deltaTime;
#if USE_FIREBASE
            if (!mFirebaseRemoteConfigTimedOut)
            {
                _FirebaseRemoteConfigWaitTime -= Time.deltaTime;
                if (_FirebaseRemoteConfigWaitTime < 0)
                {
                    mFirebaseRemoteConfigTimedOut = true;
                    OnFirebaseRemoteConfigTimedOut();
                }
            }
#endif //USE_FIREBASE
        }

        protected virtual void OnFirebaseRemoteConfigTimedOut()
        {
            XDebug.LogError("Firebase remote config loading timed out");
        }

        protected virtual List<string> GetCompilerFlags()
        {
            List<string> flags = new List<string>();
#if DEV_BUILD
            flags.Add("DEV_BUILD");
#endif
#if QA_BUILD
            flags.Add("QA_BUILD");
#endif
#if BETA_BUILD
            flags.Add("BETA_BUILD");
#endif
#if LIVE_BUILD
            flags.Add("LIVE_BUILD");
#endif
#if FB_ENABLED
            flags.Add("FB_ENABLED");
#endif
#if USE_NATIVE_WEBSOCKET
            flags.Add("USE_NATIVE_WEBSOCKET");
#endif
#if USE_WEBSOCKET_SHARP
            flags.Add("USE_WEBSOCKET_SHARP");
#endif
#if IN_MEMORY_LOGS
            flags.Add("IN_MEMORY_LOGS");
#endif
#if SIMULATE_FB_SDK
            flags.Add("SIMULATE_FB_SDK");
#endif
#if OTA_BUILD
            flags.Add("OTA_BUILD");
#endif
#if VIDEO_ENABLED
            flags.Add("VIDEO_ENABLED");
#endif
#if USE_LOCAL_NOTIFICATIONS
            flags.Add("USE_LOCAL_NOTIFICATIONS");
#endif
#if FIREBASE_CRASHLYTICS_ENABLED
            flags.Add("FIREBASE_CRASHLYTICS_ENABLED");
#endif
#if UNITY_FACEBOOK
            flags.Add("UNITY_FACEBOOK");
#endif
#if USE_FIREBASE
            flags.Add("USE_FIREBASE");
#endif
#if USE_IAP
            flags.Add("USE_IAP");
#endif
#if HANDLE_EXCEPTIONS
            flags.Add("HANDLE_EXCEPTIONS");
#endif
            return flags;
        }
        #endregion //Public Methods
    }
}
