using System.Collections.Generic;
using XcelerateGames.Timer;

namespace XcelerateGames.WebServices
{
    public enum WebServiceEvent
    {
        NONE,
        PROGRESS,
        COMPLETE,
        ERROR,
        OFFLINE
    }

    public static class WebService
    {
        #region Member Variables
        #endregion Member Variables

        #region Getters & Setters

        public static double pTicks { get { return ServerTime.pCurrentTime; } }
        public static string pSecret { get { return ProductSettings.pInstance._SecretKey; } }

        #endregion Getters & Setters

        #region Private/Protected Methods

        /// <summary>
        /// Initialize WebService class
        /// </summary>
        static WebService()
        {
            WebRequestProcessor.Init();
        }

#endregion Private/Protected Methods

#region Public Methods

        public static Dictionary<string,string> AddCommon()
        {
            Dictionary<string, string> common = new Dictionary<string, string>();
            common.Add(Constants.UUID, Utilities.GetUniqueID());
            //common.Add("platform", PlatformUtilities.GetCurrentPlatform().ToString());
            //common.Add("AppVersion", ProductSettings.GetProductVersion());
            return common;
        }

#endregion Public Methods
    }
}
