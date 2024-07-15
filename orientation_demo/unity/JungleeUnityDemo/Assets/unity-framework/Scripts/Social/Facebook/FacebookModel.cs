#if FB_ENABLED
using Facebook.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.IOC;

namespace XcelerateGames.Social.Facebook
{
    public class FacebookModel : XGModel
    {
        public List<FacebookUser> mGameFriends = new List<FacebookUser>();
        public List<FacebookUser> mNonGameFriends = new List<FacebookUser>();
        public List<FacebookUser> mInvitableFriends = new List<FacebookUser>();
        public List<FacebookRequest> mRequestList = new List<FacebookRequest>();
        public FacebookUser mCurrentUser = null;
        public Dictionary<string, object> mCurrencyInfo = null;

        public bool mLoginCancelled = false;

        public List<string> mFBPermissions = new List<string>() { "public_profile", "email", "user_friends" };

        public long? mUserID;
        public const string mRecentInvitedFriendsKey = "RecentInvitedFriends";

        public List<FacebookUser> GameFriends { get { return mGameFriends; } }
        public List<FacebookUser> NonGameFriends { get { return mNonGameFriends; } }
        public List<FacebookUser> InvitableFriends { get { return mInvitableFriends; } }
        public List<FacebookRequest> pRequestList { get { return mRequestList; } }
        public bool pLoginCancelled { get { return mLoginCancelled; } }
        public bool mIsInitialized = false;


        public string AppIdDev = null;
        public string AppIdLive = null;

        public string AppId
        {
            get
            {
                if (PlatformUtilities.GetEnvironment() == PlatformUtilities.Environment.live)
                    return AppIdLive;
                else
                    return AppIdDev;
            }
        }

        public double GetAdjustedCurrency(double priceInDollars)
        {
            if (mCurrencyInfo != null)
            {
                if (mCurrencyInfo.ContainsKey("usd_exchange_inverse"))
                    return priceInDollars * (double)mCurrencyInfo["usd_exchange_inverse"];
            }
            XDebug.LogError("Either instance or mCurrencyInfo is null, returning default value");
            return priceInDollars;
        }


        public string GetQuery(string id, PictureType type)
        {
            return string.Format("https://graph.facebook.com/{0}/picture?type={1}", id, type.ToString());
        }

        public List<string> GetGameFriendIDs(bool addSelf = false)
        {
            List<string> friendIDs = new List<string>();
            foreach (FacebookUser user in mGameFriends)
                friendIDs.Add(user.ID);

            if (addSelf)
            {
                if (mCurrentUser != null)
                    friendIDs.Add(mCurrentUser.ID);
                else
                    friendIDs.Add(AccessToken.CurrentAccessToken.UserId);
            }
            return friendIDs;
        }

        public List<string> GetAllFriendIDs()
        {
            List<string> friendIDs = new List<string>();
            foreach (FacebookUser user in mGameFriends)
                friendIDs.Add(user.ID);
            foreach (FacebookUser user in mNonGameFriends)
                friendIDs.Add(user.ID);

            return friendIDs;
        }


        public string GetFBNameByID(string fbID)
        {
            if (mGameFriends != null)
            {
                FacebookUser user = mGameFriends.Find(e => e.ID == fbID);
                if (user != null)
                    return user.FirstName;
            }
            if (mCurrentUser != null && mCurrentUser.ID == fbID)
                return mCurrentUser.FirstName;
            return null;
        }

        public string GetFBFullNameByID(string fbID)
        {
            if (mGameFriends != null)
            {
                FacebookUser user = mGameFriends.Find(e => e.ID == fbID);
                if (user != null)
                    return user.FirstName + " " + user.LastName;
            }
            if (mCurrentUser != null && mCurrentUser.ID == fbID)
                return mCurrentUser.FirstName + " " + mCurrentUser.LastName;
            return null;
        }

        #region getters for private data


        public FacebookUser pCurrentUser
        {
            get
            {
#if SIMULATE_FB_SDK
                return mCurrentUser;
#else
                return mCurrentUser;
#endif
            }
        }

        public bool pIsLoggedIn { get { return ConnectivityMonitor.pIsInternetAvailable && FB.IsLoggedIn; } }

        // Helper function to check whether the player has granted 'publish_actions'
        public bool HavePublishActions
        {
            get
            {
#if SIMULATE_FB_SDK
                return false;
#else
                return (pIsLoggedIn && (AccessToken.CurrentAccessToken.Permissions as List<string>).Contains("publish_actions")) ? true : false;
#endif
            }
        }

        public string pAccessToken
        {
#if SIMULATE_FB_SDK
            get { return RemoteSettings.GetString("FBToken-temp", "EAAGMVXLXOj4BAHSeT7HGLycaWZC910SOHo9w5ZA3DaV5wZAnGnIAc96xoGS4EUSE8l54oVO08dRVyJ4SPZCAUpLZBr7bkB6ZADZAqnlmGcZBSLJE9LvrpyeKVUbkxq8VAHJAZBb8KK9D11AuaT8ZALpO2iM7ZB5EZCTPNZBkZD"); }
#else
            get { return AccessToken.CurrentAccessToken.TokenString; }
#endif
        }

        public long pUserID
        {
            get
            {
#if SIMULATE_FB_SDK
                if (!mUserID.HasValue)
                    mUserID = long.Parse(pCurrentUser.ID);
#else
                if (!mUserID.HasValue)
                    mUserID = long.Parse(AccessToken.CurrentAccessToken.UserId);
#endif
                return mUserID.Value;
            }
        }

        #endregion getters for private data
    }
}
#endif //FB_ENABLED