
#if FB_ENABLED
using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Social.Facebook
{
    #region Auth
    public class SigFacebookLogin : Signal<Action<bool, bool>> { }
    public class SigFacebookLogout : Signal<Action> { }
    #endregion

    #region Currency
    public class SigGetCurrencyInfo : Signal { }
    #endregion

    public class SigGetUserData : Signal<Action<FacebookUser>> { }

    #region Friends
    public class SigGetFriends : Signal<Action<bool>> { }
    public class SigInviteFriends : Signal<List<FacebookUser>> { }
    public class SigGetInvitableFriends : Signal { }
    #endregion

    #region Misc
    public class SigGetAppLink : Signal { }
    #endregion

    #region Events
    public class SigOnLogin : Signal<bool, bool> { }
    public class SigOnShare : Signal<bool> { }
    public class SigOnGetInvitableFriends : Signal<bool> { }
    public class SigOnGetFriends : Signal<bool> { }
    public class SigOnSendRequest : Signal<bool> { }
    public class SigOnInviteFriends : Signal<int> { }
    public class SigOnInviteFriend : Signal<bool, int> { }
    public class SigOnGetUserData : Signal<FacebookUser> { }
    public class SigOnUserPictureLoaded : Signal<Texture> { }
    #endregion
}
#endif //FB_ENABLED