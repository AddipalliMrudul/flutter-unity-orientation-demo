#if FB_ENABLED
using System;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.Locale;

namespace XcelerateGames.Social.Facebook
{
    #region FacebookManager class
    public class FacebookManager : BaseBehaviour
    {
        [SerializeField] private string AppIdDev = null;
        [SerializeField] private string AppIdLive = null;

        #region Models
        [InjectModel] private FacebookModel mFacebookModel = null;
        #endregion //Models

        #region Signals 
        [InjectSignal] private SigFacebookLogin mSigLogin = null;
        [InjectSignal] private SigFacebookLogout mSigLogout = null;
        [InjectSignal] private SigGetCurrencyInfo mSigGetCurrencyInfo = null;
        [InjectSignal] private SigGetUserData mSigGetUserData = null;
        [InjectSignal] private SigGetFriends mSigGetFriends = null;
        [InjectSignal] private SigInviteFriends mSigInviteFriends = null;
        [InjectSignal] private SigGetInvitableFriends mSigGetInvitableFriends = null;
        [InjectSignal] private SigGetAppLink mSigGetAppLink = null;

        [InjectSignal] private SigOnLogin mSigOnLogin = null;
        [InjectSignal] private SigOnShare mSigOnShare = null;
        [InjectSignal] private SigOnGetInvitableFriends mSigOnGetInvitableFriends = null;
        [InjectSignal] private SigOnGetFriends mSigOnGetFriends = null;
        [InjectSignal] private SigOnSendRequest mSigOnSendRequest = null;
        [InjectSignal] private SigOnInviteFriends mSigOnInviteFriends = null;
        [InjectSignal] private SigOnInviteFriend mSigOnInviteFriend = null;
        [InjectSignal] public SigOnGetUserData mSigOnGetUserData = null;
        [InjectSignal] private SigOnUserPictureLoaded mSigOnUserPictureLoaded = null;
        #endregion //Signals

        //First bool = success?, Second Bool = User Cancelled?
        //public event Action<bool> OnSendAppInvites = null;

        //public event Action<string> OnError;

        #region private methods
        protected override void Awake()
        {
            base.Awake();

            mSigLogin.AddListener(Login);
            mSigLogout.AddListener(Logout);
            mSigGetCurrencyInfo.AddListener(GetCurrencyInfo);
            mSigGetUserData.AddListener(GetUserData);
            mSigGetFriends.AddListener(GetFriends);
            mSigInviteFriends.AddListener(InviteFriends);
            mSigGetInvitableFriends.AddListener(GetInvitableFriends);
            mSigGetAppLink.AddListener(GetAppLink);

            mFacebookModel.AppIdDev = AppIdDev;
            mFacebookModel.AppIdLive = AppIdLive;

            if (mFacebookModel.mIsInitialized == false)
            {
                mFacebookModel.mIsInitialized = true;
                Debug.Log($"FacebookManager::Awake:{mFacebookModel.AppId}");
                DontDestroyOnLoad(gameObject);
                if (!FB.IsInitialized)
                    FB.Init(OnInitCallback);
                //GameDataConfig.OnReady += OnGameDataConfigReady;
#if UNITY_FACEBOOK
                //ResourceManager.OnSceneLoadedEvent += OnLevelLoaded;
#endif
            }
            else
                Destroy(gameObject);
        }



        private void OnInitCallback()
        {
            XDebug.Log("OnInit callback", XDebug.Mask.Facebook);
            if (!FB.IsInitialized)
            {
                XDebug.LogException("FB init failed");
            }
            else
            {
                XDebug.Log("On Init: is logged in == " + FB.IsLoggedIn, XDebug.Mask.Facebook);
                if (FB.IsLoggedIn)
                {
                    FB.ActivateApp();
                    GetUserData(null);
                }
                //else
                //FBLogin();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (!pauseStatus)
            {
                //app resume
                if (FB.IsInitialized)
                {
                    FB.ActivateApp();
                }
                else
                {
                    //Handle FB.Init
                    FB.Init(OnInitCallback);
                }
            }
        }

#if UNITY_FACEBOOK
    private void OnLevelLoaded(string levelName)
    {
        if (mCurrencyInfo == null && pIsLoggedIn)
            GetCurrencyInfo();
    }
#endif //UNITY_FACEBOOK

        #region Currency
        private void GetCurrencyInfo()
        {
            FB.API("me?fields=currency", HttpMethod.GET, OnGetCurrencyInfo);
        }

        private void OnGetCurrencyInfo(IGraphResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("An error occured while reading currency info: " + result.Error);
            }
            else
            {
                XDebug.Log("Currency Info : " + result.RawResult, XDebug.Mask.Facebook);
                mFacebookModel.mCurrencyInfo = result.RawResult.FromJson<Dictionary<string, object>>();
                mFacebookModel.mCurrencyInfo = (Dictionary<string, object>)(mFacebookModel.mCurrencyInfo["currency"]);
            }
        }
        #endregion

        #region Auth
        private void Login(Action<bool, bool> callback = null)
        {
            if (mFacebookModel.pIsLoggedIn)
            {
                callback?.Invoke(true, true);
            }
            else
            {
                if (callback != null)
                    mSigOnLogin.AddListener(callback);

                FB.LogInWithReadPermissions(mFacebookModel.mFBPermissions, OnLoginCallback);
            }
        }

        private void Logout(Action OnLogout = null)
        {
            FB.LogOut();
            mFacebookModel.mCurrentUser = null;
            OnLogout?.Invoke();
        }
        #endregion

        //public  string GetAdjustedCurrencyWithSymbol(double priceInDollars)
        //{
        //    string symbol = string.Empty;
        //    string symbolicatedPrice = GetAdjustedCurrency(priceInDollars).ToString("00.00");
        //    if (Instance != null && Instance.mCurrencyInfo != null)
        //    {
        //        if (Instance.mCurrencyInfo.ContainsKey("user_currency"))
        //            symbol = (string)Instance.mCurrencyInfo["user_currency"];
        //        string data = GameDataConfig.GetString("CurrencySymbols", "USD=$;INR=₹;EUR=€;GBP=£");
        //        string[] symbols = data.Split(';');
        //        symbol = Array.Find(symbols, e => e.Contains(symbol));
        //        if (!string.IsNullOrEmpty(symbol))
        //        {
        //            symbol = symbol.Split('=')[1];
        //        }
        //    }
        //    return symbol + symbolicatedPrice;
        //}

        //private void OnGameDataConfigReady()
        //{
        //    GameDataConfig.OnReady -= OnGameDataConfigReady;
        //}

        private void GetUserData(Action<FacebookUser> inCaller)
        {
            if (inCaller != null)
                mSigOnGetUserData.AddListener(inCaller);
            FB.API("me?fields=id,email,first_name,last_name,picture,birthday,gender", HttpMethod.GET, OnGetUserCallback);
        }

        private void makeCallWithReadPermission(Action call)
        {
            if (mFacebookModel.pIsLoggedIn)
                call();
            else//TODO:Handle the case where login fails.
                Login((bool status, bool cancelled) => makeCallWithReadPermission(call));
        }


        #region Friends
        private void GetFriends(Action<bool> inCallback)
        {
            if (inCallback != null)
                mSigOnGetFriends.AddListener(inCallback);
            FB.API("me/friends?fields=id,email,first_name,last_name,picture", HttpMethod.GET, OnGetFriendsCallback);
        }

        //        public void SendAppInviteToFriends()
        //        {
        //#if !SIMULATE_FB_SDK
        //            string shareKey = "FBShareUrl";
        //            string shareURl = GameDataConfig.GetString(shareKey, string.Empty);
        //            string shareIcon = GameDataConfig.GetString("FBShareIcon", string.Empty);
        //            XDebug.Log("shareIcon : " + shareIcon, XDebug.Mask.Facebook);
        //            XDebug.Log("shareURl : " + shareURl, XDebug.Mask.Facebook);
        //            FB.Mobile.AppInvite(new Uri(shareURl), new Uri(shareIcon), OnAppInvite);
        //#endif
        //        }

        //private void OnAppInvite(IAppInviteResult result)
        //{
        //    bool status = false;
        //    if (result.Cancelled)
        //        XDebug.LogWarning("OnAppInvite was Cancelled", XDebug.Mask.Facebook);
        //    else if (!string.IsNullOrEmpty(result.Error))
        //    {
        //        XDebug.LogError("OnAppInvite Error : " + result.Error, XDebug.Mask.Facebook);
        //        UnLinkFBAccount(result.Error);
        //    }
        //    else
        //    {
        //        XDebug.Log("OnAppInvite Result : " + result.RawResult, XDebug.Mask.Facebook);
        //        Debug.LogError("OnAppInvite Result : " + result.RawResult);
        //        status = true;
        //    }
        //    if (OnSendAppInvites != null)
        //        OnSendAppInvites(status);
        //}

        private List<FacebookUser> tempList = null;
        private void InviteFriends(List<FacebookUser> recipients)
        {
            List<string> recipientIds = new List<string>();
            foreach (FacebookUser user in recipients)
                recipientIds.Add(user.ID);
            FB.AppRequest(Localization.Get("fb_app_invite_desc"), recipientIds, null, null, 400, "IF", Localization.Get("invite_friends"), OnInviteFriendCallback);

            tempList = new List<FacebookUser>();
            tempList.AddRange(recipients);
            //On IDE, the callback does not have recipienet ID, so just add it here. Helps in debuggig.
            //if (PlatformUtilities.IsEditor())
            //AddToInvitedFriendList(recipients);
        }

        private void OnInviteFriendCallback(IResult result)
        {
            bool status = false;
            int friendsCount = 0;
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnInviteCallback Failed, error: " + result.Error, XDebug.Mask.Facebook);
                //GuiManager.Instance.popupPanel.ShowPanel(result.Error);
                UnLinkFBAccount(result.Error);
            }
            else if (result.Cancelled)
                XDebug.Log("OnInviteCallback : Invite cancelled " + result.Error, XDebug.Mask.Facebook);
            else
            {
                XDebug.Log("OnInviteCallback result: " + result.RawResult, XDebug.Mask.Facebook);
                try
                {
                    //AppRequestResult appRequestResult = (AppRequestResult)result;
                    //if (appRequestResult.To != null)
                    //{
                    //}
                    friendsCount = tempList.Count;
                    status = true;
                }
                catch (Exception e)
                {
                    XDebug.LogException(e);
                }
            }
            tempList = null;
            if (mSigOnInviteFriend != null)
                mSigOnInviteFriend.Dispatch(status, friendsCount);
        }

        //public void GetInvitableFriends()
        //{
        //    makeCallWithReadPermission(getInvitableFriends);
        //}

        private void GetInvitableFriends()
        {
            FB.API("me/invitable_friends", HttpMethod.GET, OnGetInvitableFriendsCallback);
        }

        private void OnGetInvitableFriendsCallback(IGraphResult result)
        {
            bool status = false;
            if (result == null)
                XDebug.LogWarning("OnGetInvitableFriendsCallback : Null Response.", XDebug.Mask.Facebook);
            // Some platforms return the empty string instead of null.
            else if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnGetInvitableFriendsCallback : Error, Error Respone : " + result.Error, XDebug.Mask.Facebook);
                UnLinkFBAccount(result.Error);
            }
            else if (result.Cancelled)
                XDebug.LogError("OnGetInvitableFriendsCallback : Cancelled, Cancelled Respone : " + result.RawResult, XDebug.Mask.Facebook);
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                XDebug.Log("OnGetInvitableFriendsCallback : Success, Success Respone : " + result.RawResult, XDebug.Mask.Facebook);

                mFacebookModel.mInvitableFriends.Clear();
                mFacebookModel.mNonGameFriends.Clear();
                status = true;
                Dictionary<string, object> response = result.RawResult.FromJson<Dictionary<string, object>>();
                List<object> list = (List<object>)response["data"];
                foreach (object userObj in list)
                {
                    Dictionary<string, object> user = (Dictionary<string, object>)userObj;
                    FacebookUser currentUser = new FacebookUser();
                    currentUser.ID = TryReadValue(user, "id");
                    currentUser.EMail = "";
                    currentUser.FirstName = TryReadValue(user, "name");
                    //currentUser.LastName = TryReadValue(user, "last_name");

                    object tmpObject;
                    bool keyExist = user.TryGetValue("picture", out tmpObject);
                    if (keyExist && tmpObject != null)
                    {
                        Dictionary<string, object> picture = (Dictionary<string, object>)tmpObject;
                        keyExist = picture.TryGetValue("data", out tmpObject);
                        if (keyExist && tmpObject != null)
                        {
                            Dictionary<string, object> data = (Dictionary<string, object>)tmpObject;
                            currentUser.UrlToPicture = TryReadValue(data, "url");
                        }
                    }

                    mFacebookModel.mInvitableFriends.Add(currentUser);
                    mFacebookModel.mNonGameFriends.Add(currentUser);
                }
                //Now sort friends by name.
                mFacebookModel.mInvitableFriends.Sort(delegate (FacebookUser fb1, FacebookUser fb2)
                {
                    return fb1.FirstName.CompareTo(fb2.FirstName);
                });

                XDebug.Log("OnGetInvitableFriendsCallback : Total " + mFacebookModel.mInvitableFriends.Count + " invitable friends.", XDebug.Mask.Facebook);
            }
            else
                XDebug.LogWarning("OnGetInvitableFriendsCallback : Empty Response", XDebug.Mask.Facebook);
            if (mSigOnGetInvitableFriends != null)
                mSigOnGetInvitableFriends.Dispatch(status);
        }
        #endregion
        //Invitable friends do not have a ID. To track the user we are extracting unique ID from picture URL.
        private string ExtractUniqueID(string pictureURL)
        {
            int i1 = pictureURL.IndexOf("p50x50");
            int i2 = pictureURL.IndexOf(".jpg");
            return pictureURL.Substring(i1 + 7, i2 - i1 - 7);
        }

        //public  void ShareLink(int level, int score, string description)
        //{
        //    string shareText = Localization.Get("fb_Share_text");
        //    string title = shareText.Replace("{LEVEL}", level.ToString());
        //    title = title.Replace("{SCORE}", score.ToString());
        //    string shareKey = "FBShareUrlBrazil";
        //    string shareURl = GameDataConfig.GetString(shareKey, string.Empty);
        //    string shareIcon = GameDataConfig.GetString("FBShareIcon", string.Empty);
        //    XDebug.Log("shareIcon : " + shareIcon, XDebug.Mask.Facebook);
        //    XDebug.Log("shareURl : " + shareURl, XDebug.Mask.Facebook);

        //    XDebug.Log("Sharing : Link : " + shareURl + ", Desc : " + description + ", Titile : " + title, XDebug.Mask.Facebook);
        //    FB.ShareLink(new Uri(shareURl), title, description, new Uri(shareIcon), Instance.HandleResult);
        //}

        private void GetAppLink()
        {
            FB.GetAppLink(OnGetAppLink);
        }

        private void OnGetAppLink(IAppLinkResult result)
        {
            if (result == null)
            {
                XDebug.LogWarning("Null Response\n", XDebug.Mask.Facebook);
                return;
            }

            // Some platforms return the empty string instead of null.
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("Error, Error Respone : " + result.Error, XDebug.Mask.Facebook);
                UnLinkFBAccount(result.Error);
            }
            else if (result.Cancelled)
                XDebug.LogError("Cancelled, Cancelled Respone : " + result.RawResult, XDebug.Mask.Facebook);
            else if (!string.IsNullOrEmpty(result.RawResult))
                XDebug.Log("Success Respone : " + result.RawResult + ", URL : " + result.Url, XDebug.Mask.Facebook);
            else
                XDebug.LogWarning("Empty Response", XDebug.Mask.Facebook);
        }

        protected void HandleResult(IResult result)
        {
            bool status = false;
            if (result == null)
            {
                XDebug.LogWarning("Null Response\n", XDebug.Mask.Facebook);
                //GuiManager.Instance.popupPanel.ShowPanel(Localization.Get("fb_share_failed"));
            }
            else
            {
                // Some platforms return the empty string instead of null.
                if (!string.IsNullOrEmpty(result.Error))
                {
                    XDebug.LogError("Error, Error Respone : " + result.Error, XDebug.Mask.Facebook);
                    //GuiManager.Instance.popupPanel.ShowPanel(Localization.Get("fb_share_failed"));
                    UnLinkFBAccount(result.Error);
                }
                else if (result.Cancelled)
                    XDebug.LogError("Cancelled, Cancelled Respone : " + result.RawResult, XDebug.Mask.Facebook);
                else if (!string.IsNullOrEmpty(result.RawResult))
                {
                    XDebug.Log("Success, Success Respone : " + result.RawResult, XDebug.Mask.Facebook);
                    status = true;
                }
                else
                {
                    XDebug.LogWarning("Empty Response", XDebug.Mask.Facebook);
                    //GuiManager.Instance.popupPanel.ShowPanel(Localization.Get("fb_share_failed"));
                }
            }

            if (mSigOnShare != null)
                mSigOnShare.Dispatch(status);
        }

        private void OnLoginCallback(ILoginResult result)
        {
            mFacebookModel.mLoginCancelled = result.Cancelled;
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnLoginCallback Failed, error: " + result.Error, XDebug.Mask.Facebook);
                UnLinkFBAccount(result.Error);
                if (mSigOnLogin != null)
                    mSigOnLogin.Dispatch(false, mFacebookModel.mLoginCancelled);
            }
            else
            {
                XDebug.Log("OnLoginCallback succeeded : " + result.RawResult, XDebug.Mask.Facebook);
                if (FB.IsLoggedIn)
                {
                    if (mSigOnLogin != null)
                        mSigOnLogin.Dispatch(true, mFacebookModel.mLoginCancelled);
                    GetUserData(null);
                }
                else
                {
                    if (!mFacebookModel.mLoginCancelled)
                    {
                        //if (GuiManager.Instance != null)
                        //{
                        //    //GuiManager.SetStallPanelActive(false);
                        //    //GuiManager.Instance.popupPanel.ShowPanel("facebook_login_failed");
                        //}
                    }
                    if (mSigOnLogin != null)
                        mSigOnLogin.Dispatch(false, mFacebookModel.mLoginCancelled);
                }
            }
        }

        private void OnGetUserCallback(IGraphResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnGetUser error: " + result.Error, XDebug.Mask.Facebook);
                //GuiManager.Instance.popupPanel.ShowPanel(result.Error);
                UnLinkFBAccount(result.Error);
                return;
            }
            XDebug.Log("OnGetUser result: " + result.RawResult, XDebug.Mask.Facebook);

            FBUserData fBUserData = result.RawResult.FromJson<FBUserData>();
            Dictionary<string, object> user = result.RawResult.FromJson<Dictionary<string, object>>();
            mFacebookModel.mCurrentUser = new FacebookUser(fBUserData);
            if (!string.IsNullOrEmpty(mFacebookModel.mCurrentUser.UrlToPicture))
                ResourceManager.LoadURL(mFacebookModel.mCurrentUser.UrlToPicture, OnFBUserPictureLoaded, ResourceManager.ResourceType.Texture);

            mSigOnGetUserData.Dispatch(mFacebookModel.mCurrentUser);

            GetFriends(null);
            //GetInvitableFriends();
        }

        private void OnFBUserPictureLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                mFacebookModel.mCurrentUser.Picture = inObject as Texture;
                if (mSigOnUserPictureLoaded != null)
                    mSigOnUserPictureLoaded.Dispatch(mFacebookModel.mCurrentUser.Picture);
            }
        }

        private void OnGetFriendsCallback(IGraphResult result)
        {
            bool status = false;
            //if (!string.IsNullOrEmpty(result.Error))
            //{
            //    XDebug.LogError("OnGetFriends Failed! error : " + result.Error, XDebug.Mask.Facebook);
            //    //GuiManager.Instance.popupPanel.ShowPanel(result.Error);
            //    UnLinkFBAccount(result.Error);
            //}
            //else if (result.Cancelled) { }
            //else
            //{
            //    XDebug.Log("OnGetFriends result: " + result.RawResult, XDebug.Mask.Facebook);

            //    mGameFriends = new List<FacebookUser>();
            //    try
            //    {
            //        Dictionary<string, object> response = result.RawResult.FromJson<Dictionary<string, object>>();
            //        List<object> list = (List<object>)response["data"];
            //        foreach (object userObj in list)
            //        {
            //            Dictionary<string, object> user = (Dictionary<string, object>)userObj;
            //            FacebookUser currentUser = new FacebookUser();
            //            currentUser.ID = TryReadValue(user, "id");
            //            currentUser.EMail = "";
            //            currentUser.FirstName = TryReadValue(user, "first_name");
            //            currentUser.LastName = TryReadValue(user, "last_name");

            //            object tmpObject;
            //            bool keyExist = user.TryGetValue("picture", out tmpObject);
            //            if (keyExist && tmpObject != null)
            //            {
            //                Dictionary<string, object> picture = (Dictionary<string, object>)tmpObject;
            //                keyExist = picture.TryGetValue("data", out tmpObject);
            //                if (keyExist && tmpObject != null)
            //                {
            //                    Dictionary<string, object> data = (Dictionary<string, object>)tmpObject;
            //                    currentUser.UrlToPicture = TryReadValue(data, "url");
            //                }
            //            }
            //            mGameFriends.Add(currentUser);
            //        }
            //        //Now sort friends by name.
            //        mGameFriends.Sort(delegate (FacebookUser fb1, FacebookUser fb2)
            //        {
            //            return fb1.FirstName.CompareTo(fb2.FirstName);
            //        });
            //        status = true;
            //    }
            //    catch (Exception e)
            //    {
            //        XDebug.LogException(e);
            //    }
            //}

            mSigOnGetFriends.Dispatch(status);
        }

        private void UnLinkFBAccount(string error)
        {
            if (!string.IsNullOrEmpty(error) && error.Contains("400"))
            {
                FB.LogOut();
                UnityEngine.PlayerPrefs.DeleteKey(GameConfig.FBLinked);
                UnityEngine.PlayerPrefs.Save();
            }
        }

        private void OnSendFBRequestCallback(IResult result)
        {
            bool status = false;
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnSendFBRequest failed! Error : " + result.Error, XDebug.Mask.Facebook);
                //GuiManager.Instance.popupPanel.ShowPanel(result.Error);
                UnLinkFBAccount(result.Error);
            }
            else if (result.Cancelled)
                XDebug.Log("OnSendFBRequest cancelled", XDebug.Mask.Facebook);
            else
            {
                XDebug.Log("OnSendFBRequest succeedded", XDebug.Mask.Facebook);
                status = true;
            }

            mSigOnSendRequest.Dispatch(status);
        }

        private void OnInviteCallback(IResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                XDebug.LogError("OnInviteCallback Failed, error: " + result.Error, XDebug.Mask.Facebook);
                //GuiManager.Instance.popupPanel.ShowPanel(result.Error);
                UnLinkFBAccount(result.Error);

                return;
            }
            else
            {
                XDebug.Log("OnInviteCallback result: " + result.RawResult, XDebug.Mask.Facebook);
                int friendsCount = 0;
                try
                {
                    Dictionary<string, object> parameters = result.RawResult.FromJson<Dictionary<string, object>>();
                    List<object> idsList = (List<object>)parameters["to"];
                    friendsCount = idsList.Count;
                }
                catch (Exception e)
                {
                    XDebug.LogException(e);
                    //GuiManager.Instance.popupPanel.ShowPanel(e.Message);
                }

                mSigOnInviteFriends.Dispatch(friendsCount);
            }
        }

        private bool ValidateRequest(FacebookRequest request)
        {
            FacebookRequest ret = mFacebookModel.mRequestList.Find(
                delegate (FacebookRequest req)
                {
                    return req.ID == request.ID;
                });
            return (ret == null);
        }

        //public void NextRequest()
        //{
        //    if (mRequestList.Count > 0)
        //    {
        //        FacebookRequest request = mRequestList[mRequestList.Count - 1];
        //        string message = request.fromName;
        //        if (!request.isAccepted)
        //        {
        //            message += " request energy from you.";
        //            mRequestList[mRequestList.Count - 1].isAccepted = true;
        //            //GuiManager.Instance.popupPanel.ShowPanel(message, "Confirm", ConfirmRequest, "Reject", DeleteRequest, true, false);
        //        }
        //        else
        //        {
        //            message += " send energy for you.";
        //            //GuiManager.Instance.popupPanel.ShowPanel(message);
        //            PlayerIOManager.Energy.AddEnergy();
        //            DeleteRequest();
        //        }
        //    }
        //}

        //private void ConfirmRequest()
        //{
        //    if (PlayerIOManager.Energy.EnergyCount > 0)
        //    {
        //        PlayerIOManager.Energy.LostEnergy();
        //        List<string> ids = new List<string>();
        //        ids.Add(mRequestList[mRequestList.Count - 1].fromID);
        //        SendRequest("I Send you Energy", ids, SEND_REQUEST_DATA);
        //    }
        //    else
        //        //GuiManager.Instance.popupPanel.ShowPanel("YOU HAVE NO ENERGY.");
        //    DeleteRequest();
        //}

        //private void DeleteRequest()
        //{
        //    ProcessRequestList();
        //    DeleteFinishedRequest(null);
        //    NextRequest();
        //}

        private void ProcessRequestList()
        {
            foreach (FacebookRequest request in mFacebookModel.mRequestList)
            {
                if (request.isAccepted && !request.toDelete)
                    request.toDelete = true;
            }
        }

        private string TryReadValue(Dictionary<string, object> data, string key)
        {
            string ret = "";
            object dictData = new object();
            if (data.TryGetValue(key, out dictData))
                ret = (dictData != null) ? dictData.ToString() : "";
            return ret;
        }

        #endregion private methods
    }

    #endregion FacebookManager class
}
#endif //FB_ENABLED
