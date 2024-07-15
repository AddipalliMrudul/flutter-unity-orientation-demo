using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// This class preloads all assets & waits for any module to get ready before destroying loading screen. Loading screen will not be destroyed
    /// until all assets are loaded & all items in WaitList are ready
    /// </summary>
    public class LevelInit : BaseBehaviour
    {
        enum State
        {
            PreLoadingBundles,
            WaitList,
            Complete
        }
        #region Properties
        [SerializeField] List<WaitList> _WaitList = null;       /**< Items to wait for*/
        [SerializeField] List<string> _PreLoadBundles = null;   /**< Assets to preload*/

        [SerializeField] List<GameObject> OnLevelReadyReceivers = null; /**< Notify objects in scene that scene is ready. @See SigSceneReady*/
        private int mBundleIndex = 0;

        private State mState = State.PreLoadingBundles;
        #endregion Properties

        #region Signals & Models
        [InjectSignal] private SigSceneReady mSigSceneReady = null;
        #endregion Signals & Models

        /// <summary>
        /// Property that holds current state of process
        /// </summary>
        private State pState
        {
            get { return mState; }
            set
            {
                mState = value;
                if(XDebug.CanLog(XDebug.Mask.Resources))
                    XDebug.Log("LevelInit State : " + mState, XDebug.Mask.Resources);
                if (mState == State.PreLoadingBundles)
                {
                    if (_PreLoadBundles.Count == 0)
                        pState++;
                    else
                        ResourceManager.Load(_PreLoadBundles[mBundleIndex], OnBundleLoaded, ResourceManager.ResourceType.AssetBundle);
                }
                else if (mState == State.WaitList)
                {
                    if (_WaitList.Count == 0)
                        pState++;
                }
                else if (mState >= State.Complete)
                {
                    if (ResourceManager.pUiLoadingScreen != null)
                        ResourceManager.pUiLoadingScreen._OnLoadingComplete += OnLoadingScreenDestroyed;
                    else
                        OnLoadingScreenDestroyed();
                    ResourceManager.DestroyLoadingScreen();
                }
            }
        }

        /// <summary>
        /// Writes the state of the process to the given stream. For debugging purpose only. 
        /// </summary>
        /// <param name="fout">Stream to write the process status</param>
        internal void Dump(StreamWriter fout)
        {
            string info = "Level Init State : " + mState + "\n Wait List : \n";
            if (_WaitList.Count > 0)
            {
                foreach (WaitList wl in _WaitList)
                {
                    if (!wl.IsReady())
                        info += wl.name + "\n";
                }
            }
            else
                info += "Wait list empty";
            fout.WriteLine(info);
        }

        /// <summary>
        /// Call back from loading screen when its destroyed
        /// </summary>
        private void OnLoadingScreenDestroyed()
        {
            mSigSceneReady.Dispatch(ResourceManager.pCurrentScene);
            foreach (GameObject go in OnLevelReadyReceivers)
            {
                if (go != null)
                    go.SendMessage("OnLevelReady", ResourceManager.pCurrentScene, SendMessageOptions.DontRequireReceiver);
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Remove all null obhects & Start the process.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            //Remove all empty items.
            _WaitList.RemoveAll(e => e == null);
            pState = State.PreLoadingBundles;
        }

        // Keep checkking for items in WaitList if they are ready
        void Update()
        {
            if (pState == State.WaitList)
            {
                bool complete = true;
                for (int i = 0; i < _WaitList.Count; ++i)
                {
                    if (!_WaitList[i].IsReady())
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                    pState++;
            }
        }

        /// <summary>
        /// Callback from ResourceManager when bundle is loaded
        /// </summary>
        /// <param name="inEvent">Loading event</param>
        /// <param name="inURL">URL of the asset</param>
        /// <param name="inObject">Loaded object</param>
        /// <param name="inUserData">Any custom data that was passed</param>
        private void OnBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.ERROR || inEvent == ResourceEvent.COMPLETE)
            {
                mBundleIndex++;
                if (mBundleIndex >= _PreLoadBundles.Count)
                    pState = State.WaitList;
                else
                    ResourceManager.Load(_PreLoadBundles[mBundleIndex], OnBundleLoaded, ResourceManager.ResourceType.AssetBundle);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ResourceManager.pUiLoadingScreen != null)
                ResourceManager.pUiLoadingScreen._OnLoadingComplete -= OnLoadingScreenDestroyed;
        }
    }
}
