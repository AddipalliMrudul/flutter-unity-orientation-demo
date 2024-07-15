using UnityEngine;
using System;
using System.Collections.Generic;
using JungleeGames;

namespace XcelerateGames
{
    /// <summary>
    /// To maintain the products informations.
    /// </summary>
    [CreateAssetMenu(fileName = "ProductSettings", menuName = Utilities.MenuName + "ProductSettings")]
    public class ProductSettings : ScriptableObject
    {
        [System.Serializable]
        public class ProductInfo
        {
            public PlatformUtilities.Platform Platform;
            public string Version = string.Empty;
        }

        public string _AppName = null;
        public string _SecretKey = null;
        public bool _LoadSceneBundlesInEditor = false;
        public bool _SimulateFBLogin = false;
        public string[] _NonBundledLevels = { "Startup" };
        public string _BuildNumber = "1";
        public Product _ProductId = Product.None;
        public List<ChannelIDData> _ChannelData = null;

        [SerializeField] private EnvironmentPath _CDNPaths = null;

        //This is the name of folder user which all assets will be placed. It can be null or empty
        public string ContentFolderInBucket = null;

        public ProductInfo[] Products;

        private ProductInfo mCurrentPlatform = null;
        private static ProductSettings mInstance = null;

        public static void Init()
        {
            if (mInstance == null)
            {
                mInstance = Resources.Load<ProductSettings>("ProductSettings");
                mInstance.mCurrentPlatform = GetCurrentProductInfo();
            }
            if (mInstance == null)
                XDebug.LogException($"Could not find ProductSettings asset under Resources folder");
        }

        public static ProductSettings pInstance
        {
            get
            {
                if (mInstance == null)
                    Init();
                return mInstance;
            }
        }

        private ProductInfo pCurrentPlatform
        {
            get
            {
                if (mInstance.mCurrentPlatform == null)
                {
                    mInstance.mCurrentPlatform = GetCurrentProductInfo();
                }

                return mInstance.mCurrentPlatform;
            }
        }

        /// <summary>
		/// Gets the current product information.
		/// </summary>
		/// <returns>product info</returns>
		public static ProductInfo GetCurrentProductInfo()
        {
            PlatformUtilities.Platform platform = PlatformUtilities.GetCurrentPlatform();

            for (int i = 0; i < pInstance.Products.Length; i++)
            {
                if (pInstance.Products[i].Platform == platform)
                    return pInstance.Products[i];
            }
            XDebug.LogException("Could not find platform data for : " + platform);
            return null;
        }

        public static string GetVersionInfo()
        {
#if LIVE_BUILD
            return $"v {GetProductVersion()}-{mInstance._BuildNumber}";
#else
            return $"v {GetProductVersion()}-{mInstance._BuildNumber} {PlatformUtilities.GetEnvironment()}";
#endif
        }

        public static string GetProductVersion()
        {
            try
            {
                return pInstance.pCurrentPlatform.Version;
            }
            catch (Exception e)
            {
                if (pInstance == null)
                    XDebug.LogException("ProductSettings Instance is null");
                else if (pInstance.pCurrentPlatform == null)
                {
                    XDebug.LogException("mCurrentPlatform is null, creating default");
                    pInstance.mCurrentPlatform = new ProductInfo();
                    pInstance.mCurrentPlatform.Platform = PlatformUtilities.Platform.Android;
                    pInstance.mCurrentPlatform.Version = "1.0";
                }
                else
                    XDebug.LogException(e);
                return "1.0";
            }
        }

        public static int GetProductVersion(int index)
        {
            string[] versions = GetProductVersion().Split('.');

            if (index < versions.Length)
            {
                int v;
                if (Int32.TryParse(versions[index], out v))
                    return v;
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// Returns path of CDN from where we will fetch additions files. The path is created based on the environment (dev, qa etc), platform(Android, ios etc...) & version of the game.
        /// </summary>
        /// <returns></returns>
        public static string GetCDNPath()
        {
            return $"{pInstance._CDNPaths.Path}/{pInstance.ContentFolderInBucket}/{PlatformUtilities.GetCurrentPlatform()}/";
        }

        /// <summary>
        /// Returns a list of all supported platforms
        /// </summary>
        /// <returns></returns>
        public static List<PlatformUtilities.Platform> GetSupportedPlatforms()
        {
            List<PlatformUtilities.Platform> platforms = new List<PlatformUtilities.Platform>();
            foreach (ProductInfo productInfo in pInstance.Products)
                platforms.Add(productInfo.Platform);
            return platforms;
        }

        /// <summary>
        /// Returns chanel id based on current game type
        /// </summary>
        /// <returns></returns>
        public static ChannelId GetChannelId()
        {
            return pInstance._ChannelData.Find(e => e._GameType == PlatformUtilities.GetGameType())._ChannelId;
        }
    }
}
