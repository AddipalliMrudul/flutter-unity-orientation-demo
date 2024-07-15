using System;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Debugging
{
    public static class DebugEmail
    {
#if UNITY_ANDROID
        #region Androids
        private static string mAndroidNetUri = "android.net.Uri";
        private static string mAndroidContentIntent = "android.content.Intent";
        private static string mExtraEmail = "android.intent.extra.EMAIL";
        private static string mExtraSubject = "android.intent.extra.SUBJECT";
        private static string mExtraText = "android.intent.extra.TEXT";
        private static string mExtraCc = "android.intent.extra.CC";
        private static string mExtraBcc = "android.intent.extra.BCC";
        private static string mExtraStream = "android.intent.extra.STREAM";
        private static string mAndroidOsEnvironment = "android.os.Environment";
        private static string mJavaIoFile = "java.io.File";
        private static string mMainActivity = "com.unity3d.player.UnityPlayer";
        
        private static string mURIPermissionFlag = "FLAG_GRANT_READ_URI_PERMISSION";
        private static string mAndroidSend = "ACTION_SEND";

        public static void Init(string mainActivity = null)
        {
            if (!mainActivity.IsNullOrEmpty())
            {
                mMainActivity = mainActivity;
#if !LIVE_BUILD && !BETA_BUILD
                XDebug.Log($"DebugEmail:Using activity: {mMainActivity}");
#endif
            }
        }

        public static void Send(string supportEmailId, string subject, string message, bool takeScreenShot, Dictionary<string, string> meta = null, string[] cc = null, string[] bcc = null)
        {
            if (Application.isEditor)
            {
                Debug.Log("this feature works on Android device only");
                return;
            }
            try
            {
                Texture2D screenShot = takeScreenShot ? ScreenCapture.CaptureScreenshotAsTexture() : null;
                var recepients = new[] { supportEmailId };
                var uriClass = new AndroidJavaClass(mAndroidNetUri);
                AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "mailto:");
                AndroidJavaObject intent = new AndroidJavaObject(mAndroidContentIntent);

                AndroidJavaClass intentClass = new AndroidJavaClass(mAndroidContentIntent);

                var unityPlayer = new AndroidJavaClass(mMainActivity);
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                object[] providerParams = new object[3];
                providerParams[0] = activity;
                providerParams[1] = "com.jg.unityclient.provider";

                AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");

                if (meta != null)
                {
                    message += "\n\n-----------------------------Meta Data------------------------------------\n";
                    message += meta.Printable();
                }

                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>(mAndroidSend));

                intent.Call<AndroidJavaObject>("setData", uri);
                intent.Call<AndroidJavaObject>("putExtra", mExtraEmail, recepients);
                intent.Call<AndroidJavaObject>("putExtra", mExtraSubject, subject);
                intent.Call<AndroidJavaObject>("putExtra", mExtraText, message);

                intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>(mURIPermissionFlag));

                intent.Call<AndroidJavaObject>("setFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK"));

                if (cc != null)
                {
                    intent.Call<AndroidJavaObject>("putExtra", mExtraCc, cc);
                }
                if (bcc != null)
                {
                    intent.Call<AndroidJavaObject>("putExtra", mExtraBcc, bcc);
                }
                if (screenShot != null)
                {
                    byte[] encoded = screenShot.EncodeToPNG();
                    string fileName = "Screenshot.png";
                    var env = new AndroidJavaClass(mAndroidOsEnvironment);
                    var picsDirectory = env.GetStatic<string>("DIRECTORY_PICTURES");
                    var pathToSave = Application.persistentDataPath;
                    var filePath = System.IO.Path.Combine(pathToSave, fileName);
                    System.IO.File.WriteAllBytes(filePath, encoded);

                    intent.Call<AndroidJavaObject>("setType", "image/*");
                    providerParams[2] = new AndroidJavaObject(mJavaIoFile, filePath);

                    Debug.Log($"Attching file screenshot");
                }
                else if (takeScreenShot && screenShot == null)
                    XDebug.LogError("Failed to attach screenshot to email");
                if (!takeScreenShot && !LogConsole.FilePath.IsNullOrEmpty())
                {
                    intent.Call<AndroidJavaObject>("setType", "text/plain");
                    providerParams[2] = new AndroidJavaObject(mJavaIoFile, LogConsole.FilePath);

                    Debug.Log($"Attching log file");
                }
                else
                    Debug.LogWarning("Log file path is empty");

                AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", providerParams);
                intent.Call<AndroidJavaObject>("putExtra", mExtraStream, uriObject);

                intent.Call<AndroidJavaObject>("setType", "message/rfc822");
                intent.Call<AndroidJavaObject>("setPackage", "com.google.android.gm");
                activity.Call("startActivity", intent);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion Android
#elif UNITY_IOS
        #region iOS
        public static void Init(string mainActivity = null)
        {

        }

        public static void Send(string supportEmailId, string subject, string message, bool takeScreenShot, Dictionary<string, string> meta = null, string[] cc = null, string[] bcc = null)
        {
            if (Application.isEditor)
            {
                Debug.Log("this feature not implemented for iOS yet");
                return;
            }
        }
        #endregion iOS
#else
        public static void Init(string mainActivity = null)
        {

        }

        public static void Send(string supportEmailId, string subject, string message, bool takeScreenShot, Dictionary<string, string> meta = null, string[] cc = null, string[] bcc = null)
        {
            if (Application.isEditor)
            {
                Debug.Log("this feature not implemented for this platform yet");
                return;
            }
        }
#endif
    }
}