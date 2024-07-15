using System.Collections.Generic;

namespace XcelerateGames.Analytics
{
    public interface IAnalyticsAgent
    {
        void OnCustomEvent(string eventName, Dictionary<string, object> customData);
        void OnRewardedVideoCompleted();
        void OnWatchRewardedVideo();
        void OnLevelEnd(string levelName, Dictionary<string, object> customParams);
        void OnLevelStart(string levelName, Dictionary<string, object> customParams);
    }
}
