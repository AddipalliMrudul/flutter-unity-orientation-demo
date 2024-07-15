using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XcelerateGames.IOC;
using XcelerateGames.Timer;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.UpdateManager
{
    public class UpdateManager : BaseBehaviour
    {
        [Flags]
        [Serializable]
        public enum Trigger
        {
            Launch = 1 << 1,
            MinMax = 1 << 2,
            Manual = 1 << 3,
        }

        public enum State
        {
            None,
            Initializing,
            Initialized,
            Waiting,
            Downloading,
            Done,
            Pause,
            Resume
        }

        [Tooltip("Update notification will be sent only if one of these scene is not loaded.")]
        public string[] _NotifyIgnoreScenes = null;

        [SerializeField] bool _DontDestroyOnLoad = true;
        [EnumFlag] public Trigger _Trigger = Trigger.Launch;
        [SerializeField] float _Delay = 1f;
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] bool _Debug = false;
        [SerializeField] bool _CheckForUpdateOnAppFocus = false;
#endif
        private State mState = State.None;
        private State mPreviousState = State.None;

        //TODO:Make a signal for it if needed & delete this variable
        private System.Action<State> OnStateChanged = null;

        private volatile List<string> mUpdatedAssetList = null;
        private bool mSendUpdateEventPending = false;

        #region Signals
        [InjectSignal] private SigAssetUpdated mSigAssetUpdated = null;
        [InjectSignal] private SigCheckForAssetUpdate mSigCheckForAssetUpdate = null;
        [InjectSignal] private SigTimerRegister mSigRegister = null;
        [InjectSignal] private SigGetVersionListHash mSigGetVersionListHash = null;
        //[InjectSignal] private SigTimerUnregister mSigUnregister = null;
        #endregion Signals

        public State pState
        {
            get { return mState; }

            private set
            {
                if (mState != value)
                {
                    mPreviousState = mState;
                    mState = value;
                    if (mState == State.Initialized)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Initialization complete", XDebug.Mask.UpdateManager);
                        CheckUpdates();
                    }
                    else if (mState == State.Done)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Done downloading", XDebug.Mask.UpdateManager);
                        if (mUpdatedAssetList != null)
                            mUpdatedAssetList.Clear();
                        //StopAllCoroutines();
                        //Run in background if in IDE, helps in debugging
                        //Application.runInBackground = ProductConfig.pRunInBackground;
                    }
                    else if (mState == State.Downloading)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Downloading updates", XDebug.Mask.UpdateManager);
                    }
                    else if (mState == State.Pause)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Updating Paused", XDebug.Mask.UpdateManager);
                    }
                    else if (mState == State.Resume)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Updating Resumed", XDebug.Mask.UpdateManager);
                        if (mUpdatedAssetList.Count > 0)
                            mState = mPreviousState;
                        else
                            pState = State.Done;
                    }
                    if (OnStateChanged != null)
                        OnStateChanged(mState);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ResourceManager.OnSceneLoadedEvent += OnSceneLoadedEvent;
            mSigCheckForAssetUpdate.AddListener(CheckForUpdates);
            if ((_Trigger & Trigger.Launch) != 0)
                ConnectivityMonitor.AddListener(OnInternetStatusChanged, false);
            if (_DontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        protected override void OnDestroy()
        {
            ResourceManager.OnSceneLoadedEvent -= OnSceneLoadedEvent;
            ConnectivityMonitor.RemoveListener(OnInternetStatusChanged);
            mSigCheckForAssetUpdate.RemoveListener(CheckForUpdates);
            base.OnDestroy();
        }

        private void OnSceneLoadedEvent(string levelName)
        {
            if (pState == State.Pause)
                pState = State.Resume;

            if (mSendUpdateEventPending)
            {
                mSendUpdateEventPending = false;
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log("Update event was pending, sending now.", XDebug.Mask.UpdateManager);
                SendUpdateEvent();
            }
        }

        private void OnInternetStatusChanged(ConnectivityMonitor.Status newStatus)
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                XDebug.Log($"Internet state changed to {newStatus}", XDebug.Mask.UpdateManager);
            if (newStatus == ConnectivityMonitor.Status.Online)
            {
                CheckForUpdates();
            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (_Debug)
                XDebug.AddMask(XDebug.Mask.UpdateManager);
#endif
        }

        private bool CanUpdate()
        {
            if (!PlatformUtilities.IsLocalBuild() && ConnectivityMonitor.pIsInternetAvailable)
                return true;
            return false;
        }

        private void CheckForUpdates()
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                XDebug.Log("UpdateManager::CheckForUpdates called", XDebug.Mask.UpdateManager);
            if (CanUpdate())
            {
                if (pState != State.Downloading && !mSendUpdateEventPending)
                {
                    pState = State.Initializing;
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log("Checking for updates", XDebug.Mask.UpdateManager);
                    //Application.runInBackground = true;
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log("UpdateManager:: Getting version list hash", XDebug.Mask.UpdateManager);
                    mSigGetVersionListHash.Dispatch(OnVersionHashSet);
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log($"UpdateManager:: Not updating {pState}, {mSendUpdateEventPending}", XDebug.Mask.UpdateManager);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log("UpdateManager::CheckForUpdates Not updating", XDebug.Mask.UpdateManager);
                pState = State.Done;
            }
        }

        private void OnVersionHashSet(bool success)
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                XDebug.Log($"UpdateManager::OnVersionHashSet {success}", XDebug.Mask.UpdateManager);

            if (success)
                LoadVersionList();
        }

        private void LoadVersionList()
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                XDebug.Log("UpdateManager::LoadVersionList Loading VersionList", XDebug.Mask.UpdateManager);

            AsyncLoader.LoadVersionList(OnVersionListLoaded);
        }

        private void CheckUpdates()
        {
            //Now check if there is an update to the assets that were shipped with the app.
            Dictionary<string, string> shippedAssets = ResourceManager.pShippedAssets;
            if (shippedAssets != null)
            {
                foreach (string asset in shippedAssets.Keys)
                {
                    if (shippedAssets[asset].Equals("NA"))
                        continue;
                    //Skip Data_iOS, Data_Android files
                    if (asset.Equals(PlatformUtilities.GetAssetFolderPath()))
                        continue;
                    if (!IsCached(asset))
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Update avaialble for shipped asset : " + asset, XDebug.Mask.UpdateManager);
                        AddToList(asset);
                    }
                }
            }

            //Now check if there is an update for cached items. This is required for secondary prefecth list items.
            if (ResourceManager.pCache != null)
            {
                Dictionary<string, string> tempCaheList = new Dictionary<string, string>(ResourceManager.pCache);
                foreach (string key in tempCaheList.Keys)
                {
                    if (!ResourceManager.IsCached(key, ResourceManager.GetResourceType(key)))
                    {
                        if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                            XDebug.Log("Update avaialble for cached asset : " + key, XDebug.Mask.UpdateManager);
                        AddToList(key);
                    }
                }
            }

            if (mUpdatedAssetList != null && mUpdatedAssetList.Count > 0)
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log(pState + " : updating " + mUpdatedAssetList.Count + " assets.", XDebug.Mask.UpdateManager);
                pState = State.Downloading;
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                //Now get variants for all asset bundles
                for (int i = 0; i < mUpdatedAssetList.Count; ++i)
                {
                    if (ResourceManager.GetResourceType(mUpdatedAssetList[i]) == ResourceManager.ResourceType.AssetBundle)
                        mUpdatedAssetList[i] = ResourceManager.ResolveVariantName(mUpdatedAssetList[i]);
                }

                mUpdatedAssetList = mUpdatedAssetList.Distinct().ToList();
                mUpdatedAssetList.RemoveAll(IsCached);

                if (mUpdatedAssetList.Count > 0)
                {
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log("Update available for " + mUpdatedAssetList.Count + " assets.", XDebug.Mask.UpdateManager);
                    Prefetch(mUpdatedAssetList[0]);
                }
                else
                {
                    pState = State.Done;
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log("Either all Assets are already updates or no updates are available", XDebug.Mask.UpdateManager);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log(pState + " : No updates available");
                pState = State.Done;
            }
        }

        private void AddToList(string assetName)
        {
            if (mUpdatedAssetList == null)
                mUpdatedAssetList = new List<string>();
            if (!mUpdatedAssetList.Contains(assetName))
                mUpdatedAssetList.Add(assetName);
        }

        private void OnVersionListLoaded(Dictionary<string, AssetData> versionList)
        {
            if (versionList != null)
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log("UpdateManager::OnVersionListLoaded Loaded VersionList", XDebug.Mask.UpdateManager);
                ResourceManager.mAssetVersions = versionList;
                pState = State.Initialized;
            }
            else
            {
                XDebug.LogError("Error! Failed to load version list, nothing will be updated.", XDebug.Mask.UpdateManager);
                pState = State.Initialized;
            }
        }

        private void OnBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            string assetName = inUserData as string;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log($"UpdateManager::OnBundleLoaded : {inURL}", XDebug.Mask.UpdateManager);
                //mTxtLoading.SetText(mIndex + "/" + mPrefetchList.Count);
                ResourceManager.Unload(inURL);
                if (ResourceManager.IsTextAsset(inURL))
                {
                    //We will cache these files in player prefs. Key will be file name.
                    UnityEngine.PlayerPrefs.SetString(assetName, inObject as string);
                    ResourceManager.AddToCache(assetName, ResourceManager.GetVersionAssetHash(assetName));
                }
            }
            else if (inEvent == ResourceEvent.ERROR)
                XDebug.LogError("Failed to load : " + inURL, XDebug.Mask.Prefetching);

            mUpdatedAssetList.Remove(assetName);
            if (mUpdatedAssetList.Count == 0)
            {
                SendUpdateEvent();

                if (pState == State.Downloading)
                    pState = State.Done;
            }
            else
                Prefetch(mUpdatedAssetList[0]);
        }

        private void Prefetch(string assetName)
        {
            if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                XDebug.Log("Updating : " + assetName + ", " + Time.frameCount, XDebug.Mask.UpdateManager);
            ResourceManager.Load(/*PlatformUtilities.GetPlatformAssetPath*/(assetName), OnBundleLoaded, ResourceManager.GetResourceType(assetName), assetName);
            ResourceManager.MarkImmortal(assetName);
        }

        private bool IsCached(string assetPath)
        {
            if (ResourceManager.IsUpdateAvailable(assetPath, ResourceManager.GetResourceType(assetPath)))
                return ResourceManager.IsCached(assetPath, ResourceManager.GetResourceType(assetPath));
            return true;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
#if UNITY_EDITOR
                focus = _CheckForUpdateOnAppFocus;
#endif
            }
            if (focus)
            {
                if ((_Trigger & Trigger.MinMax) != 0)
                {
                    mSigRegister.Dispatch((dt) =>
                    {
                        ConnectivityMonitor.AddListener(OnInternetStatusChanged, false);
                    }, _Delay);
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                        XDebug.Log($"UpdateManager::OnApplicationFocus : trigger condition not met{_Trigger.ToString()}", XDebug.Mask.UpdateManager);
                }
            }
        }

        private void SendUpdateEvent()
        {
            bool canSendUpdateEvent = Array.Find(_NotifyIgnoreScenes, e => e.Equals(ResourceManager.pCurrentScene, StringComparison.OrdinalIgnoreCase)) == null;
            if (canSendUpdateEvent)
            {
                if (mUpdatedAssetList != null && mUpdatedAssetList != null)
                {
                    for (int i = 0; i < mUpdatedAssetList.Count; ++i)
                    {
                        //Force unload the asset before sending update event
                        ResourceManager.Unload(mUpdatedAssetList[i], true, false);
                    }
                }
                mSigAssetUpdated.Dispatch(mUpdatedAssetList);
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.UpdateManager))
                    XDebug.Log("Update event pending, Will try to send on next level load.", XDebug.Mask.UpdateManager);
                mSendUpdateEventPending = true;
            }
        }

        private bool IsAssetUpdated(string assetName)
        {
            if (mUpdatedAssetList != null)
                return mUpdatedAssetList.Contains(assetName);
            return false;
        }
    }
}