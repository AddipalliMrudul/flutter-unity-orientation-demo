using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Audio
{
    [CreateAssetMenu(fileName = "AudioControllerSettings", menuName = Utilities.MenuName + "AudioControllerSettings")]
    public class AudioControllerSettings : ScriptableObject
    {
        public string _SoundAssetBundleName = "sounds";

        [Header("Pool Related")]
        public string _PoolName = "AudioPool";
        public int _PoolSize = 50;

        private static AudioControllerSettings mInstance = null;

        public static void Init()
        {
            if (mInstance == null)
            {
                mInstance = ResourceManager.LoadFromResources<AudioControllerSettings>("AudioControllerSettings");
            }
            if (mInstance == null)
                XDebug.LogException($"Could not find AudioControllerSettings asset under Resources folder");
        }

        public static AudioControllerSettings pInstance
        {
            get
            {
                if (mInstance == null)
                    Init();
                return mInstance;
            }
        }
    }
}
