using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames.UI
{
    public class UiZoomAndPan : UiBase
     {
        #region Properties
        public UiItem _Image = null;
        public string _Url = null;

        private float mCurrentRotation = 0;
        #endregion //Properties

        #region Signals
        [InjectSignal] private SigSaveFileToGallery mSigSaveFileToGallery = null;
        #endregion //Signals

        #region UI Callbacks
        public void OnClickRotate()
        {
            mCurrentRotation += 90;
            _Image.transform.localRotation = Quaternion.Euler(0f, 0f, mCurrentRotation);
        }

        public void OnClickDownload()
        {
            if(!string.IsNullOrEmpty(_Url))
            {
                mSigSaveFileToGallery.Dispatch(_Url, true);
            }
        }
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();
            _Image._RawImage.SizeToParent();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
     }
}
