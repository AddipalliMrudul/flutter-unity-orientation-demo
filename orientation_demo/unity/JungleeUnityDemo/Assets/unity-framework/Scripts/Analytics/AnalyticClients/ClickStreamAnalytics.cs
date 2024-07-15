#if CLICKSTREAM_ANALYTICS
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace XcelerateGames.Analytics
{
    public class ClickStreamAnalytics : IAnalyticsAgent
    {
        public ClickStreamAnalytics()
        {
        }

        public void OnCustomEvent(string eventName, Dictionary<string, object> customData)
        {
        }

        public void OnRewardedVideoCompleted()
        {
        }

        public void OnWatchRewardedVideo()
        {
        }

        public void OnLevelEnd(string levelName, Dictionary<string, object> customParams)
        {
        }

        public void OnLevelStart(string levelName, Dictionary<string, object> customParams)
        {
        }
    }
}
#endif
