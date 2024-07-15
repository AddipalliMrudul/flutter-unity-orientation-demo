using XcelerateGames.IOC;

namespace XcelerateGames.Notifications.Local
{
    public class SigShowNotification : Signal<NotificationData> { }
    public class SigOnNotificationClick : Signal<string> { }
}
