using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace XcelerateGames.RemoteLogging
{
    public class LogDNA : RemoteLoggingBase
    {
        public string _url = "https://logs.logdna.com/logs/ingest?";
        public string _host = null;
        public string _username = null;
        public string _password = null;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            _host += PlatformUtilities.GetEnvironment();
        }

        // Update is called once per frame
        protected override void PostLog(string logString, string stackTrace, LogType logType)
        {
            StartCoroutine(PostToLoggly(logString, stackTrace, logType));
        }

        private IEnumerator PostToLoggly(string logString, string stackTrace, LogType inType)
        {
            WWWForm form = new WWWForm();

            AddCommonData(form);
            form.AddField("Message", logString);
            form.AddField("hostname", _host);
            form.AddField("NetWorkType", Application.internetReachability.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(_url, form))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("charset","UTF-8");
                www.SetRequestHeader("Authorization", authenticate(_username, _password));

                UnityWebRequestAsyncOperation asyncOperation = www.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    yield return www;
                }

#if UNITY_2020_2_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    //Debug.LogError(www.error);
                }
                else
                {
                    //Debug.Log("Form upload complete!");
                }
            }
        }

        string authenticate(string username, string password)
        {
            string auth = username + ":" /*+ password*/;
            auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
            auth = "Bearer " + username;
            return auth;
        }
    }
}
