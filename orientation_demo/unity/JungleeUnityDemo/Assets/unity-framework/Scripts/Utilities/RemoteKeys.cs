namespace XcelerateGames
{
    public static class RemoteKeys
    {
        #region All base keys go here
        private const string mApiBaseURL = "ApiBaseURL-";
        //Time in seconds
        private const string mReloadWaitTime = "ReloadWaitTime-";
        private const string mVideoAdReward = "VideoAdReward-";
        private const string mShowVideoAdReward = "ShowVideoAdReward-";
        #endregion All base keys go here

        #region All Environemnet specific keys go here
#if DEV_BUILD
        public const string ApiBaseURL = mApiBaseURL + "Dev";
        public const string ReloadWaitTime = mReloadWaitTime + "Dev";
        public const string VideoAdReward = mVideoAdReward + "Dev";

#elif QA_BUILD
        public const string ApiBaseURL = mApiBaseURL + "QA";
        public const string ReloadWaitTime = mReloadWaitTime + "QA";
        public const string VideoAdReward = mVideoAdReward + "QA";
#else
        public const string ApiBaseURL = mApiBaseURL + "Live";
        public const string ReloadWaitTime = mReloadWaitTime + "Live";
        public const string VideoAdReward = mVideoAdReward + "Live";
#endif
        #endregion All Environemnet specific keys go here

        #region All common keys go here
        public const string NetConnectCheckFrequency = "NetConnectCheckFrequency";
        public const string MinMemory = "MinMemory";
        public const string VersionListTimeOut = "VersionListTimeOut";
        public const string ManifestTimeOut = "ManifestTimeOut";
        public const string NotificationsData = "notifications_data";

        public const string RateUsURL = "app_rating_url";
        public const string RateUsKey = "RateUsPrompt";
        public const string IAPItems = "IAPItems";
        public const string ConsoleEnablePattern = "ConsoleEnablePattern";
        public const string RateUsWaitTimeKey = "RateUsWaitTime";

        #endregion All common keys go here
    }
}
