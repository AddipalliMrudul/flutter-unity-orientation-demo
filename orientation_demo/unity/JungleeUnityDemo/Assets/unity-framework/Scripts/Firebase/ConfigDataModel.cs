#if USE_FIREBASE
using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public abstract class ConfigDataModel : XGModel
    {
        protected string mRemoteConfigKey = null;

        [InjectSignal] private SigFirebaseRemoteConfigUpdated mSigFirebaseRemoteConfigUpdated;
        [InjectModel] protected RemoteConfigModel mRemoteConfigModel = null;

        public ConfigDataModel()
        {
            mSigFirebaseRemoteConfigUpdated.AddListener(OnFirebaseRemoteConfigUpdated);
        }

        ~ConfigDataModel()
        {
            mSigFirebaseRemoteConfigUpdated.RemoveListener(OnFirebaseRemoteConfigUpdated);
        }

        protected abstract void OnFirebaseRemoteConfigUpdated(List<string> updatedKeys);

        public abstract void ParseRemoteJsonData();
    }
}
#endif