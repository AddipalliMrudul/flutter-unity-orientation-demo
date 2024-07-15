#if USE_FIREBASE
using System.Collections.Generic;
using Firebase.Messaging;
using Newtonsoft.Json;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class FirebasePushNotificationManager : BaseBehaviour
    {
        [InjectSignal] private SigFirebaseInitialized mSigFirebaseInitialized = null;
        [InjectSignal] private SigFirebaseNotificationTokenReceived mSigFirebaseNotificationTokenReceived = null;
        [InjectSignal] private SigFirebaseNotificationMessageReceived mSigFirebaseNotificationMessageReceived = null;

        [InjectModel] private MiscellaneousModel mMiscellaneousModel;
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
            if (XDebug.CanLog(XDebug.Mask.Notifications))
                XDebug.Log("FirebasePushNotificationManager", XDebug.Mask.Notifications);
            mSigFirebaseInitialized.AddListener(OnFirebaseInitialized);
        }

        protected override void OnDestroy()
        {
            mSigFirebaseInitialized.RemoveListener(OnFirebaseInitialized);
            base.OnDestroy();
        }

        private void OnFirebaseInitialized(bool success)
        {
            if (XDebug.CanLog(XDebug.Mask.Notifications))
                XDebug.Log("Notifications OnFirebaseInitialized?: " + success, XDebug.Mask.Notifications);
            if (success)
            {
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
            }
        }

        protected virtual void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            if (XDebug.CanLog(XDebug.Mask.Notifications))
                XDebug.Log("Notifications OnTokenReceived: " + token.Token, XDebug.Mask.Notifications);
            mMiscellaneousModel.pushNotificationToken = token.Token;
            PlayerPrefs.SetString("push_notification_token", token.Token);
            PlayerPrefs.Save();
            mSigFirebaseNotificationTokenReceived.Dispatch(token.Token);
        }

        protected virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var item in e.Message.Data)
            {
                if (!item.Value.IsNullOrEmpty())
                {
                    data.Add(item.Key, item.Value);
                }
            }
            if (XDebug.CanLog(XDebug.Mask.Notifications))
            {
                XDebug.Log($"Notifications OnMessageReceived \nData: {JsonConvert.SerializeObject(data)}" +
                $"\nNotification: {e.Message.Notification.ToJson()}", XDebug.Mask.Notifications);
            }
            mSigFirebaseNotificationMessageReceived.Dispatch(data);
        }
    }
}
#endif