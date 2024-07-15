using XcelerateGames.IOC;

namespace XcelerateGames.Notifications.Local
{
    public class LocalNotificationDataModel : XGModel
    {
        public LocalNotificationDeepLinkData localNotificationDeepLinkData = null;

        public string GetActionType()
        {
            if (localNotificationDeepLinkData == null)
                return "None";
            return localNotificationDeepLinkData.actionType;
        }
        public string GetNotificationId()
        {
            if (localNotificationDeepLinkData == null)
                return "";
            return localNotificationDeepLinkData.id;
        }
        public string GetNotificationMeta()
        {
            if (localNotificationDeepLinkData == null)
                return "";
            return localNotificationDeepLinkData.meta;
        }
    }

    public class LocalNotificationDeepLinkData
    {
        public string id;
        public string actionType;
        public string meta;
    }
}
