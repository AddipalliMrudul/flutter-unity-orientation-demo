#if FB_ENABLED
#if SIMULATE_FB_SDK
public enum HttpMethod
{
    GET,
}

namespace Facebook.Unity
{
    public class FB
    {
        private static bool loggedIn = false;
        public static bool IsLoggedIn
        {
            get
            {
                return loggedIn;
            }
            set { loggedIn = value; }
        }

        public static bool IsInitialized { get; internal set; }
        public static string AppId { get; internal set; }

        internal static void Init(Action init)
        {
            //throw new NotImplementedException();
        }

        internal static void LogInWithReadPermissions(List<string> mFBPermissions, Action<ILoginResult> onLoginCallback)
        {
        }

        internal static void LogOut()
        {
        }

        internal static void ActivateApp()
        {
        }

        internal static void API(string v, object gET, Action<IGraphResult> onGetCurrencyInfo)
        {
        }

        internal static void AppRequest(string v1, List<string> recipientIds, object p1, object p2, int v2, string v3, string v4, Action<IResult> onInviteFriendCallback)
        {
        }

        internal static void ShareLink(Uri uri1, string title, string description, Uri uri2, Action<IResult> handleResult)
        {
        }

        internal static void GetAppLink(Action<IAppLinkResult> onGetAppLink)
        {
        }

        internal static void LogPurchase(float value, string v)
        {
        }
    }

    public class AccessToken
    {
        public static CurrentAccessToken1 CurrentAccessToken;
    }

    public class CurrentAccessToken1
    {
        public string Permissions;

        public string UserId { get; internal set; }
    }
}


public class IBase
{
    public string Error { get; internal set; }
    public string RawResult { get; internal set; }
    public bool Cancelled { get; internal set; }
}


public class IGraphResult : IBase { }
public class IResult : IBase { }
public class IAppInviteResult : IBase { }
public class IAppLinkResult : IBase
{
    public string Url { get; internal set; }
}
public class ILoginResult : IBase { }

public class AccessToken
{
    public static AccessToken CurrentAccessToken = null;

    static AccessToken()
    {
        CurrentAccessToken = new AccessToken();
    }

    public DateTime ExpirationTime
    {
        get { return DateTime.Now; }
    }

    public string UserId
    {
        get { return null; }
    }

    public string TokenString { get; }
}

#endif //SIMULATE_FB_SDK

#endif //FB_ENABLED