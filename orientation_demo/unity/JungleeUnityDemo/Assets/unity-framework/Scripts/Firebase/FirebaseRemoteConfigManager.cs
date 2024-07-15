#if USE_FIREBASE
using System.Linq;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class FirebaseRemoteConfigManager : BaseBehaviour
    {
        #region Signals
        [InjectSignal] protected SigFirebaseRemoteConfigUpdated mSigFirebaseRemoteConfigUpdated = null;
        [InjectModel] protected RemoteConfigModel mRemoteConfigModel = null;
        #endregion //Signals

        #region Private Methods
        protected override void Awake()
        {
            base.Awake();
            if (FirebaseRemoteConfig.DefaultInstance != null)
                FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += ConfigUpdateListenerEventHandler;
        }

        protected override void OnDestroy()
        {
            if (FirebaseRemoteConfig.DefaultInstance != null)
                FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener -= ConfigUpdateListenerEventHandler;
            base.OnDestroy();
        }

        private void ConfigUpdateListenerEventHandler(object sender, ConfigUpdateEventArgs args)
        {
            if (args.Error != RemoteConfigError.None)
            {
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.LogError($"Error occurred while listening: { args.Error}", XDebug.Mask.Game);
                return;
            }

            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Remote Config Updated keys: { string.Join(", ", args.UpdatedKeys)}", XDebug.Mask.Game);
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            // Activate all fetched values and then display a welcome message.
            remoteConfig.ActivateAsync().ContinueWithOnMainThread(
              task =>
              {
                  if (!task.IsCompleted)
                  {
                      Debug.LogError("Retrieval hasn't finished.");
                      return;
                  }
                  if (XDebug.CanLog(XDebug.Mask.Game))
                  {
                      XDebug.Log($"New Remote data loaded and ready for use. Last fetch time {remoteConfig.Info.FetchTime}. Remote Configs: \n" + remoteConfig.AllValues.Printable(PrintConverter), XDebug.Mask.Game);
                  }
                  mRemoteConfigModel.SetConfig(remoteConfig);
                  mSigFirebaseRemoteConfigUpdated.Dispatch(args.UpdatedKeys.ToList());
              });
        }

        private string PrintConverter(string key, ConfigValue val)
        {
            return $"{key}->{val.StringValue}";
        }
        #endregion //Private Methods
    }
}
#endif //USE_FIREBASE