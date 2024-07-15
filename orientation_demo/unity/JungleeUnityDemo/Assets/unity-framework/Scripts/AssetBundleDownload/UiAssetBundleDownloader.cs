using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.Timer;

namespace XcelerateGames.UI
{
    /// <summary>
    /// Class to handle downloading of asset bundles on demand.
    /// Functionality: Show an arrow to indicate assets needs to be downloaded
    /// Tick mark: Show user that download is complete
    /// Progress bar: Show the download progress
    /// </summary>
    public class UiAssetBundleDownloader : UiBase
    {
        #region Member variables
        [SerializeField] UiProgressBar _ProgressBar = null; /**<Download progress */
        [SerializeField] GameObject _TickMark = null;   /**<Tick mark to show download complete */
        [SerializeField] GameObject _Arrow = null;  /**<Arrow or some sort of UI that informs user that he/she needs to download the content*/
        [SerializeField] float _HideDelay = 2f; /**<UI will be hidden after this delay */
        [SerializeField] bool _UnloadBundles = true; /**<After downloading asset bundles, should we unload them? */
        #endregion Member variables

        private int mTotalCount = 0;    /**<Count of assets that are being downloaded */
        private int mDownloadedCount = 0;   /**<Count of assets downloaded successfully*/
        private int mFailedCount = 0;   /**<Count of assets failed to downloaded */
        private Dictionary<string, ulong> mDownloadStatus = new Dictionary<string, ulong>();    /**<Download progress of each asset */
        private Action<int, int> mCallback = null;  /**<Callback for asset downloading complete. First argument is the count of assets downloaded & second argument is the count of assets failed to download */
        private string mTotalSizeToDownload = null; /**<Total formatted size being downloaded. To be used to show in UI */

        private List<string> mDownloadables = null;

        #region Signals & Models
        [InjectSignal] private SigTimerRegister mSigTimerRegister = null;
        [InjectSignal] private SigAssetBundleDownloaderState mSigAssetBundleDownloaderState = null;
        private double mTime;
        #endregion Signals & Models

        #region Public Methods
        /// <summary>
        /// Start downloading assets from the given list
        /// </summary>
        /// <param name="downloadables">List of assets that needs to be downloaded</param>
        /// <param name="callback">Callback when asset downloading is done</param>
        public void StartDownloading(List<string> downloadables, Action<int, int> callback)
        {
            if (downloadables.IsNullOrEmpty())
            {
                callback?.Invoke(mDownloadedCount, mFailedCount);
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log("Nothing to download. returing", XDebug.Mask.Game);
                Hide();

                return;
            }

            mDownloadables = downloadables.Distinct().ToList();
            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Asset bundles to download : {mDownloadables.Count}", XDebug.Mask.Game);
            mDownloadables.RemoveAll(delegate (string asset) { return ResourceManager.IsAssetBundleCached(asset); });
            if (mDownloadables.IsNullOrEmpty())
            {
                callback?.Invoke(mDownloadedCount, mFailedCount);
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log("All asset bundles already cached, Nothing to download. returing", XDebug.Mask.Game);
                Hide();

                return;
            }
            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Asset bundles count post caching: {mDownloadables.Count}", XDebug.Mask.Game);

            ConnectivityMonitor.RemoveListener(OnNetworkStatusChanged);
            ConnectivityMonitor.AddListener(OnNetworkStatusChanged, false);
            if (_Arrow)
                _Arrow.SetActive(false);
            if (_TickMark)
                _TickMark.SetActive(false);

            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Downloading {mDownloadables.Count} asset bundles", XDebug.Mask.Game);

            mTotalSizeToDownload = Utilities.FormatBytes((long)ResourceManager.GetBundlesSize(downloadables));
            gameObject.SetActive(true);
            mTotalCount = downloadables.Count;
            mDownloadedCount = mFailedCount = 0;
            mCallback = callback;
            _ProgressBar.SetProgress(0f);
            FireAssetDownloadingSignal(AssetDownloadingState.Started);
            foreach (string asset in downloadables)
            {
                string bundleName = ResourceManager.ResolveVariantName(asset);
                ResourceManager.ResourceType resourceType = ResourceManager.GetResourceType(asset);
                mDownloadStatus[bundleName] = 0;

                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"ABD1: downloading: {bundleName}", XDebug.Mask.Game);
                bool loadingKilled = ResourceManager.Kill(bundleName);
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"Loading killed for {bundleName} ? => {loadingKilled}", XDebug.Mask.Game);
                bool cacheCleared = Caching.ClearAllCachedVersions(bundleName);
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"Cache for {bundleName} cleared? {cacheCleared}", XDebug.Mask.Game);

                ResourceManager.Load(bundleName, OnBundleLoaded, resourceType, inUserData: bundleName, loadDependency: false);
            }
        }
        #endregion Public Methods

        #region Private Methods
        public override void Hide()
        {
            ConnectivityMonitor.RemoveListener(OnNetworkStatusChanged);
            base.Hide();
        }

        public void OnClosePopup()
        {
            FireAssetDownloadingSignal(AssetDownloadingState.PopupClosed);
            Hide();
        }
        private void OnNetworkStatusChanged(ConnectivityMonitor.Status status)
        {
            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"OnNetworkStatusChanged {status}", XDebug.Mask.Game);
            if (status == ConnectivityMonitor.Status.Online)
            {
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log("Onliner now, Resuming download", XDebug.Mask.Game);
                StartDownloading(mDownloadables, mCallback);
            }
        }

        /// <summary>
        /// Event triggered by resource manager on asset loading progress.
        /// If the asset is a text asset, it will be cached in player prefs.
        /// AssetBundles are cached by unity`s caching system.
        /// if _UnloadBundles is true, the loaded asset bundles are unloaded from memory
        /// </summary>
        /// <param name="inEvent">Loading event</param>
        /// <param name="inURL">URL of the asset</param>
        /// <param name="inObject">Loaded object</param>
        /// <param name="inUserData">Any custom data that was passed</param>
        private void OnBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"ABD2: downloaded: {inURL}", XDebug.Mask.Game);

                ++mDownloadedCount;
                mDownloadStatus[inURL] = ResourceManager.GetBundleSize(inURL);
                if (ResourceManager.IsTextAsset(inURL))
                {
                    //We will cache these files in player prefs. Key will be file name.
                    PlayerPrefs.SetString(inURL, inObject as string);
                    ResourceManager.AddToCache(inURL, ResourceManager.GetVersionAssetHash(inURL));
                }
                if (_UnloadBundles)
                    ResourceManager.Unload(inURL, forceUnload: true);
                mDownloadables.Remove(inURL);
                OnBundleLoaded();
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                ++mFailedCount;
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"Error downloading: {inURL} Failed: {mFailedCount}, Downloaded: {mDownloadedCount}", XDebug.Mask.Game);
                OnBundleLoaded();
            }
            // else if (inEvent == ResourceEvent.PROGRESS)
            // {
            //     (float progress, ulong downloadedBytes) data = ((float progress, ulong downloadedBytes))inObject;
            //     mDownloadStatus[inURL] += data.downloadedBytes;
            //     UpdateDownloadProgress();
            // }
        }

        /// <summary>
        /// Called when asset bundle is downloaded. Update the progress bar & if all assets in the given list are downloaded, show the tick mark & close the UI after delay specied in _HideDelay
        /// Invokes the callback with the count of assets that downloaded successfully & failed
        /// </summary>
        private void OnBundleLoaded()
        {
            if (XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Download Progress : {mDownloadedCount}/{mTotalCount}", XDebug.Mask.Game);

            UpdateDownloadProgress();

            if ((mDownloadedCount + mFailedCount) >= mTotalCount)
            {
                if (_TickMark)
                    _TickMark.SetActive(true);
                if (_Arrow)
                    _Arrow.SetActive(false);
                mSigTimerRegister.Dispatch((dt) =>
                {
                    if (_TickMark)
                        _TickMark.SetActive(false);
                    FireAssetDownloadingSignal(AssetDownloadingState.Completed);
                    mCallback?.Invoke(mDownloadedCount, mFailedCount);
                    Hide();
                }, _HideDelay);
            }
        }

        private void UpdateDownloadProgress()
        {
            ulong sizeDownloaded = 0;
            mDownloadStatus.Values.ToList().ForEach(e => sizeDownloaded += e);
            _ProgressBar.SetProgress((mDownloadedCount) / (float)mTotalCount);
            _ProgressBar.SetText($"{Utilities.FormatBytes((long)sizeDownloaded)}/{mTotalSizeToDownload}");
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Space))
                StartDownloading(mDownloadables, mCallback);
        }

        private void FireAssetDownloadingSignal(AssetDownloadingState downloadingState)
        {
            if (downloadingState == AssetDownloadingState.Started)
                mTime = ServerTime.pCurrentTime;
            AssetDownloadData assetDownloadData = new AssetDownloadData
            {
                totalCount = mTotalCount,
                downloadedCount = mDownloadedCount,
                downloadingState = downloadingState,
                downloadDuration = (ServerTime.pCurrentTime - mTime)
            };
            mSigAssetBundleDownloaderState.Dispatch(assetDownloadData);
        }
        #endregion Private Methods
    }
}
