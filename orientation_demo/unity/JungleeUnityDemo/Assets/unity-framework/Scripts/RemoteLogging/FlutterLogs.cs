using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace XcelerateGames.RemoteLogging
{
    public class FlutterLogs : RemoteLoggingBase
    {
        #region Properties
        [SerializeField] private string _MessageType = "GameTableLogs";
        private Dictionary<string, string> mFlutterLogData = new Dictionary<string, string>();
        #endregion Properties

        #region Signals
        [InjectSignal] private SigSendLogToFlutter mSigSendLogToFlutter = null;
        #endregion Signals

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void AddToQueue(string logString, string stackTrace, LogType logType)
        {
            PostLog(logString, stackTrace, logType);
        }

        protected override void PostLog(string logString, string stackTrace, LogType logType)
        {
            base.PostLog(logString, stackTrace, logType);
            mFlutterLogData.Clear();
            string timeStamp = DateTime.Now.ToString("dd:MM:yy HH:mm:ss.fff");
            AddCommonData(mFlutterLogData);
            mFlutterLogData.Add("Type", logType.ToString());
            mFlutterLogData.Add("Message", logString);
            mFlutterLogData.Add("Time", timeStamp);
            mFlutterLogData.Add("ProcessorCount", timeStamp);
            if (_ShowStackTrace)
                mFlutterLogData.Add("StackTrace", stackTrace);
            if (mMeta != null)
            {
                foreach (KeyValuePair<string, string> meta in mMeta)
                {
                    mFlutterLogData.Add(meta.Key, meta.Value);
                }
            }
            mSigSendLogToFlutter.Dispatch(new FlutterMessage() { type = _MessageType, data = mFlutterLogData.ToJson() });
        }
    }
}