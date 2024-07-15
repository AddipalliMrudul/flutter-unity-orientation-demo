using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.WebServices
{
    /// <summary>
    /// To maintain the WebRequest Settings.
    /// </summary>
    [CreateAssetMenu(fileName = "WebRequestSettings", menuName = Utilities.MenuName + "WebRequest Settings")]
    public class WebRequestSettings : ScriptableObject
    {
        public int _PoolSize = 5;
        public List<APIConfig> Configs = null;
        public bool _SimulationEnabled = false;

        private static WebRequestSettings mInstance = null;

        public static List<APIConfig> pConfigs
        {
            get
            {
                if (mInstance != null)
                    return mInstance.Configs;
                return null;
            }
        }

        private const string SimulateKey = "ApiSimulationDataKey-";

        public static WebRequestSettings pInstance => mInstance;

        public static bool Init()
        {
            if (mInstance == null)
            {
                mInstance = ResourceManager.LoadFromResources<WebRequestSettings>("WebRequestSettings");
                Load();
            }
            return mInstance != null;
        }

        public static bool Load()
        {
#if DEV_BUILD || QA_BUILD
            if (mInstance != null && PlayerPrefs.HasKey(SimulateKey))
            {
                try
                {
                    mInstance.Configs = PlayerPrefs.GetString(SimulateKey).FromJson<List<APIConfig>>();
                }
                catch (Exception e)
                {
                    XDebug.LogException($"Failed to load WebRequest simulation data: {e.Message}");
                }
                return true;
            }
#endif
            return false;
        }

        public static void Save()
        {
#if DEV_BUILD || QA_BUILD
            if (mInstance == null)
                Init();
            PlayerPrefs.SetString(SimulateKey, pInstance.Configs.ToJson());
            PlayerPrefs.Save();
#endif
        }

        public static void Clear()
        {
            if(mInstance == null)
                Init();
            pInstance.Configs.ForEach(e =>
            {
                e.simulation.failProbability = 0;
                e.simulation.delay = 0;
            });
            PlayerPrefs.DeleteKey(SimulateKey);
            PlayerPrefs.Save();
        }

        public static void Randomize()
        {
            if (mInstance == null)
                Init();
            pInstance.Configs.ForEach(e =>
            {
                e.simulation.failProbability = UnityEngine.Random.Range(0, 101);
                e.simulation.delay = UnityEngine.Random.Range(0f, 5f);
                e.simulation.APIErrorType = UnityEngine.Random.Range(0, 2) < 1 ? APIErrorType.BeforeHittingServer : APIErrorType.AfterHittingServer;
            });
        }

        public static APIConfig GetConfigData(string endpoint)
        {
            if (pConfigs == null)
                return null;

            return pConfigs.Find(e => endpoint.Contains(e.endpoint));
        }
    }
}
