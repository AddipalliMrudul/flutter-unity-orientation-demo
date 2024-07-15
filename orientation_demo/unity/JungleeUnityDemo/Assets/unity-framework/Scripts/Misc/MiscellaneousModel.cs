using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class MiscellaneousModel : XGModel
    {
        public string pushNotificationToken;

        public MiscellaneousModel()
        {
            pushNotificationToken = PlayerPrefs.GetString("push_notification_token", null);
        }
    }
}
