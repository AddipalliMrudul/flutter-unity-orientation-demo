#if UNITY_REMOTE_SETTINGS

using Unity.RemoteConfig;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Class to handle Unity Remote Config.
    /// https://docs.unity3d.com/Packages/com.unity.remote-config@2.1/manual/index.html
    /// </summary>
    public class UnityRemoteConfig : MonoBehaviour
    {
        #region Data structures
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
            public string appVersion;
        }
        #endregion Data structures

        #region Member variables & properties
        private static UnityRemoteConfig mInstance = null;
        #endregion Member variables & properties

        #region Private Methods

        /// <summary>
        /// Retrieve and apply the current key-value pairs from the service on Awake:
        /// </summary>
        protected virtual void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                DontDestroyOnLoad(gameObject);

                // Add a listener to apply settings when successfully retrieved:
                ConfigManager.FetchCompleted += ApplyRemoteSettings;

                Fetch();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Callback after fetching remote settings 
        /// </summary>
        /// <param name="configResponse"></param>
        protected virtual void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            // Conditionally update settings, depending on the response's origin:
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    XDebug.Log("No settings loaded this session; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    XDebug.Log("No settings loaded this session; using cached values from a previous session.");
                    break;
                case ConfigOrigin.Remote:
                    XDebug.Log("New settings loaded this session; update values accordingly.");
                    break;
            }
        }

        /// <summary>
        /// Fetch the data for the current environment. To set environment call SetEnvironmentId(string)
        /// </summary>
        protected virtual void Fetch()
        {
            // Fetch configuration setting from the remote service:
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }

        #endregion Private Methods

        #region Static Methods
        /// <summary>
        /// Set the user’s unique ID:
        /// </summary>
        /// <param name="userId">Users unique user id</param>
        public static void SetUserId(string userId)
        {
            ConfigManager.SetCustomUserID(userId);
        }

        /// <summary>
        /// Set the environment ID.
        /// @note Environment id looks like this 3e015f22-b354-4b0e-ab80-b68d1f3a57f8. Environment id is different from environment name
        /// </summary>
        /// <param name="envId">environment id</param>
        public static void SetEnvironmentId(string envId)
        {
            ConfigManager.SetEnvironmentID(envId);
            mInstance.Fetch();
        }

        /// <summary>
        /// Get the string value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static string GetString(string key, string defaultValue = "")
        {
            return ConfigManager.appConfig.GetString(key, defaultValue);
        }

        /// <summary>
        /// Get the int value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            return ConfigManager.appConfig.GetInt(key, defaultValue);
        }

        /// <summary>
        /// Get the float value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static float GetFloat(string key, float defaultValue = 0)
        {
            return ConfigManager.appConfig.GetFloat(key, defaultValue);
        }

        /// <summary>
        /// Get the bool value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return ConfigManager.appConfig.GetBool(key, defaultValue);
        }

        /// <summary>
        /// Get the long value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static long GetLong(string key, long defaultValue = 0)
        {
            return ConfigManager.appConfig.GetLong(key, defaultValue);
        }

        /// <summary>
        /// Get the json value represented by the key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if key not found</param>
        /// <returns>value if the key is present else dafault value</returns>
        public static string GetBool(string key, string defaultValue = "")
        {
            return ConfigManager.appConfig.GetJson(key, defaultValue);
        }

        /// <summary>
        /// Get all available keys
        /// </summary>
        /// <returns>Array of all keys</returns>
        public static string[] GetKeys()
        {
            return ConfigManager.appConfig.GetKeys();
        }

        /// <summary>
        /// Checks if the given key exists
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>true if found else false</returns>
        public static bool HsKey(string key)
        {
            return ConfigManager.appConfig.HasKey(key);
        }
        #endregion Static Methods
    }
}
#endif //UNITY_REMOTE_SETTINGS
