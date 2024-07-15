#if USE_LOCAL_NOTIFICATIONS
using UnityEngine;
using NotificationSamples;
using System;
using XcelerateGames.IOC;

/* Usage : Attach this script to a GameObject (Optionally add DontDestroyOnLoad script)
 * Bind all signals in NotificationSignals file
*/


namespace XcelerateGames.Notifications.Local
{
    [RequireComponent(typeof(GameNotificationsManager))]
    public class LocalNotifications : BaseBehaviour
    {
        private GameNotificationsManager mManager = null;

        [InjectSignal] private SigShowNotification mSigShowNotification = null;
        [InjectSignal] private SigOnNotificationClick mSigOnNotificationClick = null;

        protected override void Awake()
        {
            base.Awake();

            XDebug.AddMask(XDebug.Mask.Notifications);

            mManager = GetComponent<GameNotificationsManager>();
            var c1 = new GameNotificationChannel(NotificationData.DefaultChannel, "Default Game Channel", "Generic notifications");
            var c2 = new GameNotificationChannel(NotificationData.ReminderChannel, "Reminder Channel", "Reminder notifications");

            mManager.Initialize(OnNotificationClick, c1, c2);
            mManager.DismissAllNotifications();
            mManager.LocalNotificationDelivered += OnLocalNotificationDelivered;
            mManager.LocalNotificationDeliveredMeta += OnLocalNotificationDeliveredMeta;

            //OnNotificationClick("?action=JoinTournaments&meta_data=502280&id=JoinTournamentStartRemainder");
            mSigShowNotification.AddListener(SendNotification);
            DontDestroyOnLoad(gameObject);
        }

        protected override void OnDestroy()
        {
            mSigShowNotification.RemoveListener(SendNotification);
            base.OnDestroy();
        }

        private void OnLocalNotificationDelivered(PendingNotification notification)
        {
            mSigOnNotificationClick.Dispatch(notification.Notification.Data);
        }

        private void OnLocalNotificationDeliveredMeta(string notificationData)
        {
            mSigOnNotificationClick.Dispatch(notificationData);
        }

        private void OnNotificationClick(string data)
        {
            mSigOnNotificationClick.Dispatch(data);
        }

        private void SendNotification(NotificationData data)
        {
            if (data.Data != null)
                SendNotification(data.Title, data.Body, data.DeliveryTime, data: data.Data);
            else
                SendNotification(data.Title, data.Body, data.DeliveryTime);
        }

        private void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null,
                                 bool reschedule = false, string channelId = null,
                                 string smallIcon = null, string largeIcon = null, string data = null)
        {
            IGameNotification notification = mManager.CreateNotification();

            if (notification == null)
            {
                XDebug.LogError($"Not sending notification as notification object is null, Data -> Title : {title}, Body : {body}");
                return;
            }

            notification.Title = title;
            notification.Body = body;
            notification.Group = !string.IsNullOrEmpty(channelId) ? channelId : NotificationData.DefaultChannel;
            notification.DeliveryTime = deliveryTime;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;
            if (data != null)
            {
                notification.Data = data;
            }
            if (badgeNumber != null)
            {
                notification.BadgeNumber = badgeNumber;
            }

            PendingNotification notificationToDisplay = mManager.ScheduleNotification(notification);
            notificationToDisplay.Reschedule = reschedule;
            if (XDebug.CanLog(XDebug.Mask.Notifications))
                XDebug.Log($"Scheduled notification Title : {title}, Body : {body} at {deliveryTime}",XDebug.Mask.Notifications);
        }
    }
}
#endif //USE_LOCAL_NOTIFICATIONS