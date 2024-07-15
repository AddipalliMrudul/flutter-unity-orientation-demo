using System;
using JungleeGames;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Helper class to handle platform specific requirements
    /// </summary>
    public class PlatformUtilities
    {
        private static string mBasePath = null;             /**<Base path of all assets to be loaded */
        private static string mPersistentDataPath = null;   /**<Path for persistant storage */
        private static string mDataPath = null;             /**<Data path of all assets to be loaded base on runtime platform*/

        ///<summary>
        ///List of Device Types
        /// </summary>
        public enum DeviceType : byte
        {
            Mobile,
            Desktop,
            WebBrowser,
            Others
        }

        /// <summary>
        /// List Platforms
        /// </summary>
        public enum Platform
        {
            NONE,           /**<No platform */
            iOS,            /**<iOS platform */
            Android,        /**<Android platform */
            Windows,        /**<Windows phone platform */
            StandAloneOSX,  /**<MAC OSX platform */
            StandAloneWin,  /**<Windows 7, 8, 10, 11 etc platforms */
            Amazon,         /**<Amazon(Kindle) Variant of Android platform */
            Samsung,        /**<Samsung Variant of Android platform */
            Tizen,          /**<Tizen platform */
            FBArcade,       /**<FBArcade. Standalone variant platform */
            WebGL           /**<WebGL platform */
        }

        /// <summary>
        /// List of all envirnoments
        /// @see EnvironmentMask
        /// </summary>
        public enum Environment
        {
            None = -1,  /**<None */    //Do not edit this line. If want to add more environment, add in between dev to End
            dev,        /**<Development environemt */ //Do not edit this line. If want to add more environment, add in between dev to End
            qa,         /**<QA environemt */
            stage,      /**<Staging environemt */
            live,       /**<Live/Production environemt */
            beta,       /**<Beta environemt */
            demo,       /**<Demo environemt */
            End         /**<End, to be sued to help in iteration */ //Do not edit this line. If want to add more environment, add in between dev to End
        }

        /// <summary>
        /// Environment mask. Can be used to control behaviour for multiple environments
        /// @see Environment
        /// </summary>
        [Flags]
        public enum EnvironmentMask
        {
            None = 0,       /**<No environemt */
            Dev = 1,        /**<Dev environemt */
            QA = 1 << 1,    /**<QA environemt */
            Stage = 1 << 2, /**<Staging environemt */
            Live = 1 << 3,  /**<Live/Production environemt */
        }

        /// <summary>
        /// Check if environment flag is set
        /// </summary>
        /// <param name="environmentMask"></param>
        /// <returns>true if available, else false</returns>
        public static bool HasEnvironment(EnvironmentMask environmentMask)
        {
            bool result = false;
            switch (GetEnvironment())
            {
                case Environment.dev:
                    if ((environmentMask & EnvironmentMask.Dev) != 0)
                        result = true;
                    break;
                case Environment.qa:
                    if ((environmentMask & EnvironmentMask.QA) != 0)
                        result = true;
                    break;
                case Environment.stage:
                    if ((environmentMask & EnvironmentMask.Stage) != 0)
                        result = true;
                    break;
                case Environment.live:
                    if ((environmentMask & EnvironmentMask.Live) != 0)
                        result = true;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Returns the current selected platform.
        /// </summary>
        /// <returns>current platform</returns>
        public static PlatformUtilities.Platform GetCurrentPlatform()
        {
#if UNITY_IOS
			return PlatformUtilities.Platform.iOS;
#elif UNITY_ANDROID
            return PlatformUtilities.Platform.Android;
#elif UNITY_WINDOWS
			return PlatformUtilities.Platform.Windows;
#elif UNITY_WEBGL
			return PlatformUtilities.Platform.WebGL;
#elif UNITY_STANDALONE_WIN
            return PlatformUtilities.Platform.StandAloneWin;
#elif UNITY_STANDALONE_OSX
            return PlatformUtilities.Platform.StandAloneOSX;
#else
            return PlatformUtilities.Platform.NONE;
#endif
        }

        /// <summary>
        /// Returns the path of the folder from which we load platform specific assets for current platform.
        /// </summary>
        /// <returns>Platform specific folder name</returns>
        public static string GetAssetFolderPath()
        {
            return GetAssetFolderByPlatform(GetCurrentPlatform());
        }


        /// <summary>
        /// Returns the path of the folder from which we load platform specific assets by platform. This path is used only in IDE
        /// </summary>
        /// <returns>Platform specific folder name</returns>
        public static string GetAssetFolderByPlatform(Platform platform)
        {
            if (platform == Platform.Android)
                return "Data_Android";
            else if (platform == Platform.iOS)
                return "Data_iOS";
            else if (platform == Platform.Windows)
                return "Data_Metro";
            else if (platform == Platform.WebGL)
                return "Data_WebGL";
            else if (platform == Platform.StandAloneOSX)
                return "Data_OSX";
            else if (platform == Platform.StandAloneWin)
                return "Data_Win";
            else
                Debug.LogError($"Add support for platform: {platform}");

            return null;
        }

        /// <summary>
        /// Returns device type. Ex Android Phone, Android tab, iPhone, iPad
        /// @todo Add this functionality
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceTypeString()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IOS
            return "ios";
#elif UNITY_STANDALONE_OSX
            return "standalone-osx";
#elif UNITY_STANDALONE_WIN
            return "standalone-win";
#elif UNITY_WEBGL
            return "webgl";
#else
            //Handle this platform
#endif
        }

        /// <summary>
        /// Gets the asset folder path for @param platform.
        /// </summary>
        /// <returns>The asset folder path for platform.</returns>
        public static string GetAssetDirectoryForPlatform(Platform inPlatform)
        {
            if (inPlatform.Equals(Platform.Android) || inPlatform.Equals(Platform.Amazon))
                return "Assets/Data_Android/";
            else if (inPlatform.Equals(Platform.iOS))
                return "Assets/Data_iOS/";
            else if (inPlatform.Equals(Platform.WebGL))
                return "Assets/Data_WebGL/";
            else if (inPlatform.Equals(Platform.FBArcade))
                return "Assets/Data_FBA/";
            else
                return "Assets/Data/";
        }

        /// <summary>
        /// Returns true if we are playing on a mobile platform, else returns false.
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns true if we are playing on IDE, else returns false.
        /// </summary>
        /// <returns>true if we are playing on IDE, else returns false</returns>
        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Return true if the build is local, else returns false.
        /// </summary>
        /// <returns></returns>
        public static bool IsLocalBuild()
        {
#if OTA_BUILD
            return false;
#else
            return true;
#endif
        }

        /// <summary>
        /// Return true if UNITY_FACEBOOK is enabled, else returns false.
        /// </summary>
        /// <returns></returns>
        public static bool IsUnityFacebook()
        {
#if UNITY_FACEBOOK
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Return true if FB login simulation is on, else returns false. Only for debugging purpose & works on IDE only
        /// </summary>
        /// <returns></returns>
        public static bool IsFBSimulation()
        {
            if (ProductSettings.pInstance == null)
                return false;
            else
                return ProductSettings.pInstance._SimulateFBLogin && Application.isEditor;
        }

        /// <summary>
        /// Return platform specific path based on runtime platform. It computes the path only once & caches it.
        /// </summary>
        /// <returns>path to be used to load the asset</returns>
        public static string GetPlatformAssetPath(bool isLocalAsset)
        {
            if (IsLocalBuild() || isLocalAsset)
            {
                if (IsEditor())
                {
#if SIMULATE_MOBILE
                    return "file://" + Application.streamingAssetsPath + "/Data";
#else
                    return "file://" + Application.dataPath + "/" + GetAssetFolderPath();
#endif //SIMULATE_MOBILE
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (string.IsNullOrEmpty(mDataPath))
                        mDataPath = "file://" + Application.streamingAssetsPath + "/Data";
                    return mDataPath;
                }
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    if (string.IsNullOrEmpty(mDataPath))
                        mDataPath = "file://" + Application.streamingAssetsPath + "/Data";
                    return mDataPath;
                }
                else if (Application.platform == RuntimePlatform.OSXPlayer)
                {
                    if (string.IsNullOrEmpty(mDataPath))
                        mDataPath = "file://" + Application.streamingAssetsPath + "/Data";
                    return mDataPath;
                }
                else
                {
                    if (string.IsNullOrEmpty(mDataPath))
                        mDataPath = Application.streamingAssetsPath + "/Data";
                    return mDataPath;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(mDataPath))
                    mDataPath = ProductSettings.GetCDNPath();
                return mDataPath;
            }
        }

        /// <summary>
        /// This function returns the path to the cdn.
        /// </summary>
        /// <returns>file path to CDN</returns>
        public static string GetPlatformAssetPath(string inPath)
        {
            if (string.IsNullOrEmpty(mBasePath))
                mBasePath = ProductSettings.GetCDNPath();

            return mBasePath + inPath;
        }

        /// <summary>
        /// Returns device model
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceModel()
        {
            return SystemInfo.deviceModel;
        }

        /// <summary>
        /// Returns current platforms operating system.
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            return SystemInfo.operatingSystem;
        }

        /// <summary>
        /// Returns current Environment
        /// </summary>
        /// <returns></returns>
        public static Environment GetEnvironment()
        {
#if DEV_BUILD
            return Environment.dev;
#elif QA_BUILD
            return Environment.qa;
#elif DEMO_BUILD
            return Environment.demo;
#elif STAGING_BUILD
            return Environment.stage;
#else //LIVE_BUILD
            return Environment.live;
#endif
        }

        /// <summary>
        /// Returns path of CDN url for thsi file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>CDN path</returns>
        public static string GetCDNPath(string fileName)
        {
            return ProductSettings.GetCDNPath() + fileName;
        }

        /// <summary>
        /// Returns persistent data path. Can be used as local storage.
        /// </summary>
        /// <returns>local storage path</returns>
        public static string GetPersistentDataPath()
        {
            if (string.IsNullOrEmpty(mPersistentDataPath))
                mPersistentDataPath = Application.persistentDataPath;//.Replace(":", "_").Replace(" ", "");
            return mPersistentDataPath;
        }

        /// <summary>
        /// On Device it quits the game & on IDE stops play mode
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#elif UNITY_ANDROID
            Application.Quit();
#else
#endif
        }

        /// <summary>
        /// This function just get the name from enum Environment.(Excluding None and End)
        /// </summary>
        /// <returns>Array of string/returns>
        public static string[] GetAllEnvironementType()
        {
            string[] envType = new string[(int)Environment.End];
            for (Environment env = Environment.dev; env < Environment.End; ++env)
            {
                envType[(int)env] = env.ToString().ToUpper();
            }
            return envType;
        }

        /// <summary>
        /// Returns GameType based on current platform & compiler flag
        /// </summary>
        /// <returns></returns>
        public static GameType GetGameType()
        {
            GameType gameType = GameType.None;
#if UNITY_IOS
            gameType = GameType.iOS;
#elif UNITY_ANDROID
#if PS_CASH_APP
            gameType = GameType.AndroidCash;
#else
            gameType = GameType.AndroidFree;
#endif
#endif //UNITY_ANDROID
            return gameType;
        }

       

        /// <summary>
        /// Returns the device type based on current platform
        /// </summary>
        /// <returns></returns>
        public static DeviceType GetDeviceType()
        {
            var currentPlatform = GetCurrentPlatform();
            switch (currentPlatform)
            {
                case Platform.iOS:
                case Platform.Android:
                    return DeviceType.Mobile;
                case Platform.StandAloneOSX:
                case Platform.StandAloneWin:
                    return DeviceType.Desktop;
                case Platform.WebGL:
                    return DeviceType.WebBrowser;
                default:
                    return DeviceType.Others;
            }
        }
    }
}
