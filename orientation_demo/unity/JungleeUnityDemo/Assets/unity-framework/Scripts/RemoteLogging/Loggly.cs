using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XcelerateGames.RemoteLogging
{
    public class Loggly : RemoteLoggingBase
    {
        [SerializeField] string _URL = "http://logs-01.loggly.com/inputs/{PutTokenHere}/tag/{AppNameHere}";

        private Dictionary<LogType, string> mLogLevels = new Dictionary<LogType, string>();
        private bool mSuccess = false;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            _URL += "-" + PlatformUtilities.GetEnvironment();
            Debug.Log("Loggly URL : " + _URL);
        }

        protected override void PostLog(string logString, string stackTrace, LogType logType)
        {
            base.PostLog(logString, stackTrace, logType);
            StartCoroutine(PostToLoggly(logString, stackTrace, logType, DateTime.Now.ToString("dd:MM:yy HH:mm:ss.fff"), false));
        }

        private IEnumerator PostToLoggly(string logString, string stackTrace, LogType inType, string timeStamp, bool fromCache)
        {
            if (!mLogLevels.ContainsKey(inType))
                mLogLevels.Add(inType, inType.ToString());

            WWWForm form = new WWWForm();

            //Add log message to WWWForm
            AddCommonData(form);
            form.AddField("Type", mLogLevels[inType]);
            form.AddField("Message", logString);
            form.AddField("Time", timeStamp);
            //Temp: We had max field issue, Time was not showingup
            form.AddField("ProcessorCount", timeStamp);
            if (_ShowStackTrace)
                form.AddField("StackTrace", stackTrace);
            if (mMeta != null)
            {
                foreach (KeyValuePair<string, string> meta in mMeta)
                {
                    form.AddField(meta.Key, meta.Value);
                }
            }

            using (UnityWebRequest www = UnityWebRequest.Post(_URL, form))
            {
                UnityWebRequestAsyncOperation asyncOperation = www.SendWebRequest();
                bool offline = false;

                while (!asyncOperation.isDone)
                {
                    if (!ConnectivityMonitor.pIsInternetAvailable)
                    {
                        offline = true;
                        break;
                    }
                    yield return www;
                }

                bool success = !offline && www.result == UnityWebRequest.Result.Success;
                PostNextLog(success);
            }
        }

        private void PostNextLog(bool success)
        {
            if (mLogData.Count > 0)
            {
                mSuccess = success;
                if (ConnectivityMonitor.pIsInternetAvailable)
                {
                    LogData logData = mSuccess ? mLogData.Dequeue() : mLogData.Peek();
                    //Debug.LogWarning($"Posting {logData.logString}, Count: {mLogData.Count}");
                    StartCoroutine(PostToLoggly(logData.logString, logData.stackTrace, logData.logType, logData.time, false));
                }
                else
                    ConnectivityMonitor.AddListener(OnNetworkStatusChanged);
            }
            //else
            //    Debug.LogWarning("Done with posting all logs");
        }

        private void OnNetworkStatusChanged(ConnectivityMonitor.Status status)
        {
            if(status == ConnectivityMonitor.Status.Online)
            {
                ConnectivityMonitor.RemoveListener(OnNetworkStatusChanged);
                PostNextLog(false);
            }
        }
    }
}
