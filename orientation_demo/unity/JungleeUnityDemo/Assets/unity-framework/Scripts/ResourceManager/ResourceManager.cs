using UnityEngine;
using System;
using System.Collections.Generic;
using ResEvent = System.Action<XcelerateGames.AssetLoading.ResourceEvent, string, object, object>;

using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using XcelerateGames.UI;
using UnityEngine.Scripting;
// using System.Security.Policy;

#if UNITY_EDITOR

using UnityEditor;

#endif
[assembly: Preserve]

namespace XcelerateGames.AssetLoading
{
    /*
     * Init Mode
     */
    public enum InitMode
    {
        Default,    /**<manifest & version list will be loaded before sending ready callback */
        Delayed     /**<manifest is loaded & ready callback is triggered. version list is loaded there after*/
    }

    /*
     * Resoure event
     */
    public enum ResourceEvent
    {
        ERROR,      /**<Sent on asset loading fail */
        COMPLETE,   /**<Sent on asset loading complete */
        PROGRESS,   /**<Progress of asset loading. Value between 0 & 1*/
    }

    public static class ResourceManager
    {
        /*
         *  Resource type
         */
        public enum ResourceType
        {
            Text,           /**<Text asset such as textfile, json file or xml file */
            AssetBundle,    /**< Asset bundle */
            Object,         /**< An object from within asset bundle*/
            Texture,        /**< Image if any format*/
            Video,          /**< Video file. MP4 only*/
            AudioClip,      /**< Any valid audio file such as mp3, ogg, wav*/
            Pdf,            /**< PDF files*/
            NONE            /**< Undefined*/
        }

        #region Member variables

        internal static List<Resource> mResourceLoadingList = new List<Resource>(); /**< List of all assets that are being loaded*/
        internal static List<Resource> mResources = new List<Resource>(); /**< List of all assets that are already loaded*/
        internal static Dictionary<string, string> mLoadedAssetsList = new Dictionary<string, string>(); /**< Dictionary of all assets that are already loaded. Key is asset name & hash is value*/

        private static Dictionary<string, string> mCache = new Dictionary<string, string>(); /**< Will hold info of cached assets, file name being key & hash being the value.*/

        private static string[] mVariants = { }; /**< List of all variants.*/
        private static string mCurrentScene = null; /**< Curent scene that is loaded.*/
        private static UiLoadingScreen mUiLoadingScreen = null;/**< Reference to loading screen UI. Will be created internally when scene loading starts.*/
        private static string mLastScene = string.Empty;/**< Last scene loaded.*/
        private static bool mIsSceneLoading = false;/**< Is any scene being loaded.*/
        private static AssetBundleManifest mAssetBundleManifest = null;/**< Manifest file.*/
        private static string[] mBundlesWithVariant = null; /**< List of all asset bundles that have a variant*/
        private static AssetBundle mResourceBundle = null;/**< Resources asset bundle.*/
        private static Dictionary<string, string> mShippedAssets = null;/**< Dictionary of all assets that are shipped(bundled) with app. Key is asset name & hash is value*/
        public static Dictionary<string, AssetData> mAssetVersions = null;/**< Dictionary of all assets. Key is asset name & value is AssetData.*/
        public static InitMode pInitMode { get; private set; }  /**< Initialisation mode used to init ResourceManager.*/
        public static Func<UiLoadingScreen> LoadLoadingScreenFunc = null; /**< Set a function pointer to control the loading of Loading Screen UI.*/

        public const string mAssetVersionListFileName = "version_list.json";
        public const string PrimaryPrefetchList = "prefetch_list.json";
        public const string SecondaryPrefetchList = "prefetch_list_secondary.json";
        public const string ResourcesBundleName = "resources";

        #endregion Member variables

        #region Events & Delegates

        private static System.Action OnReady = null; /**< Callback triggered on ResourceManager is Ready. Add a listener from ResourceManager::RegisterOnReadyCallback*/
        public static System.Action<string> OnLoadSceneRequested = null;/**< Callback when scene is about to be loaded.*/
        public static System.Action<string> OnSceneLoadedEvent = null;/**< .Callback when scene is loaded*/

        #endregion Events & Delegates

        #region Setters & Getters

        public static UiLoadingScreen pUiLoadingScreen { get { return mUiLoadingScreen; } }
        public static List<Resource> pResources { get { return mResources; } }
        public static List<Resource> pResourcesLoadingList { get { return mResourceLoadingList; } }
        public static string[] pVariants { get { return mVariants; } }
        public static string pCurrentScene
        {
            get
            {
                if (mCurrentScene.IsNullOrEmpty())
                    mCurrentScene = SceneManager.GetActiveScene().name;
                return mCurrentScene;
            }
        }
        public static string pLastScene { get { return mLastScene; } }
        public static bool pIsSceneLoading { get { return mIsSceneLoading; } }
        public static Dictionary<string, string> pShippedAssets { get { return mShippedAssets; } }
        public static Dictionary<string, string> pCache { get { return mCache; } }
        public static string BundledAssetsAssetName { get { return "BundledAssets-" + PlatformUtilities.GetCurrentPlatform(); } }

        //We are not ready until we are done loading AssetBundleManifest. We should wait for this to load.
        public static bool pIsReady { get { return mAssetVersions != null; } }
        public static string[] Variants => mVariants; /**<Return List of all variants.*/

        #endregion Setters & Getters

        #region AssetBundle Simulation

#if UNITY_EDITOR
        private static int mSimulateAssetBundles = -1;
        private const string mSimulateAssetBundlesKey = "SimulateAssetBundles";

        public static bool pSimulateAssetBundles
        {
            get
            {
                if (mSimulateAssetBundles == -1)
                    mSimulateAssetBundles = EditorPrefs.GetBool(mSimulateAssetBundlesKey, false) ? 1 : 0;

                return mSimulateAssetBundles != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mSimulateAssetBundles)
                {
                    mSimulateAssetBundles = newValue;
                    EditorPrefs.SetBool(mSimulateAssetBundlesKey, value);
                }
            }
        }

        private static object GetAsset(string rootAsset, string assetName)
        {
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(rootAsset, assetName);
            if (assetPaths.Length == 0)
            {
                List<string> assets = new List<string>(AssetDatabase.GetAssetPathsFromAssetBundle(rootAsset));
                if (assets.Count == 0)
                {
                    UnityEngine.Debug.LogError($"There is no asset with name : {assetName} in {rootAsset}");
                    return null;
                }
                else
                {
                    string assetPath = assets.Find(delegate (string path)
                    {
                        string assetth = Path.GetFileNameWithoutExtension(path);
                        return assetth == assetName;
                    });
                    return AssetDatabase.LoadMainAssetAtPath(assetPath);
                }
            }
            else
                return AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
        }

#endif

        #endregion AssetBundle Simulation

        #region Public Methods of ResourceManager

        /// <summary>
        /// Add a callback to get notified when ResourceManager is ready.
        /// </summary>
        /// <param name="callback"></param>

        public static void RegisterOnReadyCallback(Action callback)
        {
            if (callback != null)
            {
                if (pIsReady)
                    callback();
                else
                    OnReady += callback;
            }
        }

        /// <summary>
        /// Remove a callback added via RegisterOnReadyCallback.
        /// </summary>
        /// <param name="callback"></param>
        public static void UnregisterOnReadyCallback(Action callback)
        {
            OnReady -= callback;
        }

        /// <summary>
        /// Set variant based on available sysyetm memory. This can be used to server different assets based on device specs
        /// </summary>
        /// <param name="minMem"></param>
        public static void SetVariant(int minMem)
        {
            AddVariant(MobileUtilities.GetSystemMemory() > minMem ? "hd" : "sd");
        }

        /// <summary>
        /// Adds a new variant to list. Will be helpful to swicth assetbundles based on some rule.
        /// </summary>
        /// <param name="variant"></param>
        public static void AddVariant(string variant)
        {
            List<string> temp = new List<string>(mVariants);
            temp.Add(variant);
            mVariants = temp.ToArray();
            temp = null;
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"Adding variant: {variant}, All available variants: {mVariants.Printable(',')}", XDebug.Mask.Resources);
        }

        /// <summary>
        /// Removes the given variant from the list.
        /// </summary>
        /// <param name="variant"></param>
        public static void RemoveVariant(string variant)
        {
            //First check if the varient that we are trying to remove is in the list.
            int index = System.Array.FindIndex(mVariants, e => e == variant);
            if (index != -1)
            {
                //Varient exists, remove it now.
                List<string> temp = new List<string>(mVariants);
                temp.Remove(variant);
                mVariants = temp.ToArray();
                temp = null;
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Removed variant: {variant}", XDebug.Mask.Resources);
            }
        }

        /// <summary>
        /// Clears all variants
        /// </summary>
        public static void ClearVariants()
        {
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log("Cleared all variants", XDebug.Mask.Resources);
            mVariants = new string[] { };
        }

        /// <summary>
        /// Return the resource type based on the file extension.
        /// </summary>
        /// <returns></returns>
        public static ResourceType GetResourceType(string inURL)
        {
            string extn = Path.GetExtension(inURL).ToLower();
            ResourceType resourceType = ResourceType.NONE;

            switch (extn)
            {
                case ".xml":
                case ".tsv":
                case ".csv":
                case ".json":
                case ".txt":
                    resourceType = ResourceType.Text;
                    break;

                case ".mp4":
                    resourceType = ResourceType.Video;
                    break;

                case ".mp3":
                case ".wav":
                case ".ogg":
                    resourceType = ResourceType.AudioClip;
                    break;

                case "":
                case null:
                default:
                    resourceType = ResourceType.AssetBundle;
                    break;
            }
            if (resourceType == ResourceType.NONE)
            {
                if (extn.EndsWith("png", StringComparison.OrdinalIgnoreCase) || extn.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || inURL.Contains("fbcdn")
                    || inURL.Contains("facebook.com") || inURL.StartsWith("https://fb", StringComparison.OrdinalIgnoreCase))
                {
                    if (inURL.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || inURL.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                        return ResourceType.Texture;
                    else
                        return ResourceType.Object;
                }
            }

            return resourceType;
        }

        /// <summary>
        /// Returns true if the given asset path is of a text asset (*.txt, *.json etc)
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static bool IsTextAsset(string assetPath)
        {
            return GetResourceType(assetPath) == ResourceType.Text;
        }

        /// <summary>
        /// Add the given asset to cahced asset list. File name is key & hash is value
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hash"></param>
        public static void AddToCache(string fileName, string hash)
        {
            if (XDebug.CanLog(XDebug.Mask.Resources | XDebug.Mask.UpdateManager))
                XDebug.Log($"Adding/updating: {fileName} to cache with hash: {hash}", XDebug.Mask.Resources);
            if (!mCache.ContainsKey(fileName))
                mCache.Add(fileName, hash);
            else
                mCache[fileName] = hash;
            UnityEngine.PlayerPrefs.SetString("CachedData", mCache.ToJson());
            UnityEngine.PlayerPrefs.Save();
        }

        /// <summary>
        /// Removes the given asset from cached assets list
        /// </summary>
        /// <param name="fileName"></param>
        public static void RemoveFromCache(string fileName)
        {
            if (mCache.ContainsKey(fileName))
            {
                mCache.Remove(fileName);
                UnityEngine.PlayerPrefs.SetString("CachedData", mCache.ToJson());
                UnityEngine.PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Returns true if the given asset bundle is cached else returns false.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static bool IsCached(string assetPath, ResourceType inResoureType)
        {
            //Check if the key exists,
            if (mCache.ContainsKey(assetPath))
            {
                if (!ConnectivityMonitor.pIsInternetAvailable)
                {
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager | XDebug.Mask.Resources))
                        XDebug.Log($"{assetPath} was cached: ({mCache[assetPath]}), in offline mode, returnig true", XDebug.Mask.Resources);
                    return true;
                }
                string assetHash = GetVersionAssetHash(assetPath);
                if (!string.IsNullOrEmpty(assetHash) && !assetHash.Equals(mCache[assetPath]))
                {
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager | XDebug.Mask.Resources))
                        XDebug.Log($"{assetPath} +  was cached({mCache[assetPath]}), new update available : {assetHash}", XDebug.Mask.Resources);
                    return false;
                }
                if (UnityEngine.PlayerPrefs.HasKey(assetPath))
                {
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log($"{assetPath} is cached in player prefs WITH HASH : {assetHash}", XDebug.Mask.Resources);
                    return true;
                }
                if (inResoureType == ResourceType.AssetBundle)
                {
                    Hash128 abHash = Hash128.Parse(assetHash);
                    if (Caching.IsVersionCached(PlatformUtilities.GetCDNPath(assetPath), abHash))
                    {
                        if (XDebug.CanLog(XDebug.Mask.Resources))
                            XDebug.Log($"{assetPath} is cached in Unity cache with hash : {assetHash}", XDebug.Mask.Resources);
                        return true;
                    }
                }
            }

            if (PlatformUtilities.IsLocalBuild())
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if the given file is cached by Unity. Note this is Unity Cache NOT our own system
        /// </summary>
        /// <param name="bundleName">Name of the asset bundle</param>
        /// <returns>true if cached else false</returns>
        public static bool IsAssetBundleCached(string bundleName)
        {
            if (PlatformUtilities.IsLocalBuild())
                return true;
            bundleName = ResolveVariantName(bundleName);
            if (IsShippedWithApp(bundleName))
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Asset {bundleName} is shipped with the app", XDebug.Mask.Resources);
                return true;
            }
            bool cached = Caching.IsVersionCached(GetAssetURL(bundleName), Hash128.Parse(GetVersionAssetHash(bundleName)));
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"Asset {GetAssetURL(bundleName)} is cached {cached}", XDebug.Mask.Resources);
            return cached;
        }

        /// <summary>
        /// Returns a list of all cached versions 
        /// </summary>
        /// <param name="bundleName">name of the asset bundle</param>
        /// #example tutorials 
        /// #example tutorials/PfUiTutorials
        /// <returns>list of all hashes that are cached at client</returns>
        public static List<string> GetCachedVersions(string bundleName)
        {
            List<Hash128> hashes = new List<Hash128>();
            Caching.GetCachedVersions(SplitAssetName(bundleName).bundleName, hashes);
            return hashes.ConvertAll(e => e.ToString());
        }

        /// <summary>
        /// Returns true if the given asset has an update available else returns false
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="inResoureType"></param>
        /// <returns></returns>
        public static bool IsUpdateAvailable(string assetPath, ResourceType inResoureType)
        {
            if (mAssetVersions == null || inResoureType == ResourceType.Video)
                return false;
            string hash = GetVersionAssetHash(assetPath);
            if (string.IsNullOrEmpty(hash))
                return false;
            if (!hash.Equals(GetBundledAssetHash(assetPath)))
            {
                if (IsCached(assetPath, inResoureType))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given asset name is shipped (bundled) within the app
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static bool IsShippedWithApp(string inPath)
        {
            if (mShippedAssets != null)
                return mShippedAssets.ContainsKey(inPath);

            return false;
        }

        /// <summary>
        /// Returns hash for the given bundle.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        //public static string GetAssetBundleHash(string bundleName)
        //{
        //    if (mAssetBundleManifest != null)
        //        return mAssetBundleManifest.GetAssetBundleHash(GetBundleName(bundleName)).ToString();
        //    return string.Empty;
        //}

        /// <summary>
        /// Returns hash of the file that was shipped with the app.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static string GetBundledAssetHash(string bundleName)
        {
            try
            {
                if (mShippedAssets.ContainsKey(bundleName))
                    return mShippedAssets[bundleName];
                return string.Empty;
            }
            catch (Exception)
            {
                XDebug.LogException("GetBundledAssetHash : " + bundleName);
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns hash of the file as per Version list downloaded from CDN. Send only bundle name. DO not pass bundle name followed by asset name
        /// #example Correct tutorials
        /// #example Wrong tutorials/PfUiTutorials
        /// </summary>
        /// <param name="bundleName">Name of the asset bundle to get hash</param>
        /// <returns>hash if asset bundle is found else null</returns>
        public static string GetVersionAssetHash(string bundleName)
        {
            if (mAssetVersions != null)
            {
                bundleName = ResolveVariantName(bundleName);
                if (mAssetVersions.ContainsKey(bundleName))
                    return mAssetVersions[bundleName].hash;
                else
                {
                    XDebug.LogError($"ERROR! could not find {bundleName} in mAssetVersions");
                    return null;
                }
            }
            XDebug.LogError("ERROR! mAssetVersions is null");
            return string.Empty;
        }

        /// <summary>
        /// Returns size of the asset bundle in bytes. If the asset bundle is not found, returns zero.
        /// </summary>
        /// <param name="assetBundle">Name of the asset bundle to query the size for</param>
        /// <returns>size of asset bundle in bytes</returns>
        public static ulong GetBundleSize(string assetBundle)
        {
            if (mAssetVersions != null)
            {
                assetBundle = ResolveVariantName(assetBundle);
                if (mAssetVersions.ContainsKey(assetBundle))
                    return mAssetVersions[assetBundle].size;
                else
                {
                    XDebug.LogError($"ERROR! could not find {assetBundle} in mAssetVersions");
                    return 0;
                }
            }
            XDebug.LogError("ERROR! mAssetVersions is null");
            return 0;
        }

        /// <summary>
        /// Returns size(in bytes) of all the asset bundle names in the given list.
        /// </summary>
        /// <param name="assetBundleNames">List of asset bundle names to query the size for</param>
        /// <returns>size of all asset bundles in bytes</returns>
        public static ulong GetBundlesSize(List<string> assetBundleNames)
        {
            ulong size = 0;
            if (assetBundleNames != null)
                assetBundleNames.ForEach(e => size += GetBundleSize(e));

            return size;
        }

        /// <summary>
        /// Returns absolute URL of the asset by appending hash
        /// </summary>
        /// <returns>The asset URL.</returns>
        /// <param name="assetName">Asset name.</param>
        public static string GetAssetURL(string assetName)
        {
            return PlatformUtilities.GetPlatformAssetPath(assetName) + "/" + GetVersionAssetHash(assetName);
        }

        /// <summary>
        /// Returns path to the asset based on the current platform.
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="shouldCache"></param>
        /// <returns></returns>
        public static (string path, bool isLocal) ResolvePath(string inPath, ResourceType inResoureType, bool forceLocal)
        {
            bool useShippedAsset = true;
            if (!forceLocal && ConnectivityMonitor.pIsInternetAvailable && !PlatformUtilities.IsLocalBuild() && IsUpdateAvailable(inPath, inResoureType))
            {
                string path = GetAssetURL(inPath);
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Update available for {inPath}, Resolved path: {path}", XDebug.Mask.Resources);
                return (path, true);
            }
            useShippedAsset = !IsCached(Path.GetFileName(inPath), inResoureType);

            string basePath = null;
            if (useShippedAsset || PlatformUtilities.IsLocalBuild() || forceLocal)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log("Using shipped asset " + inPath, XDebug.Mask.Resources);
                useShippedAsset = IsShippedWithApp(inPath);
                basePath = PlatformUtilities.GetPlatformAssetPath(useShippedAsset);
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log("Using cached file " + inPath, XDebug.Mask.Resources);
                basePath = "file://" + PlatformUtilities.GetPersistentDataPath();
                inPath = inPath.Replace('/', Path.DirectorySeparatorChar);
            }

            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log("Resolved Path for : " + inPath + " is -> " + basePath, XDebug.Mask.Resources);
            char separator = Path.DirectorySeparatorChar;
            if (basePath.StartsWith("http"))
                separator = '/';
            return (basePath + separator + inPath, !useShippedAsset);
        }

        /// <summary>
        /// Remaps the asset bundle name to the best fitting asset bundle variant. Variant can be added from ResourceManager::AddVariant function
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public static string ResolveVariantName(string assetBundleName)
        {
            if (mAssetBundleManifest == null)
            {
                XDebug.LogError($"mAssetBundleManifest is null. Will not resolve bundle name for {assetBundleName}");
                return assetBundleName;
            }
            if (mBundlesWithVariant.Length == 0)
                return assetBundleName;

            // Get base bundle name
            string baseName = assetBundleName.Split('.')[0];

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < mBundlesWithVariant.Length; i++)
            {
                string[] curSplit = mBundlesWithVariant[i].Split('.');
                string curBaseName = curSplit[0];
                string curVariant = curSplit[1];

                if (curBaseName != baseName)
                    continue;

                int found = System.Array.IndexOf(mVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFit == int.MaxValue - 1)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Ambigious asset bundle variant chosen because there was no matching active variant: {mBundlesWithVariant[bestFitIndex]}", XDebug.Mask.Resources);
            }

            if (bestFitIndex != -1)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Resolved Variant for : {assetBundleName} is : {mBundlesWithVariant[bestFitIndex]}", XDebug.Mask.Resources);
                return mBundlesWithVariant[bestFitIndex];
            }
            else
                return assetBundleName;
        }

        /// <summary>
        /// Loads an asset of type T synchronously. Use this function to load assest when you know that the asset bundle is already loaded.
        /// Returns null if the asset bundle is not loaded already
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static T LoadAssetFromBundle<T>(string inPath) where T : UnityEngine.Object
        {
            object obj = LoadAssetFromBundle(inPath);
            if (obj == null)
            {
                Debug.LogError("There is no asset : " + inPath);
                return null;
            }
            else
                return obj as T;
        }

        /// <summary>
        /// Loads an asset synchronously. Use this function to load assest when you know that the asset bundle is already loaded.
        /// Returns null if the asset bundle is not loaded already. Type cast the returned object to the required type. 
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static object LoadAssetFromBundle(string inPath)
        {
            if (string.IsNullOrEmpty(inPath))
            {
                XDebug.LogError("Trying to load from null/empty path", XDebug.Mask.Resources);
                return null;
            }

            (string bundleName, string assetName) data = SplitAssetName(inPath);
            data.bundleName = ResolveVariantName(data.bundleName);

            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"LoadAssetFromBundle : path : {inPath}", XDebug.Mask.Resources);

            #region AssetBundle Simulation

#if UNITY_EDITOR
            if (pSimulateAssetBundles && !string.IsNullOrEmpty(data.assetName) && data.bundleName != data.assetName)
            {
                object obj = GetAsset(data.bundleName, data.assetName);
                if (obj == null)
                {
                    XDebug.LogError($"There is no asset with name : {data.assetName} in {data.bundleName}", XDebug.Mask.Resources);
                    return null;
                }
                else
                    return obj;
            }
#endif

            #endregion AssetBundle Simulation

            Resource res = GetResource(data.bundleName);
            if (res != null)
                return res.mAssetBundleHandler.assetBundle.LoadAsset(data.assetName);
            else
            {
                XDebug.LogError($"Could not find asset : \"{inPath}\". Make sure the bundle is loaded before calling this function.", XDebug.Mask.Resources);
                return null;
            }
        }

        /// <summary>
        /// Returns raw bytes of the asset. Can be used to perform operation on bytes such as save to disc, create texture etc
        /// </summary>
        /// <param name="assetName">Name of the asset</param>
        /// <param name="isURL">are we gettings bytes of asset in AssetBundle or from remote URL</param>
        /// <returns></returns>
        public static byte[] GetBytes(string assetName, bool isURL = false)
        {
            string rootAsset = isURL ? assetName : GetRootAsset(assetName);

            Resource res = GetResource(rootAsset);
            if (res != null)
            {
                if (res.mResourceType == ResourceType.Texture)
                    return res.mTextureHandler.data;
                else if (res.mResourceType == ResourceType.Pdf || res.mResourceType == ResourceType.Video)
                    return res.mDownloadHandler.data;
                else if (res.mResourceType == ResourceType.AudioClip)
                    return res.mAudioClipHandler.data;
                else
                    XDebug.LogError($"IMPLEMENT FUNCTIONALITY FOR TYPE {res.mResourceType} asset name : {assetName}");
            }
            else
                XDebug.LogError($"This function must be called after asset has been loaded {assetName}");
            return null;
        }

        /// <summary>
        /// Returns true if the given asset is loaded, else returns false
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static bool IsAssetLoaded(string inPath)
        {
            return mLoadedAssetsList.ContainsKey(inPath);
        }

        /// <summary>
        /// Loads the given asset by id. Add the asset name in the mapping created by AssetConfigData
        /// </summary>
        /// <param name="id">id of the asset</param>
        /// <param name="inEvent">callback</param>
        /// <param name="inResourceType">type of resource to load</param>
        /// <param name="inUserData">user data to be passed with callback</param>
        /// <param name="loadDependency">should we load dependent assets</param>
        /// <param name="dontDestroyOnLoad">mark the asset dont destroy on load. If set to true, asset will not be unloaded on scene transition</param>
        /// <param name="downloadOnly">If set to true, asset will not be extracted from asset bundle</param>
        /// @see AssetConfigData for more details on how to add config for asset
        /// @see Load(string, ResEvent, ResourceType, object, bool, bool, bool)
        public static void LoadById(string id, ResEvent inEvent, ResourceType inResourceType, object inUserData = null, bool loadDependency = true, bool dontDestroyOnLoad = false, bool downloadOnly = false)
        {
            Load(AssetConfigData.GetAssetName(id), inEvent, inResourceType, inUserData, loadDependency, dontDestroyOnLoad, downloadOnly);
        }

        /// <summary>
        /// Loads the given asset, once done gives a callback with loading status.
        /// </summary>
        /// <param name="inEvent">callback</param>
        /// <param name="inResourceType">type of resource to load</param>
        /// <param name="inUserData">user data to be passed with callback</param>
        /// <param name="loadDependency">should we load dependent assets</param>
        /// <param name="dontDestroyOnLoad">mark the asset dont destroy on load. If set to true, asset will not be unloaded on scene transition</param>
        /// <param name="downloadOnly">If set to true, asset will not be extracted from asset bundle</param>
        /// ### Example
        /// @include Load.cs
        public static void Load(string inPath, ResEvent inEvent, ResourceType inResourceType, object inUserData = null, bool loadDependency = true, bool dontDestroyOnLoad = false, bool downloadOnly = false)
        {
            if (string.IsNullOrEmpty(inPath))
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    Debug.LogError("Error! Trying to load an asset from an empty path.");
                if (inEvent != null)
                    inEvent(ResourceEvent.ERROR, inPath, null, inUserData);
                return;
            }

            (string bundleName, string assetName) data = SplitAssetName(inPath);
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"Loading asset : {inPath}, BundleName: {data.bundleName}, AssetName: {data.assetName}, ResourceType: {inResourceType}", XDebug.Mask.Resources);

            #region AssetBundle Simulation

#if UNITY_EDITOR
            if (inEvent != null && pSimulateAssetBundles && !string.IsNullOrEmpty(data.assetName) && data.bundleName != data.assetName)
            {
                object obj = GetAsset(data.bundleName, data.assetName);
                if (obj == null)
                {
                    XDebug.LogError($"There is no asset with name : {data.assetName} in {data.bundleName}", XDebug.Mask.Resources);
                    inEvent(ResourceEvent.ERROR, inPath, null, inUserData);
                    return;
                }
                else
                {
                    inEvent(ResourceEvent.COMPLETE, inPath, obj, inUserData);
                    return;
                }
            }
#endif

            #endregion AssetBundle Simulation

            if (inResourceType == ResourceType.AssetBundle || inResourceType == ResourceType.Object)
                data.bundleName = ResolveVariantName(data.bundleName);

            if (!PlatformUtilities.IsLocalBuild() && inResourceType == ResourceType.Text)
            {
                if (UnityEngine.PlayerPrefs.HasKey(inPath))
                {
                    //Try pulling the file from PlayerPrefs.
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log($"Loading : {inPath} from PlayerPrefs.", XDebug.Mask.Resources);
                    inEvent(ResourceEvent.COMPLETE, inPath, UnityEngine.PlayerPrefs.GetString(inPath), inUserData);
                    return;
                }
            }
            Resource res = GetResource(data.bundleName);
            if (res != null)
            {
                //Resource already loaded, trigger loaded event.
                res.AddAsset(data.bundleName, data.assetName, inUserData, inEvent, inResourceType);
                res.OnResourceLoaded();
                //If this call wants this resource to be dontDestroyOnLoad
                if (dontDestroyOnLoad)
                    res.mImmortal = true;
            }
            else
            {
                //This resource is not yet loaded, see if it is in the loading list.
                mResourceLoadingList.RemoveAll(e => (e == null));
                res = GetLoadingResource(data.bundleName);
                if (res != null)
                {
                    //The resource is still being loaded, add the event handler.
                    res.AddAsset(data.bundleName, data.assetName, inUserData, inEvent, inResourceType);
                    //If this call wants this resource to be dontDestroyOnLoad
                    if (dontDestroyOnLoad)
                        res.mImmortal = true;
                }
                else
                {
                    //The resource is not loaded yet, add it to the loading list.
                    Resource rs = new Resource(data.bundleName, data.assetName, inEvent, inUserData, loadDependency, dontDestroyOnLoad, inResourceType, downloadOnly);
                    rs.AddAsset(data.bundleName, data.assetName, inUserData, inEvent, inResourceType);
                    mResourceLoadingList.Add(rs);
                }
            }
        }

        /// <summary>
        /// Load the given asset from the resources folder. We first try to load it from AssetBundle, if that fails then we try to load from Resources directly
        /// </summary>
        /// <param name="inResourceName"></param>
        /// <returns></returns>
        public static UnityEngine.Object LoadFromResources(string inResourceName)
        {
#if HANDLE_EXCEPTIONS
            try
#endif //HANDLE_EXCEPTIONS

            {
                UnityEngine.Object obj = null;
#if UNITY_EDITOR
                if (pSimulateAssetBundles)
                {
                    obj = Resources.Load(inResourceName);
                    if (obj == null)
                        XDebug.LogError("Could not find resource : " + inResourceName + " in Resources folder", XDebug.Mask.Resources);
                    return obj;
                }
#endif
                //First try loading the asset from the bundle.
                if (mResourceBundle != null)
                    obj = mResourceBundle.LoadAsset(inResourceName);
                //If we cant find that in Resource bundle, try loading from Resources directly.
                if (obj == null)
                    obj = Resources.Load(inResourceName);
                if (obj == null)
                    XDebug.LogError("Could not find resource : " + inResourceName + " in Resources folder", XDebug.Mask.Resources);
                return obj;
            }
#if HANDLE_EXCEPTIONS
            catch (Exception e)
            {
                XDebug.LogException($"Exception thrown while trying to load : {inResourceName} \n Message : {e.Message}\nStack\n {e.StackTrace}");
                return null;
            }
#endif //HANDLE_EXCEPTIONS
        }

        /// <summary>
        /// Loads a specified asset of given type from Resources folder. We first try to load it from AssetBundle, if that fails then we try to load from Resources directly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inResourceName"></param>
        /// <returns></returns>
        public static T LoadFromResources<T>(string inResourceName) where T : UnityEngine.Object
        {
#if HANDLE_EXCEPTIONS
            try
#endif //HANDLE_EXCEPTIONS
            {
#if UNITY_EDITOR
                if (pSimulateAssetBundles)
                {
                    T objOfType = Resources.Load<T>(inResourceName);
                    if (objOfType == null)
                        XDebug.LogError("Could not find resource : " + inResourceName + " in Resources folder", XDebug.Mask.Resources);
                    return objOfType;
                }
#endif
                UnityEngine.Object obj = null;
                //First try loading the asset from the bundle.
                if (mResourceBundle != null)
                    obj = mResourceBundle.LoadAsset<T>(inResourceName);
                //If we cant find that in Resource bundle, try loading from Resources directly.
                if (obj == null)
                    obj = Resources.Load<T>(inResourceName);
                if (obj == null)
                    XDebug.LogError($"Could not find : {inResourceName} in Resources folder and {mResourceBundle?.name} bundle", XDebug.Mask.Resources);
                return obj as T;
            }
#if HANDLE_EXCEPTIONS
            catch (Exception e)
            {
                XDebug.LogException($"Exception thrown while trying to load : {inResourceName} \n Message : {e.Message}\nStack\n {e.StackTrace}");
                return null;
            }
#endif //HANDLE_EXCEPTIONS
        }

        /// <summary>
        /// Load the scene by name. This function internally creates a loading screen.
        /// </summary>
        /// <param name="sceneName"></param>
        public static void LoadScene(string sceneName)
        {
            FrameworkEventManager.LogEvent("unity_load_scene_started");

            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"Loading Scene : {sceneName}", XDebug.Mask.Resources);
            if (OnLoadSceneRequested != null)
                OnLoadSceneRequested(sceneName);
            mIsSceneLoading = true;
            mLastScene = mCurrentScene;
            mCurrentScene = sceneName;

            ShowLoadingScreen();
            KillAllLoads();
            ProcessLoadScene();
        }

        /// <summary>
        /// Callback for Loading screen destroyed
        /// </summary>
        private static void OnLoadingScreenDestroyed()
        {
        }

        /// <summary>
        /// Reloads the level. If the scene is being loaded from a bundle then call this function to reload the level.
        /// Example is : Level Restart.
        /// </summary>
        public static void ReloadScene()
        {
            KillAllLoads();
            SceneManager.LoadScene(mCurrentScene);
        }

        public static void ProcessLoadScene()
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager | XDebug.Mask.Resources))
                XDebug.Log($"ProcessLoadScene : Starting LoadSceneAsync {mCurrentScene}", XDebug.Mask.Resources);
            LoadSceneAsync(mCurrentScene);
        }

        /// <summary>
        /// Destroys the loading screen. This function has to be called explicitly after a level load.
        /// Use this function when you want to control when the loading screen needs to be destroyed. Ex, waiting for user data to load.
        /// If there is no need to wait, add DestroyLoadingScreen component to any empty GameObject in scene. 
        /// </summary>
        public static void DestroyLoadingScreen()
        {
            //Debug.Log($"LTS: DestroyLoadingScreen: {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            if (mUiLoadingScreen != null)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log("Received request to DestroyLoadingScreen", XDebug.Mask.Resources);
                mUiLoadingScreen.DestroySelf();
                mUiLoadingScreen = null;
            }
        }

        /// <summary>
        /// Show loading screen. Typical usage is to show a loading screen while loading scene
        /// </summary>
        public static void ShowLoadingScreen()
        {
            DestroyLoadingScreen();
            if (LoadLoadingScreenFunc != null)
            {
                mUiLoadingScreen = LoadLoadingScreenFunc.Invoke();
                if (mUiLoadingScreen != null)
                    mUiLoadingScreen._OnLoadingComplete += OnLoadingScreenDestroyed;
            }
            else
            {
                if (mUiLoadingScreen == null)
                {
                    GameObject obj = LoadFromResources<GameObject>("PfUiLoadingScreen");
                    if (obj != null)
                    {
                        if (XDebug.CanLog(XDebug.Mask.Resources))
                            XDebug.Log("Showing Loading Screen", XDebug.Mask.Resources);
                        GameObject loadLevel = GameObject.Instantiate(obj) as GameObject;
                        mUiLoadingScreen = loadLevel.GetComponentInChildren<UiLoadingScreen>();
                        mUiLoadingScreen._OnLoadingComplete += OnLoadingScreenDestroyed;
                    }
                }
            }
        }

        /// <summary>
        /// Waits for the end of frame & unloads the asset from memory.
        /// </summary>
        /// <param name="inURL">Name of the asset to unload</param>
        /// <param name="forceUnload">Assets are unloaded from memory only when reference count goes to zero. Set to true if you want to release momory ignoring reference count. Use this with caution</param>
        /// <param name="unloadLoadedObjects">If set to true, Along with AssetBundle, deletes the instatiated object as well</param>
        /// <param name="unloadDependencies">Recursivley unloads dependent assets. Must be set to true, else can lead to memory leaks as reference count will get messed up</param>
        /// <returns></returns>
        private static IEnumerator DoUnload(string inURL, bool forceUnload = false, bool unloadLoadedObjects = true, bool unloadDependencies = true)
        {
            yield return new WaitForEndOfFrame();
            string rootAsset = GetRootAsset(inURL);
            rootAsset = ResolveVariantName(rootAsset);
            if (rootAsset != null)
            {
                Resource res = GetResource(rootAsset);
                if (res != null)
                {
                    if (res.Release(forceUnload, unloadLoadedObjects, unloadDependencies))
                    {
                        mLoadedAssetsList.Remove(inURL);
                        mResources.Remove(res);
                        res = null;
                        mResources.RemoveAll(e => (e == null));

                        if (XDebug.CanLog(XDebug.Mask.Resources | XDebug.Mask.UpdateManager))
                            XDebug.Log($"Unloaded : {rootAsset}", XDebug.Mask.Resources);
                    }
                    else
                    {
                        if (XDebug.CanLog(XDebug.Mask.Resources | XDebug.Mask.UpdateManager))
                            XDebug.Log($"DoUnload : Did not unload resource {rootAsset}, might have references", XDebug.Mask.Resources);
                    }
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Resources | XDebug.Mask.UpdateManager))
                        XDebug.LogError($"DoUnload : Failed to find asset {rootAsset}", XDebug.Mask.Resources);
                }
            }
        }

        /// <summary>
        /// Unloads the asset. Actual unload will be done at end of frame from ResourceManager::DoUnload function
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static void Unload(string inURL, bool forceUnload = false, bool unloadLoadedObjects = true, bool unloadDependencies = true)
        {
            if (AsyncLoader.pInstance != null)
                AsyncLoader.pInstance.StartCoroutine(DoUnload(inURL, forceUnload, unloadLoadedObjects, unloadDependencies));
        }

        public static void UnloadResourceBundle()
        {
            if (mResourceBundle != null)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Trying to unload resource bundle \"{ResourcesBundleName}\"", XDebug.Mask.Resources);
                Unload(ResourcesBundleName, forceUnload: true);
            }
        }
        /// <summary>
        /// All assets are unloaded when a new scene is being loaded. If you wish to retain any asset pass the asset name here.
        /// Returns true if successfully marked else returns false
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>true if successfully marked else returns false</returns>
        public static bool MarkImmortal(string inURL)
        {
            if (string.IsNullOrEmpty(inURL))
            {
                XDebug.LogWarning("Trying to immortalize an empty/null path", XDebug.Mask.Resources);
                return false;
            }
            string rootAsset = GetRootAsset(inURL);
            //Search in loading asset List
            Resource res = GetLoadingResource(rootAsset);
            //If its not in loading assets list, search in loaded assets list.
            if (res == null)
                res = GetResource(rootAsset);

            if (res != null)
            {
                res.mImmortal = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the Resouce object for the given url.
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>Resource object if found, else null</returns>
        public static Resource GetResource(string inURL)
        {
            return mResources.Find(e =>
            {
                if (e == null)
                    return false;
                if (!e.mBundleName.IsNullOrEmpty())
                    return e.mBundleName.Equals(inURL, StringComparison.OrdinalIgnoreCase);
                else if (!e.mAssetName.IsNullOrEmpty())
                    return e.mAssetName.Equals(inURL, StringComparison.OrdinalIgnoreCase);
                return false;
            });
        }

        /// <summary>
        /// Returns the Resouce object for the given url which is still being loaded.
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>Resource object if found, else null</returns>
        public static Resource GetLoadingResource(string inURL)
        {
            return mResourceLoadingList.Find(e =>
            {
                if (e == null)
                    return false;
                if (!e.mBundleName.IsNullOrEmpty())
                    return e.mBundleName.Equals(inURL, StringComparison.OrdinalIgnoreCase);
                else if (!e.mAssetName.IsNullOrEmpty())
                    return e.mAssetName.Equals(inURL, StringComparison.OrdinalIgnoreCase);
                return false;
            });
        }

        /// <summary>
        /// Unloads the AssetBundle from memory keeping all the instatiated objects.
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>true if released else false</returns>
        public static bool ReleaseBundleData(string inURL)
        {
            Resource res = GetResource(inURL);
            if (res != null)
            {
                if (res.mAssetBundleHandler != null)
                {
                    if (res.mAssetBundleHandler != null)
                        res.mAssetBundleHandler.assetBundle.Unload(false);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Load a file directly by passing absolute path. The other Load function takes relative path.
        /// </summary>
        /// <param name="path">This is an absolute path such as https</param>
        /// <param name="inEvent">Callback to be triggered on loadding the asset</param>
        public static void LoadURL(string inPath, ResEvent inEvent, ResourceType inResourceType, object inUserData = null, bool dontDestroyOnLoad = false, bool downloadOnly = false)
        {
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"LoadURL : {inPath}", XDebug.Mask.Resources);
            if (!string.IsNullOrEmpty(inPath))
            {
                //Altaf: I saw a random exception here, so removing all null or empty url objects from the list.
                mResources.RemoveAll(e => (e == null));
                Resource res = GetResource(inPath);
                if (res != null)
                {
                    //Resource already loaded, trigger loaded event.
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log("Resource already loaded, trigger loaded event", XDebug.Mask.Resources);

                    res.AddAsset(inPath, null, inUserData, inEvent, inResourceType);
                    res.OnResourceLoaded();
                }
                else
                {
                    //This resource is not yet loaded, see if it is in the loading list.
                    res = GetLoadingResource(inPath);
                    if (res != null)
                    {
                        //The resource is still being loaded, add the event handler.
                        if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("The resource is still being loaded, add the event handler.", XDebug.Mask.Resources);
                        res.AddAsset(inPath, null, inUserData, inEvent, inResourceType);
                    }
                    else
                    {
                        //The resource is not loaded yet, add it to the loading list.
                        if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("The resource is not loaded yet, add it to the loading list.", XDebug.Mask.Resources);
                        Resource rs = new Resource(inPath, inEvent, inUserData, dontDestroyOnLoad, inResourceType, downloadOnly);
                        rs.AddAsset(inPath, null, inUserData, inEvent, inResourceType);
                        mResourceLoadingList.Add(rs);
                    }
                }
            }
            else
            {
                XDebug.LogWarning("Trying to load a empty/null url.");
                if (inEvent != null)
                    inEvent(ResourceEvent.ERROR, inPath, null, inUserData);
            }
        }

        /// <summary>
        /// Dump all the data of loaded assets. To be used for debugging purpose only.
        /// </summary>
        public static void DumpLoadedAssetInfo()
        {
#if UNITY_EDITOR
            List<string> resources = mResources.ConvertAll(e => $"{e.mBundleName}:{e.pRefCount}");
            File.WriteAllLines("LoadedAssetBundles.txt", resources);
#endif
        }

        /// <summary>
        /// Dump all the data to the passed stream. To be used for debugging purpose only.
        /// </summary>
        /// <param name="fout"></param>
        /// <param name="verbose">should the log be verbose?</param>
        public static void Dump(StreamWriter fout, bool verbose)
        {
            string assetNames = null;
            if (mResources.Count > 0)
            {
                assetNames = ("==========Loaded assets start===============\n");
                foreach (Resource res in mResources)
                {
                    assetNames += res.mAssetName + ", " + res.pRefCount + "\n";
#if UNITY_EDITOR
                    if (verbose)
                        assetNames += res.pStackTrace + "\n";
#endif
                }
                assetNames += ("==========Loaded assets end================");
                fout.WriteLine(assetNames);
            }
            else
                fout.WriteLine("==========No Assets Loaded assets================");

            if (mResourceLoadingList.Count > 0)
            {
                assetNames = ("==========Loading assets start===============\n");
                foreach (Resource res in mResourceLoadingList)
                    assetNames += res.mAssetName + ", " + res.pRefCount + "\n";
                assetNames += ("==========Loading assets end================");
                fout.WriteLine(assetNames);
            }
            else
                fout.WriteLine("==========No Assets waiting to load================");
        }

        /// <summary>
        /// Call this function to stop all loads.
        /// </summary>
        public static void KillAllLoads()
        {
            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log("========== KillAllLoads ================", XDebug.Mask.Resources);

            mResourceLoadingList.RemoveAll(delegate (Resource res)
            {
                if (!res.mImmortal)
                {
                    if (res.mObject != null)
                    {
                        res.Reset();
                        res = null;
                        return true;
                    }
                }
                return false;
            });
            //WebRequestProcessor.KillAll();
            UtBehaviour.StopAll();
        }        /// <summary>

        /// Kills the loading of given url.
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>true if loading killed/stopped else false</returns>
        public static bool Kill(string inURL)
        {
            if (string.IsNullOrEmpty(inURL))
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.LogWarning("Trying to kill an empty/null path", XDebug.Mask.Resources);
                return false;
            }
            string rootAsset = GetRootAsset(inURL);
            Resource res = GetLoadingResource(rootAsset);
            if (res != null)
            {
                mResourceLoadingList.Remove(res);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns the list of assets that are shipped with the app
        /// </summary>
        /// <returns>Dictionary with key being assetname & value being its hash</returns>
        public static Dictionary<string, string> GetDefaultVersionList()
        {
            TextAsset data = LoadFromResources(BundledAssetsAssetName) as TextAsset;
            if (data != null && !string.IsNullOrEmpty(data.text))
                return data.text.FromJson<Dictionary<string, string>>();
            return null;
        }

        /// <summary>
        /// Static constructor. Being used to intialise ResourceManager`s static data
        /// </summary>
        static ResourceManager()
        {
            if (UnityEngine.PlayerPrefs.HasKey("CachedData"))
                mCache = UnityEngine.PlayerPrefs.GetString("CachedData").FromJson<Dictionary<string, string>>();
        }

        /// <summary>
        /// This function loads the manifest file for asset bundles. This file should be loaded before any call to loading AssetBundle is done.
        /// This function must be called before loading any asset. Wait for ResourceManager to be ready before loading any asset. Register a callback from ResourceManager::RegisterOnReadyCallback to know when ResourceManager is ready
        /// </summary>
        /// <returns></returns>
        public static void Init(Action inCallback, InitMode initMode = InitMode.Default)
        {
            //Debug.Log($"LTS: ResourceManager Init: {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
            FrameworkEventManager.LogEvent("unity_resourcemanager_init_start");

            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"ResourceManager Inited in {initMode} mode : {Time.realtimeSinceStartup}", XDebug.Mask.Resources);
            pInitMode = initMode;
            //First unsubscribe
            SceneManager.sceneLoaded -= OnSceneLoaded;
            //Subscibe again. This is to avoid multipe callbacks
            SceneManager.sceneLoaded += OnSceneLoaded;
            SetVariant(RemoteSettings.GetInt(RemoteKeys.MinMemory, 1000));
            AsyncLoader.Init();
            try
            {
                PlayerPrefs.SetString("AssetCachedVersion", ProductSettings.GetProductVersion());

                if (PlayerPrefs.HasKey("CachedData"))
                    mCache = PlayerPrefs.GetString("CachedData").FromJson<Dictionary<string, string>>();
                else if (mCache == null)
                    mCache = new Dictionary<string, string>();
            }
            catch (SystemException e)
            {
                XDebug.LogError("Failed to deserialize cached asset data : Error : " + e.Message, XDebug.Mask.Resources);
                mCache = new Dictionary<string, string>();
            }
            if (inCallback != null)
                OnReady += inCallback;
            //We do not unregister the delegate as we want the UpdateManager to be able to set the updated manifest
            if (mAssetBundleManifest == null)
            {
                //AsyncLoader.OnVersionListLoadedEvent = OnVersionListLoaded;
                AsyncLoader.LoadManifest(true);
                if (pInitMode != InitMode.Default)
                {
                    OnResourceManagerReady();
                }
            }
            else if (mAssetVersions == null)
            {
                AsyncLoader.LoadVersionList(OnVersionListLoaded);
            }
            else if (mResourceBundle == null)
            {
                LoadResourceBundle();
            }
            else
                OnResourceManagerReady();
        }


        /// <summary>
        /// Callback triggered by AsyncLoader when version_list.json file is loaded.
        /// </summary>
        /// <param name="assetVersions"></param>
        private static void OnVersionListLoaded(Dictionary<string, AssetData> assetVersions)
        {
            if (assetVersions == null)
                AsyncLoader.LoadVersionList(OnVersionListLoaded, true);
            else
            {
                FrameworkEventManager.LogEvent("unity_loading_version_list_completed");

                mShippedAssets = GetDefaultVersionList();
                mAssetVersions = assetVersions;
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log("Loading Resources bundle.", XDebug.Mask.Resources);
                if (mResourceBundle == null)
                {
                    FrameworkEventManager.LogEvent("unity_loading_resource_bundle_started");
                    Load(ResourcesBundleName, OnResourceBundleLoaded, ResourceType.AssetBundle);
                }
                else
                    OnResourceManagerReady();
            }
        }

        private static void LoadResourceBundle()
        {
            Load(ResourcesBundleName, OnResourceBundleLoaded, ResourceType.AssetBundle);
        }

        /// <summary>
        /// Callback from unity when scene is loaded
        /// </summary>
        /// <param name="scene">Scene object</param>
        /// <param name="loadSceneMode"></param>
        private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            FrameworkEventManager.LogEvent("unity_load_scene_completed " + scene.name);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (scene.name == mCurrentScene)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"OnScene Loaded : {scene.name}, {Time.realtimeSinceStartup}", XDebug.Mask.Resources);
                UtBehaviour.pInstance.StartCoroutine(DelayedLoadEvent(0.25f));
            }
        }

        /// <summary>
        /// Trigger scene loaded event after the given delay (in sec)
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <returns></returns>
        public static IEnumerator DelayedLoadEvent(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnSceneLoaded();
        }


        /// <summary>
        /// Callback triggered from AsyncLoader after manifets is loaded.
        /// </summary>
        /// <param name="manifest"></param>
        public static void OnManifestLoaded(AssetBundleManifest manifest)
        {
            if (manifest == null)
                AsyncLoader.LoadManifest(true);
            else
            {
                FrameworkEventManager.LogEvent("unity_loading_manifest_complete");

                mAssetBundleManifest = manifest;
                mBundlesWithVariant = mAssetBundleManifest.GetAllAssetBundlesWithVariant();
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"BundlesWithVariant : {mBundlesWithVariant.Length}, Variants: {mBundlesWithVariant.Printable()}", XDebug.Mask.Resources);
                AsyncLoader.LoadVersionList(OnVersionListLoaded);
            }
        }

        /// <summary>
        /// Unload unsued assets.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IEnumerator DelayedUnloadUnusedAssets(float delay = 1f)
        {
            yield return new WaitForSeconds(delay);
            XDebug.Log("Unloading Unused assets, Game will freeze for a while", XDebug.Mask.Resources);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        /// <summary>
        /// Unload unused assets. Do not call this function frequently as its a expensive operation
        /// </summary>
        public static void UnloadUnusedAssets()
        {
            if (UtBehaviour.pInstance != null)
                UtBehaviour.pInstance.StartCoroutine(DelayedUnloadUnusedAssets());
        }

        /// <summary>
        /// Retrieves a list of assets by module name.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <returns>A list of assets associated with the specified module.</returns>
        public static List<AssetData> GetAssetsByModule(string moduleName)
        {
            if (moduleName.IsNullOrEmpty())
            {
                XDebug.LogWarning("Trying to get assets by an empty/null module name", XDebug.Mask.Resources);
                return null;
            }
            List<AssetData> assets = new List<AssetData>();
            foreach (KeyValuePair<string, AssetData> kvp in mAssetVersions)
            {
                if (kvp.Value.module == moduleName)
                    assets.Add(kvp.Value);
            }
            return assets;
        }

        #endregion Public Methods of ResourceManager

        #region Private Methods of ResourceManager

        /// <summary>
        /// Clear cached asset history.
        /// </summary>
        private static void ClearCache()
        {
            UnityEngine.PlayerPrefs.DeleteKey("CachedData");
            UnityEngine.PlayerPrefs.Save();
        }

        /// <summary>
        /// Returns the asset name from th egiven url. Return the last asset name from the path.
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns></returns>
        internal static string GetAssetName(string inURL)
        {
            string[] paths = inURL.Split('/');
            return paths[paths.Length - 1];
        }

        /// <summary>
        /// Return the AssetBundle name from the given url
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns></returns>
        public static string GetBundleName(string inURL)
        {
            if (string.IsNullOrEmpty(inURL))
                return "";
            string[] paths = inURL.Split('/');
            if (paths.Length > 0)
                return paths[0];
            return string.Empty;
        }

        /// <summary>
        /// Splits the given path in bundle name & asset name
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>tuple of bundleName & assetName</returns>
        /// #example input -=> cards/cardAtlas, returns (cards, cardAtlas)
        public static (string bundleName, string assetName) SplitAssetName(string inURL)
        {
            string bundleName = null, assetName = null;
            if (!string.IsNullOrEmpty(inURL))
            {
                string[] paths = inURL.Split('/');
                if (paths.Length > 0)
                    bundleName = paths[0];
                if (paths.Length >= 2)
                    assetName = paths[1];
            }
            return (bundleName, assetName);
        }

        /// <summary>
        /// Returns name of AssetBunde from a remote url such as http://some-remote-server/
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns></returns>
        internal static string GetBundleNameFromRemoteUrl(string inURL)
        {
            if (string.IsNullOrEmpty(inURL))
                return "";
            string[] paths = inURL.Split('/');
            if (paths.Length >= 2)
                return paths[paths.Length - 2];
            return paths[0];
        }

        /// <summary>
        /// Returns root of the given path. Ex: if we pass loading/PfUiLoader, it returns loading
        ///  Mostly used in case of AssetBindle loading
        /// </summary>
        /// <param name="inURL"></param>
        /// <returns>Name of asset bundle name</returns>
        internal static string GetRootAsset(string inURL)
        {
            if (inURL.IsNullOrEmpty())
                return null;
            string[] paths = inURL.Split('/');
            if (paths.IsNullOrEmpty())
                return null;
            if (inURL.Contains(".") && paths[0].LastIndexOf(".") == -1)
                return inURL;
            return paths[0];
        }

        /// <summary>
        /// Returns all dependencies for the given asset. Used for AssetBundles only
        /// </summary>
        /// <param name="inAssetBundle"></param>
        /// <returns>Array of assets that the currentb asset dependends on</returns>
        internal static string[] GetAllDependencies(string inAssetBundle)
        {
            if (mAssetBundleManifest != null)
                return mAssetBundleManifest.GetAllDependencies(inAssetBundle);
            else
                return new string[0];
        }

        /// <summary>
        /// Callback for Resources asset bundle being loaded
        /// </summary>
        /// <param name="inEvent"></param>
        /// <param name="inURL"></param>
        /// <param name="inObject"></param>
        /// <param name="inUserData"></param>
        private static void OnResourceBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            bool sendEvent = false;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                FrameworkEventManager.LogEvent("unity_loading_resource_bundle_completed");

                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Resource bundle loaded {inURL}", XDebug.Mask.Resources);
                mResourceBundle = inObject as AssetBundle;
                sendEvent = true;
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                FrameworkEventManager.LogEvent("unity_loading_resource_bundle_failed");

                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.LogError($"Failed to load Resource bundle : {inURL}", XDebug.Mask.Resources);
                sendEvent = true;
            }

            if (sendEvent)
            {
                OnResourceManagerReady();
            }
        }

        private static void OnResourceManagerReady()
        {
            //Debug.Log($"LTS: ResourceManager Ready: {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            if (XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"ResourceManager is Ready. {Time.realtimeSinceStartup}", XDebug.Mask.Resources);
            OnReady?.Invoke();
            OnReady = null;
        }

        /// <summary>
        /// Checks is the scene is loaded as asset bundle
        /// </summary>
        /// <param name="inLevelName"></param>
        /// <returns>true if scene is marked as bundle else false</returns>
        private static bool IsBundledLevel(string inLevelName)
        {
            if (!ProductSettings.pInstance._LoadSceneBundlesInEditor && PlatformUtilities.IsEditor())
                return false;
            if (Array.IndexOf(ProductSettings.pInstance._NonBundledLevels, inLevelName) >= 0)
                return false;
            return true;
        }

        /// <summary>
        /// Load the given scene asynchronously
        /// </summary>
        /// <param name="inLevelName"></param>
        private static void LoadSceneAsync(string inLevelName)
        {
            //Debug.Log($"LTS: LoadSceneAsync {inLevelName} : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            if (IsBundledLevel(inLevelName))
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Loading scene as a bundle : {inLevelName}", XDebug.Mask.Resources);
                string levelPath = inLevelName;
                Load(levelPath, OnStreamedLevelLoaded, ResourceType.AssetBundle);
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Loading scene directly : {inLevelName}", XDebug.Mask.Resources);
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadSceneAsync(inLevelName);
                //OnLevelLoaded();
            }
        }

        /// <summary>
        /// Callback of scene being loaded
        /// </summary>
        /// <param name="inEvent"></param>
        /// <param name="inURL"></param>
        /// <param name="inObject"></param>
        /// <param name="inUserData"></param>
        private static void OnStreamedLevelLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                SceneManager.LoadScene(mCurrentScene);
                //Debug.Log($"LTS: OnStreamedLevelLoaded {inURL} : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");
                UtBehaviour.pInstance.StartCoroutine(DelayedLoadEvent(Time.deltaTime));

                //OnLevelLoaded();

                /*AsyncOperation async = null;
                async = SceneManager.LoadSceneAsync(mCurrentLevel);
                if (async != null)
                {
                    OnLevelLoaded();
                    UtBehaviour.RunCoroutine(WaitAndUnload(async));
                }*/
            }
            /* else if (inEvent == ResourceEvent.PROGRESS)
             {
                 if(mUiLoadingScreen != null)
                     mUiLoadingScreen.SetProgress((float)inObject);
             }*/
        }

        /// <summary>
        /// Unloads the current scene bundle
        /// </summary>
        /// <param name="aync"></param>
        /// <param name="aBundle"></param>
        /// <returns></returns>
       /* private static IEnumerator WaitAndUnload(AsyncOperation aync)
        {
            while (!aync.isDone)
                yield return null;

            XDebug.Log("Scene bundle unloaded.", XDebug.Mask.Resources);
            Unload(pCurrentLevel, false, false);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }*/

        /// <summary>
        /// This is called after the scene loading completes.
        /// </summary>
        private static void OnSceneLoaded()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            //Debug.Log($"LTS: OnSceneLoaded {mCurrentScene} : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            mIsSceneLoading = false;
            if (OnSceneLoadedEvent != null)
                OnSceneLoadedEvent(pCurrentScene);
        }

        #endregion Private Methods of ResourceManager
    }
}