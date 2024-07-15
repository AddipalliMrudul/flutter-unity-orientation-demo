using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XcelerateGames;

namespace JungleeGames.WebServices
{
    public class UploadDeviceLogs : BaseBehaviour
    {
#if DEV_BUILD || QA_BUILD
        [SerializeField] string _URL = "https://unity-asset-uploader-inc-hzt-qa-1.howzatfantasy.com/UploadDeviceLogsMP";
#else
        [SerializeField] string _URL = "https://unity-asset-uploader-hzt-prod.howzat.com/UploadDeviceLogsMP";
#endif
        //[SerializeField] string _URL = "http://127.0.0.1:5001/UploadDeviceLogsMP";

        private readonly string[] RequiredKeys = { "UserId", "Game", "FileName", "Version" };

        //Callback, true is successful else false
        protected Action<bool> mCallback = null;

        public void SetUrl(string url)
        {
            if (url.IsNullOrEmpty())
                XDebug.LogException("Trying to set null/empty url");
            else
                _URL = url;
        }

        public virtual bool StartUpload(Dictionary<string, string> data, Action<bool> callback = null, bool useLogFile = false)
        {
            if (!ContainsAllRequiredKeys(data))
            {
                XDebug.LogError($"One or more Required key(s) are missing. Passed Data: {data.Printable()}");
                return false;
            }
            mCallback = callback;
            StartCoroutine(Upload(data, useLogFile));
            return true;
        }

        protected virtual bool ContainsAllRequiredKeys(Dictionary<string, string> data)
        {
            return data.ContainsKeys(RequiredKeys);
        }

        /// <summary>
        /// Starts uploading the logs to server via API call
        /// </summary>
        /// <param name="data">data keys & values</param>
        /// <param name="useLogFile">if true, log file created by LogConsole class will be uploaded, else all logs added via XDebug.* will be uploaded</param>
        /// <returns></returns>
        IEnumerator Upload(Dictionary<string, string> data, bool useLogFile)
        {
            Debug.Log($"Uploading logs: {data.Printable()}");
            byte[] logs = null;
#if IN_MEMORY_LOGS
            if (useLogFile)
                logs = LogConsole.GetLogs();
            else
                logs = System.Text.Encoding.UTF8.GetBytes(XDebug.GetLogString());
#else
                logs = LogConsole.GetLogs();
#endif //IN_MEMORY_LOGS
            bool uploaded = false;
            if (logs != null && logs.Length > 0)
            {
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormFileSection("file", logs, "DeviceLogs.json", "text/plain"));

                foreach (KeyValuePair<string, string> kvp in data)
                {
                    formData.Add(new MultipartFormDataSection(kvp.Key, kvp.Value));
                }

                UnityWebRequest www = UnityWebRequest.Post(_URL, formData);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    XDebug.LogError($"Failed to upload device logs, Error: {www.error}, URL:{www.url}");
                }
                else
                {
                    try
                    {
                        WSUploadDeviceLogs.Response response = www.downloadHandler.text.FromJson<WSUploadDeviceLogs.Response>();
                        uploaded = response.success;
                        if (uploaded)
                        {
                            Debug.Log($"Device logs Uploaded!: {www.downloadHandler.text}");
                        }
                        else
                            XDebug.LogError($"Failed to upload log file to server, Error: {response.error}");
                    }
                    catch (Exception ex)
                    {
                        XDebug.LogError($"Failed to parse response of log uploader: {www.downloadHandler.text}\nMessage:{ex.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Log file not found to upload.");
            }
            mCallback?.Invoke(uploaded);

            Destroy(gameObject);
        }
    }
}