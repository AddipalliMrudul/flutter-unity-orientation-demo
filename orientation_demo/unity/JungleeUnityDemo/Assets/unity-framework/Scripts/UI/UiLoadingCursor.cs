using System;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.UI;

namespace XcelerateGames
{
    /// <summary>
    /// Class to show loading cursor. By calling Show we can show a loading cursor & block all user input.
    /// The visibility is refernce counted, If we call Show(true) twice, then we must call Show(false) twice.
    /// </summary>
    public class UiLoadingCursor : BaseBehaviour
    {
        #region Member Variables
        public UiProgressBar _ProgressBar = null;

        private int mRefCount = 0;

        protected static UiLoadingCursor mInstance = null;
        public static bool IsActive => mInstance != null && mInstance.mRefCount > 0;
        public static Func<UiLoadingCursor> LoadingFunction = null; /**< Set a function pointer to control the loading of Loading Screen UI.*/
        #endregion Member Variables

        #region Public Methods
        /// <summary>
        /// Shows a exclusive/modal loading cursor. Every call to show(true) will increase the ref count & every call to Show(false) will reduce the ref count.
        /// When the ref count is zero, the UI will be hidden.
        /// </summary>
        /// <param name="inShow"></param>
        public static void Show(bool inShow)
        {
            if(XDebug.CanLog(XDebug.Mask.Game))
                XDebug.Log($"Show loading cursor:{inShow}", XDebug.Mask.Game);
            if (mInstance == null)
                CreateInstance();
            if (inShow)
            {
                mInstance.mRefCount++;
                mInstance.gameObject.SetActive(true);
            }
            else
            {
                mInstance.mRefCount--;
                if (mInstance.mRefCount <= 0)
                {
                    mInstance.mRefCount = 0;
                    mInstance.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Set progress on loading cursor. Value must be betwee 0 & 1
        /// </summary>
        /// <param name="progress">Progress value. Must be between 0 & 1</param>
        public static void SetProgress(float progress)
        {
            if (mInstance != null && mInstance._ProgressBar != null)
            {
                mInstance._ProgressBar.SetProgress(progress);
                mInstance._ProgressBar.text = $"{(int)(progress * 100)}%";
            }
        }

        /// <summary>
        /// Show progress truw/false
        /// </summary>
        /// <param name="show">show?</param>
        public static void ShowProgress(bool show)
        {
            if (mInstance != null && mInstance._ProgressBar != null)
            {
                mInstance._ProgressBar.gameObject.SetActive(show);
                mInstance._ProgressBar.text = "0%";
            }
        }

        #endregion Public Methods

        #region Private/Protected Methods
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// Creates the static instance of the exclusive loading cursor.
        /// </summary>
        protected static void CreateInstance()
        {
            if (LoadingFunction == null)
            {
                GameObject obj = ResourceManager.LoadFromResources("PfUiLoadingCursor") as GameObject;
                obj = Instantiate(obj) as GameObject;
                obj.name = "PfUiLoadingCursor";
                mInstance = obj.GetComponent<UiLoadingCursor>();
            }
            else
            {
                mInstance = LoadingFunction.Invoke();
            }
            ShowProgress(false);
        }

        public static void Destroy()
        {
            if (mInstance != null)
            {
                Destroy(mInstance.gameObject);
            }
        }

        [ContextMenu("Reset State")]
        private void ResetState()
        {
            mInstance.mRefCount = 0;
            Show(false);
        }
        #endregion Private/Protected Methods
    }
}