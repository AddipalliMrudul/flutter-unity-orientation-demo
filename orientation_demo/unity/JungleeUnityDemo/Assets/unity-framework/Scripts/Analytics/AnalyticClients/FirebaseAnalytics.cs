#if XG_ANALYTICS
//#define USE_FIREBASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Analytics
{
    public class FirebaseAnalytics : IAnalyticsAgent
    {
        public FirebaseAnalytics()
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#endif //USE_FIREBASE
        }

        public void OnCustomEvent(string eventName, Dictionary<string, object> customData)
        {
#if USE_FIREBASE
            if (customData == null)
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
            else
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, GetFirebaseParameters(customData));
#endif //USE_FIREBASE
        }

        public void OnRewardedVideoCompleted()
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent("RewardedVideoComplete");
#endif //USE_FIREBASE
        }

        public void OnWatchRewardedVideo()
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent("WatchRewardedVideo");
#endif //USE_FIREBASE
        }

        public void OnLevelEnd(string levelName, Dictionary<string, object> customParams)
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd, Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, levelName);
#endif //USE_FIREBASE
        }

        public void OnLevelStart(string levelName, Dictionary<string, object> customParams)
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelStart, Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, levelName);
#endif //USE_FIREBASE
        }

#if USE_FIREBASE
        //Converts a dictionary of custom data to array of Parameter to be used in Firebase
        private Firebase.Analytics.Parameter[] GetFirebaseParameters(Dictionary<string, object> customData)
        {
            if (customData == null)
                return null;
            Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[customData.Count];
            int i = 0;
            foreach (string key in customData.Keys)
            {
                parameters[i++] = new Firebase.Analytics.Parameter(key, customData[key].ToString());
            }

            return parameters;
        }
#endif //USE_FIREBASE
    }
}
#endif