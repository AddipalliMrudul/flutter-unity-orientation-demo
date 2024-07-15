#if XG_ANALYTICS
using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Analytics
{
    [Flags]
    public enum AnalyticsAgent
    {
        None = 0,
        Unity = 1,
        Firebase = 1 << 1,
        Flurry = 1 << 2,
        ClickStream = 1 << 3,
    }

    public class Analytics : BaseBehaviour
    {
        public AnalyticsAgent _AnalyticsAgents;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private bool _EnableLogs = false;
#endif
        [InjectSignal] private SigAnalyticsLevelStart mSigLevelStart = null;
        [InjectSignal] private SigAnalyticsLevelEnd mSigLevelEnd = null;
        [InjectSignal] private SigAnalyticsWatchRewardVideo mSigAnalyticsWatchRewardVideo = null;
        [InjectSignal] private SigAnalyticsRewardVideoCompleted mSigAnalyticsRewardVideoCompleted = null;
        [InjectSignal] private SigAnalyticsCustomEvent mSigAnalyticsCustomEvent = null;

        [InjectModel] private AnalyticsModel mAnalyticsModel = null;

        private Dictionary<AnalyticsAgent, IAnalyticsAgent> mAgents = null;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            if (_EnableLogs)
                XDebug.AddMask(XDebug.Mask.Analytics);
#endif
            mAgents = new Dictionary<AnalyticsAgent, IAnalyticsAgent>();
            if (CanLog(AnalyticsAgent.Firebase))
                mAgents.Add(AnalyticsAgent.Firebase, new FirebaseAnalytics());
#if UNITY_ANALYTICS
            if (CanLog(AnalyticsAgent.Unity))
                mAgents.Add(AnalyticsAgent.Unity, new UnityAnalytics());
#endif
            if (CanLog(AnalyticsAgent.Flurry))
            {
                FlurryAnalytics flurryAnalytics = Utilities.FindObjectOfType<FlurryAnalytics>();
                if (flurryAnalytics != null)
                    mAgents.Add(AnalyticsAgent.Flurry, flurryAnalytics);
                else
                    XDebug.LogException("Flurry Analytics is enabled, but object is added in scene");
            }

            mSigLevelStart.AddListener(OnLevelStart);
            mSigLevelEnd.AddListener(OnLevelEnd);
            mSigAnalyticsWatchRewardVideo.AddListener(OnWatchRewardedVideo);
            mSigAnalyticsRewardVideoCompleted.AddListener(OnRewardedVideoCompleted);
            mSigAnalyticsCustomEvent.AddListener(OnCustomEvent);
        }

        private void OnDestroy()
        {
            mSigLevelStart.RemoveListener(OnLevelStart);
            mSigLevelEnd.RemoveListener(OnLevelEnd);
            mSigAnalyticsWatchRewardVideo.RemoveListener(OnWatchRewardedVideo);
            mSigAnalyticsRewardVideoCompleted.RemoveListener(OnRewardedVideoCompleted);
            mSigAnalyticsCustomEvent.RemoveListener(OnCustomEvent);
        }

        private void OnCustomEvent(string eventName, Dictionary<string, object> customData)
        {
            if (customData == null && mAnalyticsModel.CommonData.Count > 0)
                customData = new Dictionary<string, object>();
            customData.Append(mAnalyticsModel.CommonData);

            if (XDebug.CanLog(XDebug.Mask.Analytics))
                Debug.Log($"OnCustomEvent: {eventName}\n{customData.Printable()}");

            foreach (KeyValuePair<AnalyticsAgent, IAnalyticsAgent> keyValuePair in mAgents)
            {
                keyValuePair.Value.OnCustomEvent(eventName, customData);
            }
        }

        private void OnRewardedVideoCompleted()
        {
            foreach (KeyValuePair<AnalyticsAgent, IAnalyticsAgent> keyValuePair in mAgents)
            {
                keyValuePair.Value.OnRewardedVideoCompleted();
            }
        }

        private void OnWatchRewardedVideo()
        {
            foreach (KeyValuePair<AnalyticsAgent, IAnalyticsAgent> keyValuePair in mAgents)
            {
                keyValuePair.Value.OnWatchRewardedVideo();
            }
        }

        private void OnLevelEnd(string levelName, Dictionary<string, object> customData)
        {
            if (customData == null && mAnalyticsModel.CommonData.Count > 0)
                customData = new Dictionary<string, object>();
            customData.Append(mAnalyticsModel.CommonData);
            if (XDebug.CanLog(XDebug.Mask.Analytics))
                Debug.Log($"OnLevelEnd: {levelName}\n{customData.Printable()}");
            foreach (KeyValuePair<AnalyticsAgent, IAnalyticsAgent> keyValuePair in mAgents)
            {
                keyValuePair.Value.OnLevelEnd(levelName, customData);
            }
        }

        private void OnLevelStart(string levelName, Dictionary<string, object> customData)
        {
            if (customData == null && mAnalyticsModel.CommonData.Count > 0)
                customData = new Dictionary<string, object>();
            customData.Append(mAnalyticsModel.CommonData);
            if (XDebug.CanLog(XDebug.Mask.Analytics))
                Debug.Log($"OnLevelStart: {levelName}\n{customData.Printable()}");
            foreach (KeyValuePair<AnalyticsAgent, IAnalyticsAgent> keyValuePair in mAgents)
            {
                keyValuePair.Value.OnLevelStart(levelName, customData);
            }
        }

        private bool CanLog(AnalyticsAgent agent)
        {
            return (_AnalyticsAgents & agent) != 0;
        }
    }
}
#endif