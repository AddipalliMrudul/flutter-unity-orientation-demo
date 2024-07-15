using System.Collections.Generic;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public abstract class CmdSendBreadcrumbEvent : Command
    {
        [InjectParameter] protected string mEventName;
        [InjectParameter] protected Dictionary<string, object> mParams = null;

        protected void AddCommonParams()
        {
            if (mParams == null)
                mParams = new Dictionary<string, object>();
            mParams.Add("appVersion", ProductSettings.GetProductVersion());
            mParams.Add("deviceId", Utilities.GetUniqueID());
            mParams.Add("timeStamp", TimeUtilities.GetEpochTime());
            mParams.Add("activeSceneName", ResourceManager.pCurrentScene);
        }
    }
}