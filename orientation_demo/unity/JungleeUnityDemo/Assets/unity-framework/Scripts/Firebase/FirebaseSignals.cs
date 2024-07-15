#if USE_FIREBASE
using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class SigFirebaseInitialized : Signal<bool> { }
    public class SigFirebaseRemoteConfigFetched : Signal<bool> { }
    public class SigFirebaseRemoteConfigUpdated : Signal<List<string>> { }

    public class SigFirebaseNotificationTokenReceived : Signal<string> { };
    public class SigFirebaseNotificationMessageReceived : Signal<Dictionary<string, string>> { };
}
#endif //USE_FIREBASE
