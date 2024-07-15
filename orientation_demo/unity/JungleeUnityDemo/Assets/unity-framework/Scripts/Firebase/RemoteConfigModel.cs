#if USE_FIREBASE
using System.Collections.Generic;
using Firebase.RemoteConfig;
using XcelerateGames.IOC;
using System.Linq;

namespace XcelerateGames
{
    public class RemoteConfigModel : XGModel
    {
        private FirebaseRemoteConfig Config = null;
        private List<string> mKeys = null;

        public void SetConfig(FirebaseRemoteConfig config)
        {
            Config = config;
            mKeys = Config.Keys.ToList();
        }

        public string GetString(string key)
        {
            return Config.GetValue(key).StringValue;
        }

        public string GetString(string key, string defaultValue)
        {
            if (mKeys.Contains(key))
                return Config.GetValue(key).StringValue;
            return defaultValue;
        }

        public bool GetBool(string key)
        {
            return Config.GetValue(key).BooleanValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (mKeys.Contains(key))
                return Config.GetValue(key).BooleanValue;
            return defaultValue;
        }

        public long GetLong(string key)
        {
            return Config.GetValue(key).LongValue;
        }

        public long GetLong(string key, long defaultValue)
        {
            if (mKeys.Contains(key))
                return Config.GetValue(key).LongValue;
            return defaultValue;
        }

        public double GetDouble(string key)
        {
            return Config.GetValue(key).DoubleValue;
        }

        public double GetDouble(string key, double defaultValue)
        {
            if (mKeys.Contains(key))
                return Config.GetValue(key).DoubleValue;
            return defaultValue;
        }
    }
}
#endif
