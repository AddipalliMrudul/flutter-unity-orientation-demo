#if FB_ENABLED
using System;
using Facebook.Unity;
using UnityEngine;
using XcelerateGames.IOC;
using XcelerateGames.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace XcelerateGames.Social.Facebook
{
    public class UiFBUser : UiItem
    {
        #region Models
        [InjectModel] private FacebookModel mFacebookModel = null;
        #endregion //Models

        #region Signals
        [InjectSignal] public SigOnGetUserData mSigOnGetUserData = null;
        #endregion //Signals

        [SerializeField] private PictureType _PictureType = PictureType.large;

        public virtual void Init(FacebookUser user)
        {
            SetText(user.FirstName);
            FB.API(mFacebookModel.GetQuery(user.ID, _PictureType), HttpMethod.GET, ProfilePhotoCallback);
        }

        protected override void Awake()
        {
            base.Awake();

            if (mFacebookModel.pIsLoggedIn && mFacebookModel.pCurrentUser != null)
                Init(mFacebookModel.pCurrentUser);
            else
                mSigOnGetUserData.AddListener(Init);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigOnGetUserData.RemoveListener(Init);
        }

        private void ProfilePhotoCallback(IGraphResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
                XDebug.LogError($"Failed to fetch FB profile pic. Error : {result.Error}");
            else if (result.Texture == null)
                XDebug.LogError("Failed to fetch FB profile pic, texture is null");
            else
                SetTexture(result.Texture);
        }
    }
}
#endif //FB_ENABLED
