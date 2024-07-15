#if USE_UNITY_REMOTE_CONFIG
using UnityEngine;
using XcelerateGames;
using XcelerateGames.IOC;
using Unity.RemoteConfig;

namespace XcelerateGames
{
    public class RemoteConfigManager : BaseBehaviour
    {
#region Member variables
        public EnvironmentVars _EnvironmentIds = null;
        public float _WaitTime = 5f;
#endregion

#region Signals & Models
        [InjectSignal] private SigRemoteConfigLoaded mSigRemoteConfigLoaded = null;
#endregion
        public struct userAttributes
        {
            // Optionally declare variables for any custom user attributes:
            public bool expansionFlag;
        }

        public struct appAttributes
        {
            // Optionally declare variables for any custom app attributes:
            public int level;
            public int score;
            public string version;
            public string platform;
            public string buildNumber;
        }

#region Private Methods
        // Retrieve and apply the current key-value pairs from the service on Awake:
        protected override void Awake()
        {
            base.Awake();

            ConfigManager.SetEnvironmentID(_EnvironmentIds.Value);

            // Add a listener to apply settings when successfully retrieved:
            ConfigManager.FetchCompleted += ApplyRemoteSettings;

            // Set the user’s unique ID:
            ConfigManager.SetCustomUserID("some-user-id");

            // Set the environment ID:
            ConfigManager.SetEnvironmentID(_EnvironmentIds.Value);
            appAttributes appAttr = new appAttributes()
            {
                platform = PlatformUtilities.GetCurrentPlatform().ToString(),
                buildNumber = ProductSettings.pInstance._BuildNumber,
                version = ProductSettings.GetCurrentProductInfo().Version
            };

            // Fetch configuration setting from the remote service:
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), appAttr);
        }

        void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            Debug.Log($"ApplyRemoteSettings: {configResponse.ToJson()}");
            // Conditionally update settings, depending on the response's origin:
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    Debug.Log("No settings loaded this session; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    Debug.Log("No settings loaded this session; using cached values from a previous session.");
                    break;
                case ConfigOrigin.Remote:
                    Debug.Log("New settings loaded this session; update values accordingly.");
                    break;
            }
            mSigRemoteConfigLoaded.Dispatch(configResponse.status == ConfigRequestStatus.Success);
        }

        private void Update()
        {
            _WaitTime -= Time.deltaTime;
            if (_WaitTime <= 0)
            {
                mSigRemoteConfigLoaded.Dispatch(false);
                enabled = false;
            }
        }

#endregion Private Methods

#region static Methods
        public static string GetString(string key, string defaultValue = null)
        {
            return ConfigManager.appConfig.GetString(key, defaultValue);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return ConfigManager.appConfig.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return ConfigManager.appConfig.GetFloat(key, defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return ConfigManager.appConfig.GetBool(key, defaultValue);
        }

        public static long GetLong(string key, long defaultValue = 0)
        {
            return ConfigManager.appConfig.GetLong(key, defaultValue);
        }

        //public static string GetJson(string key, string defaultValue = "{}")
        //{
        //    return ConfigManager.appConfig.(key, defaultValue);
        //}
#endregion static Methods
    }
}
#endif //USE_UNITY_REMOTE_CONFIG