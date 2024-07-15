#if XG_ANALYTICS
//#define USE_FLURRY
using System.Collections.Generic;
using UnityEngine;
#if USE_FLURRY
using FlurrySDK;
#endif

/*How To: When we import the plugin, FlurrySDK folder is created under Assets/Plugin folder. With this we get compiler errors.
 * move FlurrySDK folder out of Assets/Plugin to under Assets.
 * Create a Assemble Definition file & add it as dependency to Framework_ADF file.
 */

namespace XcelerateGames.Analytics
{
    public class FlurryAnalytics : BaseBehaviour, IAnalyticsAgent
    {
#if USE_FLURRY
        [SerializeField] private PlatformVars _Keys = null;
#endif

        public void Start()
        {
#if USE_FLURRY
            // Note: When enabling Messaging, Flurry Android should be initialized by using AndroidManifest.xml.
            // Initialize Flurry once.
            new Flurry.Builder()
                      .WithCrashReporting(true)
                      .WithLogEnabled(true)
                      .WithLogLevel(Flurry.LogLevel.ERROR)
                      .WithMessaging(true)
                      .Build(_Keys.Value);

            // Example to get Flurry versions.
            Debug.Log("AgentVersion: " + Flurry.GetAgentVersion());
            Debug.Log("ReleaseVersion: " + Flurry.GetReleaseVersion());

            // Set user preferences.
            //Flurry.SetAge(36);
            //Flurry.SetGender(Flurry.Gender.Female);
            Flurry.SetReportLocation(true);

            // Set user properties.
            Flurry.UserProperties.Set(Flurry.UserProperties.PROPERTY_REGISTERED_USER, "True");

            // Set Messaging listener
            //Flurry.SetMessagingListener(new MyMessagingListener());
#endif
        }

        public void OnCustomEvent(string eventName, Dictionary<string, object> customData)
        {
#if USE_FLURRY
            Flurry.EventRecordStatus status = Flurry.LogEvent(eventName, GetFlurryParameters(customData));
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"FlurryAnalytics::OnCustomEvent, Result : {status}");
#endif
        }

        public void OnRewardedVideoCompleted()
        {
#if USE_FLURRY
            Flurry.EventRecordStatus status = Flurry.LogEvent("RewardedVideoComplete");
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"FlurryAnalytics::OnRewardedVideoCompleted, Result : {status}");
#endif
        }

        public void OnWatchRewardedVideo()
        {
#if USE_FLURRY

            Flurry.EventRecordStatus status = Flurry.LogEvent("WatchRewardedVideo");
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"FlurryAnalytics::OnWatchRewardedVideo, Result : {status}");
#endif
        }

        public void OnLevelEnd(string levelName, Dictionary<string, object> customParams)
        {
#if USE_FLURRY
            Flurry.EventRecordStatus status = Flurry.LogEvent(levelName, GetFlurryParameters(customParams));
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"FlurryAnalytics::OnLevelEnd {levelName}, Result : {status}");
#endif
        }

        public void OnLevelStart(string levelName, Dictionary<string, object> customParams)
        {
#if USE_FLURRY
            Flurry.EventRecordStatus status = Flurry.LogEvent(levelName, GetFlurryParameters(customParams));
            if (XDebug.CanLog(XDebug.Mask.Analytics)) Debug.Log($"FlurryAnalytics::OnLevelStart {levelName}, Result : {status}");
#endif
        }

#if USE_FLURRY
        //Converts a dictionary of custom data to array of Parameter to be used in Firebase
        private Dictionary<string, string> GetFlurryParameters(Dictionary<string, object> customData)
        {
            if (customData == null)
                return null;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (string key in customData.Keys)
            {
                parameters.Add(key, customData[key].ToString());
            }

            return parameters;
        }
#endif
    }
}
#endif