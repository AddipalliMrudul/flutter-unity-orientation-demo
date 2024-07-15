using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XcelerateGames.Cryptography;
using XcelerateGames.Timer;
#pragma warning disable 618
//public delegate void WebServiceEventHandler(WebServiceEvent inEvent, string inData, object inUserData);

namespace XcelerateGames.WebServices
{
    //public enum WebRequestType
    //{
    //    POST,
    //    GET
    //}

    public class WebRequestProcessor : MonoBehaviour
    {
        private List<WebRequest> mRequests = new List<WebRequest>();
        private static WebRequestProcessor mInstance = null;

        public static void Init()
        {
            if (mInstance == null)
            {
                mInstance = new GameObject("WebRequestProcessor").AddComponent<WebRequestProcessor>();
                mInstance.gameObject.isStatic = true;
                DontDestroyOnLoad(mInstance.gameObject);
            }
        }

        public static void AddRequest(WebRequest request)
        {
            mInstance.mRequests.Add(request);
        }

        /// <summary>
        /// Kills all calls that are being made, This function should be called before doing a scene transition, else will cause null exceptions.
        /// If you do not want a call to be killed, make mCanBeKilled = false
        /// </summary>
        public static void KillAll()
        {
            if (mInstance != null)
                mInstance.mRequests.RemoveAll(e => e.Kill());
        }

        private void Update()
        {
            for (int i = 0; i < mRequests.Count; ++i)
            {
                WebRequest webRequest = mRequests[i];
                if (webRequest.IsDone())
                {
                    webRequest.OnRequestComplete();
                    if (i >= mRequests.Count)
                        XDebug.LogWarning("Trying to remove an element that does not exist : " + mRequests.Count + " " + i);
                    else
                        mRequests.RemoveAt(i);
                    i--;
                }
                else
                {
                    webRequest.UpdateProgress();
                }
            }
        }
    }

    public class WebRequest : IDisposable
    {
        #region Member variables

        private string mURL = null;
        private string mData = "{}";
        private WebServiceEventHandler mEvent = null;
        private WWW mWWWObj = null;
        private WebRequestType mRequestType = WebRequestType.GET;
        private object mUserData = null;
        private Dictionary<string, string> mHeaders = null;
        private static string mMeta = null;
        private int mRetryCount = 0;
        private int mMaxRetries = 0;
        private float mTimer = 180f * 2;
        private bool mInternalTimeOut = false;
        private bool mCanBeKilled = true;

        public WWW WWWObj => mWWWObj;

        #endregion Member variables

        #region Static/Const member variables

        private static string mCurrentPlatform = null;
        public const string mUUIDKey = "uuid";
        public const string mContentTypeKey = "Content-Type";
        public const string mDataKey = "data";
        public const string mMetaKey = "meta";
        public const string mTimerKey = "ticks";
        public const string mPlatformKey = "platform";
        public const string mSecureKey = "secure";
        public const string mTokenKey = "token";
        public const string mAppVersion = "AppVersion";

        public Dictionary<string, string> pHeaders
        {
            get
            {
                if (mHeaders == null)
                    mHeaders = new Dictionary<string, string>();
                return mHeaders;
            }
        }

        public bool pCanBeKilled
        {
            get { return mCanBeKilled; }
            set { mCanBeKilled = value; }
        }

        public void RemoveAllListeners()
        {
            mEvent = null;
        }

        #endregion Static/Const member variables

        #region Public Methods & Constructor

        static WebRequest()
        {
            if (string.IsNullOrEmpty(mCurrentPlatform))
                mCurrentPlatform = PlatformUtilities.GetCurrentPlatform().ToString();
            GenerateMeta();
        }

        public static void GenerateMeta(int userId = -1, string onesignalUserId = null, string oneSignalPushToken = null)
        {
            Dictionary<string, string> meta = new Dictionary<string, string>();
            meta.Add("uuid", Utilities.GetUniqueID());
            meta.Add("platform", PlatformUtilities.GetCurrentPlatform().ToString());
            meta.Add("app_version", ProductSettings.GetProductVersion());
            meta.Add("app_version_major", ProductSettings.GetProductVersion(0).ToString());
            meta.Add("app_version_minor", ProductSettings.GetProductVersion(1).ToString());
            meta.Add("build_number", ProductSettings.pInstance._BuildNumber);
            meta.Add("env", PlatformUtilities.GetEnvironment().ToString());
            meta.Add("app", ProductSettings.pInstance._AppName);
            meta.Add("device_os", SystemInfo.operatingSystem);
            meta.Add("device_model", SystemInfo.deviceModel);

            if (userId >= 0)
                meta.Add("user_id", userId.ToString());
            if (!onesignalUserId.IsNullOrEmpty())
                meta.Add("one_signal_push_user_id", onesignalUserId);
            if (!oneSignalPushToken.IsNullOrEmpty())
                meta.Add("one_signal_push_token", oneSignalPushToken);
            mMeta = meta.ToJson();
        }

        /// <summary>
        /// Builds the WebRequest object.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="inEvent"></param>
        /// <param name="inUserData"></param>
        public WebRequest(string url, WebServiceEventHandler inEvent, object inUserData)
        {
            mURL = url;
            if (inEvent != null)
                mEvent += inEvent;
            mUserData = inUserData;
        }

        /// <summary>
        /// Add additional parameters to header.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        //public void AddParam(string key, string value)
        //{
        //    pHeaders.Add(key, value);
        //}

        //public void AddUUID(string uuid)
        //{
        //    pHeaders.Add(mUUIDKey, uuid);
        //}

        /// <summary>
        /// The json data that needs to be sent to the server
        /// </summary>
        /// <param name="data"></param>
        public void AddData(string data)
        {
            if (!string.IsNullOrEmpty(data))
                mData = data;
            else
            {
                if (XDebug.CanLog(XDebug.Mask.WebService))
                    XDebug.LogWarning("Trying to post null/empty data.", XDebug.Mask.WebService);
            }
        }

        /// <summary>
        /// Kill the webservice call
        /// </summary>
        public bool Kill()
        {
            if (mCanBeKilled)
                Dispose();
            return mCanBeKilled;
        }

        /// <summary>
        /// Kill the webservice call
        /// </summary>
        public void ClearCallBack()
        {
            mEvent = null;
        }

        /// <summary>
        /// Delete the object & free the resources.
        /// </summary>
        public void Dispose()
        {
            mURL = null;
            mEvent = null;
            mUserData = null;
            mHeaders = null;
            mData = null;
            mWWWObj = null;
        }

        #endregion Public Methods & Constructor

        #region Private Methods

        public void Execute(WebRequestType requestType)
        {
            try
            {
                if (XDebug.CanLog(XDebug.Mask.WebService))
                    XDebug.Log(string.Format("WebService calling :-> URL : {0}, Data : {1}, Meta : {2}", mURL, mData, mMeta), XDebug.Mask.WebService);
                mRequestType = requestType;
                string ticks = WebService.pTicks.ToString();
                WWWForm form = new WWWForm();
                form.AddField(mContentTypeKey, "application/x-www-form-urlencoded");
                string encryptedData = mData;
                if (WebRequestV2.pUseEncryption)
                    encryptedData = CryptoUtilities.EncryptOrDecrypt(mData);
                form.AddField(mDataKey, encryptedData);
                form.AddField(mMetaKey, mMeta);
                Dictionary<string, string> headers = form.headers;
                headers.Add(mTimerKey, ticks);
                headers.Add(mSecureKey, CryptoUtilities.GetMd5Hash(Utilities.GetUniqueID(), ticks, encryptedData));
                headers.Add(mPlatformKey, PlatformUtilities.GetCurrentPlatform().ToString());
                headers.Add(mAppVersion, ProductSettings.GetProductVersion());

                //Add additional headers
                foreach (string key in pHeaders.Keys)
                    headers.Add(key, pHeaders[key]);
                byte[] rawData = form.data;
                if (mRequestType == WebRequestType.GET)
                    rawData = null;

                mWWWObj = new WWW(mURL, rawData, headers);
                WebRequestProcessor.AddRequest(this);
            }
            catch (Exception ex)
            {
                XDebug.LogError("Error executing : " + mURL + "\n Message : " + ex.Message);
            }
        }

        public bool IsDone()
        {
            mTimer -= Time.deltaTime;
            if (mTimer <= 0f)
            {
                string errorMsg = "Internal Time out : ";
                XDebug.LogError(errorMsg + mURL);
                //mInternalTimeOut = true;
                return true;
            }

            return mWWWObj.isDone;
        }

        public void UpdateProgress()
        {
            if (mEvent != null)
            {
                mEvent.Invoke(WebServiceEvent.PROGRESS, null, mWWWObj.uploadProgress, null);
            }
        }

        public void OnRequestComplete()
        {
            bool dispose = false;
            if (!mInternalTimeOut && string.IsNullOrEmpty(mWWWObj.error))
            {
                //if (XDebug.CanLog(XDebug.Mask.WebService))
                //    Debug.Log("WebService Complete : " + mURL + " Data : " + mWWWObj.text);
                if (mEvent != null)
                {
                    try
                    {
                        JObject responseData = JObject.Parse(mWWWObj.text);

                        //Every API response returns server time under meta. use it to initialze server time.
                        ServerTime.Init(responseData["ts"].Value<long>());

                        CGWebResponse res1 = mWWWObj.text.FromJson<CGWebResponse>();

                        if (res1 == null || res1.code != 200)
                        {
                            XDebug.LogError("WebService Error!! " + mURL + " :->" + res1.ToString());
                            mEvent?.Invoke(WebServiceEvent.ERROR, (res1 == null ? "Empty response" : res1.message), mUserData, mWWWObj.responseHeaders);
                        }
                        else
                        {
                            string decryptedData = responseData["data"].ToString();
                            if (WebRequestV2.pUseEncryption)
                                decryptedData = CryptoUtilities.EncryptOrDecrypt(decryptedData);
                            if (XDebug.CanLog(XDebug.Mask.WebService))
                                XDebug.Log($"{mURL} : {decryptedData}", XDebug.Mask.WebService);

                            mEvent?.Invoke(WebServiceEvent.COMPLETE, decryptedData, mUserData, mWWWObj.responseHeaders);
                        }
                    }
                    catch (Exception e)
                    {
                        XDebug.LogException(e);
                        mEvent?.Invoke(WebServiceEvent.ERROR, "Empty response", mUserData, null);
                    }
                }
                dispose = true;
            }
            else
            {
                if (mInternalTimeOut || mRetryCount == mMaxRetries)
                {
                    string error = mWWWObj.error;
                    if (mInternalTimeOut)
                        error = "Internal system error : Timed out";
                    XDebug.LogError("Error! WebService Failed : " + mURL + " Error : " + error, XDebug.Mask.WebService);
                    if (mEvent != null)
                        mEvent(WebServiceEvent.ERROR, error, mUserData, null);
                    dispose = true;
                }
                else
                {
                    mRetryCount++;
                    Debug.LogWarning("Warning! WebService Failed : " + mURL + ", Retrying " + mRetryCount);
                    Execute(mRequestType);
                }
            }
            if (dispose)
                Dispose();
        }

        #endregion Private Methods
    }
}
