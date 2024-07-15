#if FB_ENABLED
using XcelerateGames.UI;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Social.Facebook
{
    public class UiFBProfilePic : UiItem
    {
        #region Models
        [InjectModel] private FacebookModel mFacebookModel = null;
        #endregion //Models

        [SerializeField] private PictureType _PictureType = PictureType.large;

        public virtual void Init(string userId, string userName)
        {
            SetText(userName);
            SetTexture(mFacebookModel.GetQuery(userId, _PictureType), null);
        }
    }
}
#endif //FB_ENABLED
