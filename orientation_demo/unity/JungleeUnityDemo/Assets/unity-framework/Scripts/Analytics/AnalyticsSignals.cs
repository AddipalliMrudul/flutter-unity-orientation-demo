using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames.Analytics
{
    public class SigAnalyticsLevelStart : Signal<string, Dictionary<string, object>> { }
    public class SigAnalyticsLevelEnd : Signal<string, Dictionary<string, object>> { }
    public class SigAnalyticsWatchRewardVideo : Signal { }
    public class SigAnalyticsRewardVideoCompleted : Signal { }
    public class SigAnalyticsCustomEvent : Signal<string, Dictionary<string, object>> { }
}
