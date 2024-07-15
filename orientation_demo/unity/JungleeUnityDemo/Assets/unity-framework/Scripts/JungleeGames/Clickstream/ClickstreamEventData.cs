using System.Collections.Generic;

namespace JungleeGames.Analytics
{
    /// <summary>
    /// All events
    /// </summary>
    public class ClickstreamEvent
    {
        public List<ClickstreamEventData> events = null;
        public ClickstreamVisitData visit = null;
    }

    /// <summary>
    /// Metadata for each events
    /// </summary>
    public class ClickstreamEventData
    {
        //Epoch time in milliseconds: The time of event firing
        public long clientTimestamp = 0;
        //Event specific meta data
        public Dictionary<string, object> eventMetadata = null;
        //event name from doc
        public string name = null;
        public long userId = 0;
    }

    /// <summary>
    /// Static data for the entire session
    /// </summary>
    public class ClickstreamVisitData
    {
        // Only one decimal point allowed. EX: 1.2 is correct whereas 1.2.3 is wrong
        public string appVersion = null;
        public int channelId = 0;
        //Epoch time in milliseconds: The time of API firing
        public long clientTimestamp = 0;
        public Product productId = Product.None;
        public long userId = 0;
        //Firebase token for FCM
        public string deviceId = null;
        public string manufacturersamsung;
        public string model = null;
        public string networkOp = null;
        public string networkType = null;
        public string osName = null;
        public string osVersion = null;
        public string refCode = null;
        public string refURL = null;
        public string serial = null;
        public string utmCampaign = null;
        public string utmContent = null;
        public string utmMedium = null;
        public string utmSource = null;
        public string utmTerm = null;
        public string googleAddId = null;
        public string id = null;
        public string sessionId = null;
        public string uid = null;
        public string creativeId = null;
        public string partnerId = null;
        public string providerId = null;
    }
}
