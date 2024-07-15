using System;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.RemoteLogging
{
    public class RemoteLoggingBase : BaseBehaviour
    {
        #region Properties
        public bool _EnableLog = true;
        public bool _ShowLog = false;
        public bool _ShowWarning = true;
        public bool _ShowError = true;
        public bool _ShowStackTrace = true;
        public bool _SkipDuplicates = true;

        //0: all logs will be sent without any limit
        //public int _LogsPerMinute = 30;

#if UNITY_EDITOR
        public bool _EnableLogOnEditor = false;
#endif

        public static RemoteLoggingBase Instance { get; protected set; }

        protected Dictionary<string, string> mMeta = null;
        protected List<string> mSentMessageHash = new List<string>();
        protected Queue<LogData> mLogData = new Queue<LogData>();

        //Time in seconds
        protected float mTimer = 60f;
        protected int mCount = 0;

        //protected bool CanLog => mCount <= _LogsPerMinute;
        #endregion Properties

        #region Private/Protected Methods
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (!_EnableLogOnEditor)
                _EnableLog = false;
#endif

            if (_EnableLog)
            {
                Instance = this;
                GameObject.DontDestroyOnLoad(gameObject);
                ConnectivityMonitor.AddListener(OnNetworkStatusChanged);
                Application.logMessageReceived += OnDebugLogCallbackHandler;
            }
            else
                GameObject.Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceived -= OnDebugLogCallbackHandler;
            ConnectivityMonitor.RemoveListener(OnNetworkStatusChanged);
            base.OnDestroy();
        }

        private void OnNetworkStatusChanged(ConnectivityMonitor.Status status)
        {
            if (status == ConnectivityMonitor.Status.Online)
            {

            }
        }

        private void OnDebugLogCallbackHandler(string logString, string stackTrace, LogType logType)
        {
            CheckLoggingConfig(logString, stackTrace, logType);
        }

        /// <summary>
        /// Check and skip logs based on server config 
        /// </summary>
        private void CheckLoggingConfig(string logString, string stackTrace, LogType logType)
        {
            if (_EnableLog)
            {
                if (XDebug.mLogPriority != XDebug.Priority.Always)
                {
                    if (logType == LogType.Log && !_ShowLog)
                        return;

                    if (logType == LogType.Warning && !_ShowWarning)
                        return;

                    if ((logType == LogType.Error || logType == LogType.Exception || logType == LogType.Assert) && !_ShowError)
                        return;
                }

                if (!IsDuplicate(logString, stackTrace))
                {
                    AddToQueue(logString, stackTrace, logType);
                }
            }
        }

        protected virtual void AddToQueue(string logString, string stackTrace, LogType logType)
        {
            mLogData.Enqueue(new LogData() { logString = logString, stackTrace = stackTrace, logType = logType, time = DateTime.Now.ToString("dd:MM:yy HH:mm:ss.fff") });
            //Debug.Log($"Added to cache. Count {mLogData.Count}");
            if (mLogData.Count == 1)
                PostLog(logString, stackTrace, logType);
        }

        protected virtual void PostLog(string logString, string stackTrace, LogType logType)
        {
            //Debug.Log($"PostLog: {logString}");
        }

        protected virtual void Update()
        {
            mTimer -= Time.deltaTime;
            if (mTimer <= 0f)
            {
                OnTimerElapsed();
            }
        }

        protected virtual void OnTimerElapsed()
        {
            mTimer = 60f;
            mCount = 0;
        }

        protected virtual bool IsDuplicate(string logString, string stackTrace)
        {
            if (!_SkipDuplicates)
                return false;
            string hash = Utilities.MD5Hash(logString + stackTrace);
            if (mSentMessageHash.Contains(hash))
            {
                //Debug.Log($"Skipping duplicate log: {logString}");
                return true;
            }
            mSentMessageHash.Add(hash);
            return false;
        }

        protected void AddCommonData(WWWForm form)
        {
            form.AddField("Build", ProductSettings.pInstance._BuildNumber);
            form.AddField("Version", ProductSettings.GetCurrentProductInfo().Version);
            form.AddField("Platform", Application.platform.ToString());
            form.AddField("NetWorkType", Application.internetReachability.ToString());
            form.AddField("DeviceModel", SystemInfo.deviceModel);
            form.AddField("DeviceOS", SystemInfo.operatingSystem);
        }

        protected void AddCommonData(Dictionary<string,string> keyValues)
        {
            keyValues.Add("Build", ProductSettings.pInstance._BuildNumber);
            keyValues.Add("Version", ProductSettings.GetCurrentProductInfo().Version);
            keyValues.Add("Platform", Application.platform.ToString());
            keyValues.Add("NetWorkType", Application.internetReachability.ToString());
            keyValues.Add("DeviceModel", SystemInfo.deviceModel);
            keyValues.Add("DeviceOS", SystemInfo.operatingSystem);
        }
        #endregion Private/Protected Methods

        #region Public Methods

        public void ApplyConfig(LoggingConfig config)
        {
            if (config == null)
                return;

            _EnableLog = config.enable;
            _ShowLog = config.showLog;
            _ShowWarning = config.showWarning;
            _ShowError = config.showError;
            _ShowStackTrace = config.showStackTrace;
            _SkipDuplicates = config.skipDuplicates;
        }
        #endregion Public Methods

        #region Static methods
        public virtual void AddMeta(Dictionary<string, string> meta)
        {
            mMeta = meta;
        }

        public static void AddMeta(string key, string val)
        {
            if (Instance == null)
                return;
            if (Instance.mMeta == null)
                Instance.mMeta = new Dictionary<string, string>();
            if (Instance.mMeta.ContainsKey(key))
                Instance.mMeta[key] = val;
            else
                Instance.mMeta.Add(key, val);
        }

        public static void RemoveMeta(string key)
        {
            if (Instance == null)
                return;
            if (Instance.mMeta != null)
            {
                if (Instance.mMeta.ContainsKey(key))
                {
                    Instance.mMeta.Remove(key);
                }
            }
        }

        public static void Log(string data, XDebug.Mask mask = XDebug.Mask.None)
        {
#if !LIVE_BUILD && !BETA_BUILD
            //Debug.Log(data);
#else
            if (Instance == null)
                return;
            Instance.CheckLoggingConfig(data, StackTraceUtility.ExtractStackTrace(), LogType.Log);
#endif
        }
        #endregion Static methods
    }
}
