using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XcelerateGames
{
    public class SendMail : MonoBehaviour
    {
        void Start()
        {
        }

        public IEnumerator Execute(string json)
        {
#if UNITY_2022_3
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm("https://api.sendgrid.com/v3/mail/send", json))
#else
            using (UnityWebRequest request = UnityWebRequest.Post("https://api.sendgrid.com/v3/mail/send", json))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
#if LIVE_BUILD
                request.SetRequestHeader("Authorization", "NA");
#else
                request.SetRequestHeader("Authorization", "NA");
#endif
                yield return request.SendWebRequest();
            }
        }

        public static void Init(string body)
        {
            SendMail obj = new GameObject("SendMail").AddComponent<SendMail>();
            string jsonData = GetConfig();
            if (!string.IsNullOrEmpty(jsonData))
            {
                body += "<br> Version : " + ProductSettings.GetProductVersion();
                body += "<br> Build : " + ProductSettings.pInstance._BuildNumber;
                body += "<br> Environemnt : " + PlatformUtilities.GetEnvironment();
                //body += "<br> Device Type : " + PlatformUtilities.GetDeviceType();
                body += "<br> Device Model : " + PlatformUtilities.GetDeviceModel();
                body += "<br> UUID : " + Utilities.GetUniqueID();

                jsonData = jsonData.Replace("{BODY}", body);
                XDebug.LogException($"Sending mail {jsonData}");
                obj.StartCoroutine(obj.Execute(jsonData));
            }
            else
                XDebug.LogException("Failed to load config to send mail");
        }

        private static string GetConfig()
        {
            string jsonData = RemoteSettings.GetString("app_not_loading_config");
            if (!string.IsNullOrEmpty(jsonData))
                return jsonData;
            string configFileName = "EmailConfig";
            TextAsset textAsset = Resources.Load<TextAsset>(configFileName);
            if (textAsset != null)
            {
                return textAsset.text;
            }
            else
            {
                XDebug.LogException($"Failed to load {configFileName}");
                return null;
            }
        }
    }
}
