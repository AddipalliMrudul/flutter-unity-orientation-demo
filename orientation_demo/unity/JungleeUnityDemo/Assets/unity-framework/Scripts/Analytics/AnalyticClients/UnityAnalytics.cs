using System.Collections.Generic;
#if UNITY_ANALYTICS
using UnityEngine;
using UnityEngine.Analytics;
#endif

namespace XcelerateGames.Analytics
{
    public class UnityAnalytics : IAnalyticsAgent
    {
        public UnityAnalytics()
        {
        }

        public void OnCustomEvent(string eventName, Dictionary<string, object> customData)
        {
#if UNITY_ANALYTICS
            AnalyticsResult result = AnalyticsEvent.Custom(eventName, customData);
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"UnityAnalytics::OnCustomEvent, Result : {result}");
#endif
        }

        public void OnRewardedVideoCompleted()
        {
#if UNITY_ANALYTICS
            AnalyticsResult result = AnalyticsEvent.Custom("RewardedVideoComplete");
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"UnityAnalytics::OnRewardedVideoCompleted, Result : {result}");
#endif
        }

        public void OnWatchRewardedVideo()
        {
#if UNITY_ANALYTICS
            AnalyticsResult result = AnalyticsEvent.Custom("WatchRewardedVideo");
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"UnityAnalytics::OnWatchRewardedVideo, Result : {result}");
#endif
        }

        public void OnLevelEnd(string levelName, Dictionary<string, object> customParams)
        {
#if UNITY_ANALYTICS
            AnalyticsResult result = AnalyticsEvent.LevelComplete(levelName, customParams);
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"UnityAnalytics::OnLevelEnd {levelName}, Result : {result}");
#endif
        }

        public void OnLevelStart(string levelName, Dictionary<string, object> customParams)
        {
#if UNITY_ANALYTICS
            AnalyticsResult result = AnalyticsEvent.LevelStart(levelName, customParams);
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"UnityAnalytics::OnLevelStart {levelName}, Result : {result}");
#endif
        }
    }
}
