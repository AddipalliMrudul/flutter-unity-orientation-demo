using UnityEngine;
using System;
using System.Collections.Generic;
using ResEvent = System.Action<XcelerateGames.AssetLoading.ResourceEvent, string, object, object>;
using UnityEngine.Networking;

namespace XcelerateGames.AssetLoading
{
    /// <summary>
    /// Every asset in the game that is loaded is a Resource. This class manages all operations related to it
    /// </summary>
    public class Resource
    {
        #region Member Variables

        public List<AssetInfo> mAssets = new List<AssetInfo>();         /**<Assets that are loaded from the asset bundle */
        public string mAssetName = null;                                /**<Name of the asset in asset bundle */
        public string mBundleName = null;                               /**<Name of the asset bundle */
        private int mRefCount = 0;                                      /**<Reference count of this asset */
        public ResourceManager.ResourceType mResourceType = ResourceManager.ResourceType.NONE;  /**<Type of resource */
        public bool mLoadDependency = false;                            /**<Weather to load dependent assets? */
        public bool mDownloadOnly = false;                              /**<After loading asset should we extract from AssetBundle? */
        public bool mImmortal = false;                                  /**<If set to true, asset will not be unloaded on scene transition */
        public bool mIsRemoteAsset = false;                             /**<Is asset being loaded locally or from remote server? */

        public UnityWebRequest mObject = null;                          /**<UnityWebRequest object used for loading the asset */
        public DownloadHandlerAssetBundle mAssetBundleHandler = null;   /**<DownloadHandlerAssetBundle to get AssetBundle data */
        public DownloadHandlerAudioClip mAudioClipHandler = null;       /**<DownloadHandlerAudioClip to get Audio clip data*/
        public DownloadHandler mDownloadHandler = null;                 /**<DownloadHandler handler*/
        public DownloadHandlerTexture mTextureHandler = null;           /**<DownloadHandlerTexture texture data handler*/

        public int mRetryCount = 2;                                     /**<How many times to retry if asset fails to load */
        private int mNumDependencies = 0;                               /**<Num of dependencies of this Asset */
        private List<Resource> mDependencies = null;                    /**<List of dependent assets */

        /// <summary>
        /// Reference count of the asset. Reference count increases everytime the asset is loaded directly or indirectly as a dependency.
        /// Reference count is reduced when the asset is unloaded. The asset is unloaded from memory when reference count is zero.
        /// </summary>
        public int pRefCount
        {
            get { return mRefCount; }
            set
            {
                bool removing = (mRefCount > value);
                mRefCount = value;
                AddSourceInfo(removing);
            }
        }

        public string Path()
        {
            if (mBundleName.IsNullOrEmpty())
                return mAssetName;
            if (mAssetName.IsNullOrEmpty())
                return mBundleName;
            return $"{mBundleName}/{mAssetName}";
        }

#if UNITY_EDITOR

        /// <summary>
        /// Stack trace. Works only on IDE for debugging purpose only 
        /// </summary>
        public string pStackTrace
        {
            get;
            private set;
        }

#endif

        #endregion Member Variables

        #region Public Methods
        /// <summary>
        /// String representaton of object state
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"url:{Path()}, ref count:{mRefCount}";
        }

        /// <summary>
        /// Constructor used while loading AssetBundle or an asset from AssetBundle
        /// </summary>
        /// <param name="bundleName">Name of the asset bundle. Should never be null</param>
        /// <param name="assetName">Name of the asset under asset bundle. Can be null</param>
        /// <param name="inEvent">Callback to be triggered when asset is loaded</param>
        /// <param name="inUserData">Any user data to be passed when asset loaded in the callback</param>
        /// <param name="loadDependency">Load all dependencies</param>
        /// <param name="dontDestroyOnLoad">If set to true, the asset bundle will not be unloaded on scene transition</param>
        /// <param name="inType">Type of resource, see ResourceManager.ResourceType</param>
        /// <param name="downloadOnly">If set to true, after loading the bundle, asset wont be extracted. Can be used for preloading</param>
        public Resource(string bundleName, string assetName, ResEvent inEvent, object inUserData, bool loadDependency, bool dontDestroyOnLoad, ResourceManager.ResourceType inType, bool downloadOnly)
        {
            mAssetName = assetName;
            mBundleName = bundleName;
            mLoadDependency = loadDependency;
            mResourceType = inType;
            mImmortal = dontDestroyOnLoad;
            mDownloadOnly = downloadOnly;
            Load();
        }

        /// <summary>
        /// Constructor used while loading asset with http(s) url
        /// </summary>
        /// <param name="inPath">URL of the asset to be loaded</param>
        /// <param name="inEvent">Callback to be triggered when asset is loaded</param>
        /// <param name="inUserData">Any user data to be passed when asset loaded in the callback</param>
        /// <param name="loadDependency">Load all dependencies</param>
        /// <param name="dontDestroyOnLoad">If set to true, the asset bundle will not be unloaded on scene transition</param>
        /// <param name="inType">Type of resource, see ResourceManager.ResourceType</param>
        /// <param name="downloadOnly">If set to true, after loading the bundle, asset wont be extracted. Can be used for preloading</param>
        public Resource(string inPath, ResEvent inEvent, object inUserData, bool dontDestroyOnLoad, ResourceManager.ResourceType inType, bool downloadOnly)
        {
            mAssetName = inPath;
            if (inType == ResourceManager.ResourceType.AssetBundle || inType == ResourceManager.ResourceType.Object)
                mBundleName = ResourceManager.GetBundleNameFromRemoteUrl(inPath);
            mImmortal = dontDestroyOnLoad;
            mDownloadOnly = downloadOnly;
            mResourceType = inType;
            LoadURL();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Add tracking information for debugging purpose only. Works only on IDE
        /// </summary>
        /// <param name="removeReference"></param>
        public void AddSourceInfo(bool removeReference)
        {
            pStackTrace += string.Format("\n{0} Ref : {1}, Action : {2} \n{3}", mAssetName, mRefCount, removeReference ? "Removing" : "Adding", StackTraceUtility.ExtractStackTrace());
        }

#else
            public void AddSourceInfo(bool removeReference)
            {
            }
#endif
        /// <summary>
        /// Add asset to the list
        /// </summary>
        /// <param name="bundleName">Name of the asset bundle. Should never be null</param>
        /// <param name="assetName">Name of the asset under asset bundle. Can be null</param>
        /// <param name="inUserData">Any user data to be passed when asset loaded in the callback</param>
        /// <param name="inEvent">Callback to be triggered when asset is loaded</param>
        /// <param name="inType">Type of resource, see ResourceManager.ResourceType</param>
        public void AddAsset(string bundleName, string assetName, object inUserData, ResEvent inEvent, ResourceManager.ResourceType inType)
        {
            AssetInfo assetInfo = new AssetInfo(bundleName, assetName, inUserData, inEvent, inType);
            pRefCount++;
            if (inType == ResourceManager.ResourceType.AssetBundle || inType == ResourceManager.ResourceType.Object)
            {
                string[] deps = ResourceManager.GetAllDependencies(ResourceManager.GetBundleName(bundleName));
                for (int i = 0; i < deps.Length; ++i)
                {
                    Resource res = ResourceManager.GetResource(deps[i]);
                    if (res != null)
                        res.pRefCount++;
                }
            }
            mAssets.Add(assetInfo);
        }

        /// <summary>
        /// Reset asset state
        /// </summary>
        public void Reset()
        {
            if(XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log($"Resetting Resource, Bundle Name:{mBundleName}, Asset Name: {mAssetName}", XDebug.Mask.Resources);
            mRefCount = 0;
            mAssetName = null;
            mObject = null;
            if (mAssetBundleHandler != null)
                mAssetBundleHandler.Dispose();
            mAssetBundleHandler = null;

            if (mDownloadHandler != null)
                mDownloadHandler.Dispose();
            mDownloadHandler = null;

            if (mTextureHandler != null)
                mTextureHandler.Dispose();
            mTextureHandler = null;
            mAssets.Clear();
        }

        /// <summary>
        /// Releases the resource if reference count is zero or if forceUnload is true
        /// </summary>
        /// <returns></returns>
        public bool Release(bool forceUnload, bool unloadLoadedObjects = true, bool unloadDependencies = true)
        {
            //Release the dependent resources as well.
            if (unloadDependencies && mDependencies != null)
            {
                int count = mDependencies.Count;
                for (int i = 0; i < count; ++i)
                {
                    Resource res = mDependencies[i];
                    if (res != null)
                        ResourceManager.Unload(res.mBundleName, forceUnload, unloadLoadedObjects);
                }
            }
            pRefCount--;
            if (forceUnload || mRefCount <= 0)
            {
                if (mResourceType == ResourceManager.ResourceType.AssetBundle || mResourceType == ResourceManager.ResourceType.Object)
                {
                    if (mAssetBundleHandler != null && mAssetBundleHandler.assetBundle != null)
                        mAssetBundleHandler.assetBundle.Unload(unloadLoadedObjects);
                }
                else if (mResourceType == ResourceManager.ResourceType.Texture)
                {
                    if (mTextureHandler != null)
                        UnityEngine.Object.Destroy(mTextureHandler.texture);
                }
                // clear all references
                Reset();
                return true;
            }
            if(XDebug.CanLog(XDebug.Mask.Resources | XDebug.Mask.UpdateManager))
                XDebug.Log($"Release : Could not release {Path()}, Ref count {mRefCount}", XDebug.Mask.Resources | XDebug.Mask.UpdateManager);

            return false;
        }

        /// <summary>
        /// This function is called when the resource is done loading.
        /// </summary>
        /// <param name="success"></param>
        public void OnResourceLoaded(UnityWebRequest request, float progress, bool complete)
        {
            if (complete && XDebug.CanLog(XDebug.Mask.Resources))
            {
                if (request != null)
                {
                    if(XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log($"Resources::OnResourceLoaded: Loaded {request.url}", XDebug.Mask.Resources);
                }
            }
#if HANDLE_EXCEPTIONS
            try
#endif //HANDLE_EXCEPTIONS

            {
                if (!complete)
                {
                    for (int i = 0; i < mAssets.Count; ++i)
                    {
                        AssetInfo assetInfo = mAssets[i];
                        if (assetInfo != null && assetInfo._Event != null)
                        {
                            (float progress, ulong downloadedBytes) data = (progress, request.downloadedBytes);
                            SendEvent(ResourceEvent.PROGRESS, assetInfo, data, false);
                        }
                    }

                    return;
                }
                if (request == null)
                {
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log("Resources::OnResourceLoaded: request is null, returning", XDebug.Mask.Resources);
                    return;
                }
                mObject = request;
                bool success = true;
#if UNITY_2020_2_OR_NEWER
                if (mObject.result == UnityWebRequest.Result.ConnectionError || mObject.result == UnityWebRequest.Result.ProtocolError)
#else
                if (mObject.isHttpError || mObject.isNetworkError)
#endif
                    success = false;
                if (mNumDependencies > 0)
                {
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.Log($"Resources::OnResourceLoaded: {request.url} dependencies remaining {mNumDependencies}, returning", XDebug.Mask.Resources);
                    return;
                }
                if (!ResourceManager.mResources.Contains(this) && success)
                {
                    ResourceManager.mResources.Add(this);
                }
                //Remove it from the loading list.
                ResourceManager.mResourceLoadingList.Remove(this);
                bool clearList = false;
                if (success)
                {
                    if (mResourceType == ResourceManager.ResourceType.AssetBundle || mResourceType == ResourceManager.ResourceType.Object)
                        mAssetBundleHandler = mObject.downloadHandler as DownloadHandlerAssetBundle;
                    else if (mResourceType == ResourceManager.ResourceType.Texture)
                        mTextureHandler = mObject.downloadHandler as DownloadHandlerTexture;
                    else if (mResourceType == ResourceManager.ResourceType.AudioClip)
                        mAudioClipHandler = mObject.downloadHandler as DownloadHandlerAudioClip;
                    else
                        mDownloadHandler = mObject.downloadHandler;
                }
                for (int i = 0; i < mAssets.Count; ++i)
                {
                    AssetInfo assetInfo = mAssets[i];
                    if (assetInfo != null && assetInfo._Event != null)
                    {
                        if (success)
                        {
                            if (!ResourceManager.mLoadedAssetsList.ContainsKey(assetInfo._BundleName))
                                ResourceManager.mLoadedAssetsList.Add(assetInfo._BundleName, null);
                            clearList = true;
                            if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("Sending COMPLETE event for " + Path() + " to " + assetInfo._Event.Target + "::" + assetInfo._Event.Method, XDebug.Mask.Resources);
                            if (assetInfo._Type == ResourceManager.ResourceType.Text)
                                assetInfo._Event.Invoke(ResourceEvent.COMPLETE, assetInfo.Path(), mDownloadHandler.text, assetInfo._UserData);
                            else if (assetInfo._Type == ResourceManager.ResourceType.AssetBundle || assetInfo._Type == ResourceManager.ResourceType.Object)
                            {
                                if (assetInfo._Type == ResourceManager.ResourceType.AssetBundle)
                                    SendEvent(ResourceEvent.COMPLETE, assetInfo, mDownloadOnly ? null : mAssetBundleHandler.assetBundle, true);
                                else if (assetInfo._Type == ResourceManager.ResourceType.Object)
                                    SendEvent(ResourceEvent.COMPLETE, assetInfo, mAssetBundleHandler.assetBundle.LoadAsset(assetInfo._AssetName), true);
                            }
                            else if (assetInfo._Type == ResourceManager.ResourceType.Texture)
                                SendEvent(ResourceEvent.COMPLETE, assetInfo, mTextureHandler.texture, true);
                            else if (assetInfo._Type == ResourceManager.ResourceType.Video || assetInfo._Type == ResourceManager.ResourceType.Pdf)
                                SendEvent(ResourceEvent.COMPLETE, assetInfo, mDownloadOnly ? null : mDownloadHandler.data, true);
                            else if (assetInfo._Type == ResourceManager.ResourceType.AudioClip)
                                SendEvent(ResourceEvent.COMPLETE, assetInfo, mDownloadOnly ? null : mAudioClipHandler.audioClip, true);
                        }
                        else
                        {
                            //Loading failed, reduce reference count.
                            mRefCount--;
                            mRetryCount--;
                            if (mRetryCount < 0)
                            {
                                if (mIsRemoteAsset)
                                {
                                    if (XDebug.CanLog(XDebug.Mask.Resources))
                                        XDebug.Log($"Failed to load remote asset {Path()}, forcing to load asset locally.", XDebug.Mask.Resources);

                                    //bool cacheCleared = Caching.ClearAllCachedVersions(mBundleName);
                                    //Debug.Log($"Cache for {mBundleName} cleared? {cacheCleared}, URL:{mURL}");
                                    ResourceManager.mResourceLoadingList.Remove(this);
                                    ResourceManager.mResources.Remove(this);

                                    ResourceManager.Load(assetInfo.Path(), assetInfo._Event, mResourceType, assetInfo._UserData);
                                }
                                else
                                {
                                    if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("Sending ERROR event for " + Path() + " to " + assetInfo._Event.Target + "::" + assetInfo._Event.Method + " for asset : " + assetInfo._AssetName, XDebug.Mask.Resources);
                                    XDebug.LogError("Failed to load asset : " + Path() + ", URL : " + mObject.url + ", Asset : " + assetInfo._AssetName + ", Error : " + mObject.error /*+ ", Headers : " + mObject.responseHeaders.ToJson()*/, XDebug.Mask.Resources);
                                    ResourceManager.mResourceLoadingList.Remove(this);
                                    SendEvent(ResourceEvent.ERROR, assetInfo, null, true);
                                }
                            }
                            else
                            {
                                if(!ResourceManager.mResourceLoadingList.Contains(this))
                                    ResourceManager.mResourceLoadingList.Add(this);
                                if(XDebug.CanLog(XDebug.Mask.Resources))
                                    XDebug.LogWarning("Warning! Failed to load " + Path() + ", Retrying", XDebug.Mask.Resources);
                                if (Path().StartsWith("http"))
                                    ResourceManager.LoadURL(Path(), assetInfo._Event, mResourceType, assetInfo._UserData);
                                else
                                    Load();
                            }
                        }
                    }
                }
                if (mRetryCount < 0 || clearList)
                    mAssets.Clear();
                if (mObject != null)
                    mObject.Dispose();
            }
#if HANDLE_EXCEPTIONS
            catch (Exception e)
            {
                XDebug.LogException($"Exception while loading asset: {e.Message}, URL: {mAssetName}, \nStackTrace: {e.StackTrace}, \nObject State: {ReflectionUtilities.GetInstanceValues(this)}");
            }
#endif //HANDLE_EXCEPTIONS
        }

        /// <summary>
        /// Called when the resource load is called when its already loaded.
        /// </summary>
        public void OnResourceLoaded()
        {
#if HANDLE_EXCEPTIONS
            try
#endif //HANDLE_EXCEPTIONS

            {
                bool clearList = false;
                for (int i = 0; i < mAssets.Count; ++i)
                {
                    AssetInfo assetInfo = mAssets[i];
                    if (assetInfo != null && assetInfo._Event != null)
                    {
                        clearList = true;
                        if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("Sending COMPLETE event for " + Path() + " to " + assetInfo._Event.Target + "::" + assetInfo._Event.Method, XDebug.Mask.Resources);
                        if (assetInfo._Type == ResourceManager.ResourceType.Text)
                        {
                            if (XDebug.CanLog(XDebug.Mask.Resources)) XDebug.Log("Contents of : " + Path() + "\n" + mDownloadHandler.text, XDebug.Mask.Resources);
                            assetInfo._Event.Invoke(ResourceEvent.COMPLETE, assetInfo.Path(), mDownloadHandler.text, assetInfo._UserData);
                        }
                        else if (assetInfo._Type == ResourceManager.ResourceType.AssetBundle || assetInfo._Type == ResourceManager.ResourceType.Object)
                        {
                            if (assetInfo._Type == ResourceManager.ResourceType.AssetBundle)
                                SendEvent(ResourceEvent.COMPLETE, assetInfo, mAssetBundleHandler.assetBundle, true);
                            else if (assetInfo._Type == ResourceManager.ResourceType.Object)
                                SendEvent(ResourceEvent.COMPLETE, assetInfo, mAssetBundleHandler.assetBundle.LoadAsset(assetInfo._AssetName), true);
                        }
                        else if (assetInfo._Type == ResourceManager.ResourceType.Texture)
                            SendEvent(ResourceEvent.COMPLETE, assetInfo, mTextureHandler.texture, true);
                        else if (assetInfo._Type == ResourceManager.ResourceType.Video)
                            SendEvent(ResourceEvent.COMPLETE, assetInfo, mDownloadHandler.data, true);
                        else if (assetInfo._Type == ResourceManager.ResourceType.AudioClip)
                            SendEvent(ResourceEvent.COMPLETE, assetInfo, mAudioClipHandler.audioClip, true);
                        else if (assetInfo._Type == ResourceManager.ResourceType.Pdf)
                            SendEvent(ResourceEvent.COMPLETE, assetInfo, mDownloadHandler.data, true);
                    }
                }
                if (mRetryCount < 0 || clearList)
                    mAssets.Clear();
            }
#if HANDLE_EXCEPTIONS
            catch (Exception e)
            {
                if (mAssetBundleHandler == null)
                    XDebug.LogException($"mAssetBundleHandler is null : mURL: {mAssetName}");
                else if (mAssetBundleHandler.assetBundle == null)
                    XDebug.LogException($"mAssetBundleHandler.assetBundle is null : mURL: {mAssetName}");
                XDebug.LogException($"Exception while loading asset: {e.Message}, URL: {mAssetName}, \nStackTrace: {e.StackTrace}, \nObject State: {ReflectionUtilities.GetInstanceValues(this)}");
            }
#endif //HANDLE_EXCEPTIONS

        }

        /// <summary>
        /// Trigger the callback for asset loading
        /// </summary>
        /// <param name="inEvent">Callback to be called</param>
        /// <param name="assetInfo">AssetInfo that was loaded</param>
        /// <param name="obj">Loaded object</param>
        /// <param name="unregisterDelegate">Should we unsubscribe from callback?</param>
        private void SendEvent(ResourceEvent inEvent, AssetInfo assetInfo, object obj, bool unregisterDelegate)
        {
            if (assetInfo == null || assetInfo._Event == null)
                return;
            Delegate[] invocationList = assetInfo._Event.GetInvocationList();
            for (int i = 0; i < invocationList.Length; ++i)
            {
                if (invocationList[i] == null || assetInfo._Event == null)
                    continue;
                bool sendEvent = true;
                if (invocationList[i].Target != null && invocationList[i].Target.ToString() == "null")
                {
                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        XDebug.LogWarning($"Target object is null for : {invocationList[i].Method}, URL: {assetInfo._AssetName}", XDebug.Mask.Resources);
                    sendEvent = false;
                }
                //For static callbacks Target will be null, For for instanced objects, if the object is deleted, Target will be null string.
                //We trigger the event only if, target is null or if the target object is not null
                if (sendEvent || invocationList[i].Target == null)
                {
                    ResEvent dlgt = invocationList[i] as ResEvent;
                    if (unregisterDelegate)
                        assetInfo._Event -= dlgt;
                    dlgt(inEvent, assetInfo.Path(), obj, assetInfo._UserData);
                    if (assetInfo._Event != null)
                        invocationList = assetInfo._Event.GetInvocationList();
                }
            }
        }

#endregion Public Methods

#region Private & Protected Methods
        /// <summary>
        /// Called when depended asset bundle is loaded
        /// </summary>
        /// <param name="inEvent">Callbck to be called</param>
        /// <param name="inUrl">URL of the asset</param>
        /// <param name="inObject">Loaded object</param>
        /// <param name="inUserData">Any user data to be passed in callback</param>
        private void OnDependencyLoaded(ResourceEvent inEvent, string inUrl, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Loaded dependency \"{inUrl}\" for \"{mBundleName}\"",XDebug.Mask.Resources);

                OnDependencyLoaded(inUrl);

                if (XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log($"Remaining dependency files for \"{mBundleName}\" : {mNumDependencies}", XDebug.Mask.Resources);
                if (mNumDependencies == 0)
                    OnResourceLoaded(mObject, 1f, true);
            }
            else if (inEvent == ResourceEvent.ERROR)
                XDebug.LogError("Failed to load : " + inUrl, XDebug.Mask.Resources);
        }

        /// <summary>
        /// Called when depended asset bundle is loaded
        /// </summary>
        /// <param name="inUrl">URL of the asset</param>
        private void OnDependencyLoaded(string inUrl)
        {
            if (mDependencies == null)
                mDependencies = new List<Resource>();
            Resource res = ResourceManager.GetResource(inUrl);
            if (res != null)
                mDependencies.Add(res);
            mNumDependencies--;
        }

        /// <summary>
        /// Start loading the asset
        /// </summary>
        private void Load()
        {
            string fullPath = null;
            (string fullPath, bool isRemoteAsset) resp = ResourceManager.ResolvePath(mBundleName, mResourceType, mIsRemoteAsset);

            fullPath = resp.fullPath;
            mIsRemoteAsset = resp.isRemoteAsset;
            if (mLoadDependency && (mResourceType == ResourceManager.ResourceType.AssetBundle || mResourceType == ResourceManager.ResourceType.Object))
            {
                string[] dependencies = ResourceManager.GetAllDependencies(mBundleName);
                mNumDependencies = dependencies.Length;
                if (dependencies.Length > 0)
                    LoadDependencies(dependencies);
            }

            if(XDebug.CanLog(XDebug.Mask.Resources))
                XDebug.Log("Loading : " + fullPath, XDebug.Mask.Resources);
            if (mResourceType == ResourceManager.ResourceType.AssetBundle || mResourceType == ResourceManager.ResourceType.Object)
                AsyncLoader.pInstance.StartCoroutine(AsyncLoader.pInstance.LoadAssetBundle(fullPath, OnResourceLoaded, ResourceManager.GetVersionAssetHash(mBundleName)));
            else
                AsyncLoader.pInstance.StartCoroutine(AsyncLoader.pInstance.Load(fullPath, OnResourceLoaded, mResourceType));
        }

        /// <summary>
        /// Start loading the asset. Remote url in this case
        /// </summary>
        private void LoadURL()
        {
            if (mResourceType == ResourceManager.ResourceType.AssetBundle || mResourceType == ResourceManager.ResourceType.Object)
                AsyncLoader.pInstance.StartCoroutine(AsyncLoader.pInstance.LoadAssetBundle(mAssetName, OnResourceLoaded, ResourceManager.GetVersionAssetHash(mBundleName)));
            else
                AsyncLoader.pInstance.StartCoroutine(AsyncLoader.pInstance.Load(mAssetName, OnResourceLoaded, mResourceType));
        }

        /// <summary>
        /// Starts loading given dependent assets for the asset bundle being loaded
        /// </summary>
        /// <param name="dependencies"></param>
        private void LoadDependencies(string[] dependencies)
        {
            int length = dependencies.Length;
            for (int i = 0; i < length; ++i)
                LoadAssetBundleInternal(dependencies[i]);
        }

        /// <summary>
        /// Load asset bundle
        /// </summary>
        /// <param name="assetBundle"></param>
        private void LoadAssetBundleInternal(string assetBundle)
        {
            ResourceManager.Load(assetBundle, OnDependencyLoaded, ResourceManager.ResourceType.AssetBundle, null, false);
        }

#endregion Private & Protected Methods
    }
}