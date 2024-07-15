using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames.Analytics
{
    public class AnalyticsModel : XGModel
    {
        //Common Data, such as user ID etc to be sent by each event
        public Dictionary<string, object> CommonData = new Dictionary<string, object>();
    }
}
