using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XcelerateGames.AssetLoading
{
    public class AsyncLoader : MonoBehaviour
    {
        private static AsyncLoader mInstance = null;

        public static AsyncLoader pInstance { get { return mInstance; } }
        public static string VersionListHash = null;

#if UNITY_EDITOR
        private float RandomTime => UnityEngine.Random.Range(0f, 3f);

        private static int mSimulateLoadingDelay = -1;
        private const string mSimulateLoadingDelayKey = "SimulateLoadingDelayKey";

        public static bool pSimulateLoadingDelay
        {
            get
            {
                if (mSimulateLoadingDelay == -1)
                    mSimulateLoadingDelay = EditorPrefs.GetBool(mSimulateLoadingDelayKey, false) ? 1 : 0;

                return mSimulateLoadingDelay != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != mSimulateLoadingDelay)
                {
                    mSimulateLoadingDelay = newValue;
                    EditorPrefs.SetBool(mSimulateLoadingDelayKey, value);
                }
            }
        }
#endif

        private void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                gameObject.isStatic = true;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                XDebug.LogError("Duplicate instance found, deleting it : " + name);
                Destroy(gameObject);
            }
        }

        public IEnumerator LoadAssetBundle(string url, Action<UnityWebRequest, float, bool> callback, string hash)
        {
#if UNITY_EDITOR
            if (pSimulateLoadingDelay)
            {
                float waitTime = RandomTime;
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    Debug.Log($"Delaying loading of {url} for {waitTime} seconds");
                yield return new WaitForSeconds(waitTime);
            }
#endif
            Hash128 abHash = Hash128.Parse(hash);
            if (XDebug.CanLog(XDebug.Mask.Resources))
                UnityEngine.Debug.LogFormat("Loading file {0}, Hash {1}, IsCached {2}", url, hash, Caching.IsVersionCached(url, abHash));

#if UNITY_EDITOR && !OTA_BUILD
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, abHash, 0);
#endif
            request.disposeDownloadHandlerOnDispose = false;
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources)) Debug.Log("Progress : " + url + "->" + asyncOperation.progress);
                callback(request, asyncOperation.progress, false);
                yield return request;
            }
            if (XDebug.CanLog(XDebug.Mask.Resources)) Debug.Log("Completed : " + url + ", Calling callback");

            callback(request, 1f, true);
            callback = null;
        }

        public IEnumerator Load(string url, Action<UnityWebRequest, float, bool> callback, ResourceManager.ResourceType resType)
        {
#if UNITY_EDITOR
            if (pSimulateLoadingDelay)
            {
                float waitTime = RandomTime;
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    Debug.Log($"Delaying loading of {url} for {waitTime} seconds");
                yield return new WaitForSeconds(waitTime);
            }
#endif

            UnityWebRequest request = UnityWebRequest.Get(url);
            if (resType == ResourceManager.ResourceType.Texture)
                request.downloadHandler = new DownloadHandlerTexture(false);
            else if (resType == ResourceManager.ResourceType.AudioClip)
            {
                DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(url, Utilities.GetAudioType(url));
                downloadHandler.streamAudio = true;
                request.downloadHandler = downloadHandler;
            }

            request.disposeDownloadHandlerOnDispose = false;
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources)) Debug.Log("Progress : " + url + "->" + asyncOperation.progress);
                callback(request, asyncOperation.progress, false);
                yield return request;
            }

            callback(request, 1f, true);
            callback = null;
        }

        private IEnumerator LoadManifest(string url)
        {
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0))
            {
                request.timeout = RemoteSettings.GetInt(RemoteKeys.ManifestTimeOut, 20);
                DownloadHandlerAssetBundle handler = request.downloadHandler as DownloadHandlerAssetBundle;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    XDebug.LogException($"Failed to load AssetBundleManifest : {url}");
                    ResourceManager.OnManifestLoaded(null);
                }
                else
                {
                    //Load AssetBundleManifest object.
                    AssetBundleManifest manifest = handler.assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

#if !UNITY_WEBGL
                    if (request.url.StartsWith("http"))
                    {
                        //We loaded the Mnifest file from CDN, cache it for offline/later use.
                        FileUtilities.WriteToFile(PlatformUtilities.GetAssetFolderPath(), request.downloadHandler.data);
                    }
#endif

                    //Unload the bundle, but keep the Manifest in memory.
                    handler.assetBundle.Unload(false);
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        Debug.Log("AssetBundleManifest loaded successfully");
                    ResourceManager.OnManifestLoaded(manifest);
                }
            }
        }

        private IEnumerator LoadVersionList(string url, Action<Dictionary<string, AssetData>> callback, int timeout)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeout;
                yield return request.SendWebRequest();
                XDebug.Log($"VersionList loaded result: {request.result}", XDebug.Mask.Resources, XDebug.Priority.High);

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    if (request.url.StartsWith("http"))
                    {
                        XDebug.LogError("Failed to load " + url + ", Trying to load local file now", XDebug.Mask.Resources);
                        callback(null);
                    }
#if UNITY_IOS
                    else if(request.url.Contains("/Raw"))
#else
                    else if (request.url.Contains("file:/"))
#endif
                    {
                        //If this is null, ResourceManager wont be ready, just set some data
                        XDebug.LogError("Failed to load " + url + ", Just setting empty data", XDebug.Mask.Resources);
                        callback(new Dictionary<string, AssetData>());
                    }
                    else
                    {
                        XDebug.LogException("What to do here?");
                    }
                }
                else
                {
                    try
                    {
                        if (XDebug.CanLog(XDebug.Mask.Resources))
                            Debug.Log("VersionList loaded successfully");
                        if (XDebug.CanLog(XDebug.Mask.Resources)) Debug.Log("Version List \n" + request.downloadHandler.text);
                        callback(request.downloadHandler.text.FromJson<Dictionary<string, AssetData>>());
                    }
                    catch (Exception ex)
                    {
                        string msg = "Failed to load : " + request.url + "\n Message : " + ex.Message;
                        XDebug.LogException(msg);
                        LoadVersionList(callback, true);
                    }
                }
            }
        }

        #region Static Methods

        public static void Init()
        {
            if (mInstance == null)
                new GameObject("AsyncLoader").AddComponent<AsyncLoader>();
        }

        /// <summary>
        /// This funation has to be called before call to any bundle loading is done.
        /// </summary>
        /// <returns></returns>
        public static void LoadManifest(bool forceLocal = false)
        {
            FrameworkEventManager.LogEvent("unity_loading_manifest_started");

            string assetBundleManifestPath = null;
            if (!forceLocal && ConnectivityMonitor.pIsInternetAvailable && !PlatformUtilities.IsLocalBuild())
                assetBundleManifestPath = PlatformUtilities.GetCDNPath(PlatformUtilities.GetAssetFolderPath());
            else
            {
                assetBundleManifestPath = PlatformUtilities.GetPersistentDataPath() + Path.DirectorySeparatorChar + PlatformUtilities.GetAssetFolderPath();
                if (PlatformUtilities.IsLocalBuild() || !File.Exists(assetBundleManifestPath))
                {
                    //Get path to our AssetBundleManifest asset bundle file.
                    assetBundleManifestPath = PlatformUtilities.GetPlatformAssetPath(true) + "/" + PlatformUtilities.GetAssetFolderPath();
                }
                else//Use the cached version.
                    assetBundleManifestPath = "file:///" + assetBundleManifestPath;
            }
            if (XDebug.CanLog(XDebug.Mask.Resources))
                Debug.Log($"Loading AssetBundleManifest from : {assetBundleManifestPath}");
            //Load the bundle now.
            mInstance.StartCoroutine(mInstance.LoadManifest(assetBundleManifestPath));
        }

        public static void LoadVersionList(Action<Dictionary<string, AssetData>> callback, bool forceLocal = false)
        {
            FrameworkEventManager.LogEvent("unity_loading_version_list_started");

            string assetBundleManifestPath = null;
            if (VersionListHash.IsNullOrEmpty())
            {
                XDebug.Log("Version list hash is null or empty, forcing local asset load", inPriority: XDebug.Priority.High);
                forceLocal = true;
            }
            if (!forceLocal && ConnectivityMonitor.pIsInternetAvailable && !PlatformUtilities.IsLocalBuild())
            {
                XDebug.Log("Version List Hash : " + VersionListHash, inPriority: XDebug.Priority.High);
                assetBundleManifestPath = PlatformUtilities.GetCDNPath(ResourceManager.mAssetVersionListFileName) + "/" + VersionListHash;
            }
            else
            {
                assetBundleManifestPath = ResourceManager.mAssetVersionListFileName;
                if (UnityEngine.PlayerPrefs.HasKey(assetBundleManifestPath))
                {
                    try
                    {
                        string data = UnityEngine.PlayerPrefs.GetString(assetBundleManifestPath);
                        callback.Invoke(data.FromJson<Dictionary<string, AssetData>>());
                    }
                    catch (Exception)
                    {
                        UnityEngine.PlayerPrefs.DeleteKey(ResourceManager.mAssetVersionListFileName);
                        UnityEngine.PlayerPrefs.Save();
                        UnityEngine.Debug.LogError("Version list was cached in PlayerPrefs, May be it is currupted, Deleting & loading from shipped file");
                        callback?.Invoke(null);
                    }
                }
                else
                    assetBundleManifestPath = PlatformUtilities.GetPlatformAssetPath(true) + "/" + assetBundleManifestPath;
            }
            if (XDebug.CanLog(XDebug.Mask.Resources))
                Debug.Log($"Loading version_list.json from : {assetBundleManifestPath}");
            //Load the bundle now.
            mInstance.StartCoroutine(mInstance.LoadVersionList(assetBundleManifestPath, callback, 3));
        }
    }

    #endregion Static Methods
}
