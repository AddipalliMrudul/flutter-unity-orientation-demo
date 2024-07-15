
using System;

namespace XcelerateGames.Notifications.Local
{
    public class NotificationData
    {
        public const string DefaultChannel = "game_channel0";
        public const string ReminderChannel = "reminder_channel1";


        public string Title;
        public string Body;
        public string Channel;
        public DateTime DeliveryTime;
        public string Data;

        public NotificationData()
        {

        }

        public NotificationData(string titile, string body, DateTime dateTime, string channel = DefaultChannel,string data = null)
        {
            Title = titile;
            Body = body;
            Channel = channel;
            Data = data ;
            DeliveryTime = dateTime;
        }

        public NotificationData(string titile, string body, int delayInMins, string channel = DefaultChannel, string data = null)
        {
            Title = titile;
            Body = body;
            Channel = channel;
            Data = data;
            DeliveryTime = DateTime.Now.AddMinutes(delayInMins);
        }

        public NotificationData(string titile, string body, long delayInSeconds, string channel = DefaultChannel, string data = null)
        {
            Title = titile;
            Body = body;
            Channel = channel;
            Data = data;
            DeliveryTime = DateTime.Now.AddSeconds(delayInSeconds);
        }
    }
}
