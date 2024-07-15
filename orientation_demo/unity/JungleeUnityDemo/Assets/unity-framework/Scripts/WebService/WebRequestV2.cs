using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using XcelerateGames.Cryptography;
using XcelerateGames.Pooling;
using XcelerateGames.Timer;

public delegate void WebServiceEventHandler(XcelerateGames.WebServices.WebServiceEvent inEvent, string inData, object inUserData, Dictionary<string, string> responseHeaders);

namespace XcelerateGames.WebServices
{
    public enum WebRequestType
    {
        POST,
        GET,
        PUT,
        DELETE
    }

    public class WebRequestV2 : MonoBehaviour
    {
        public static bool pUseEncryption = false;

        #region Member variables

        protected WebServiceEventHandler mCallbacks = null;

        protected WebRequestType mRequestType = WebRequestType.GET;
        protected object mInternalPayload = null;

        protected double mRequestTimeStamp;
        protected int mRetryCount = 0;
        protected int mMaxRetries = 3;
        protected bool mInternalTimeOut = false;
        protected bool mCanBeKilled = true;
        protected bool mUseCompression = false;
        protected bool mSkipResponseProcessing = false;

        protected APIConfig mConfig = null;

        #endregion Member variables

        #region Getters & Setters

        #endregion Getters & Setters

        #region Static/Const member variables

        protected WebRequestData mWebRequestData = null;
        public static Dictionary<string, object> mMetaData;
        protected static Pool mPool = null;
        protected static Dictionary<string, Pool> mPoolDict = new Dictionary<string, Pool>();
        public static bool IsInitialized => mPool != null;

        public bool pCanBeKilled
        {
            get { return mCanBeKilled; }
            set { mCanBeKilled = value; }
        }

        #endregion Static/Const member variables

        #region static methods

        public static void Init()
        {
            GenerateMeta();

            WebRequestSettings.Init();
            CreatePool(typeof(WebRequestV2));
        }

        public static void DeInit()
        {
            if (XDebug.CanLog(XDebug.Mask.Game))
                Debug.Log("WebRequestV2 DeInit called");
            if (mPool != null)
            {
                PoolManager.ReleasePool(mPool);
            }
        }

        public static void GenerateMeta(int userId = -1, string onesignalUserId = null, string oneSignalPushToken = null)
        {
            mMetaData = new Dictionary<string, object>();
            mMetaData.Add("platform", PlatformUtilities.GetCurrentPlatform().ToString());
            mMetaData.Add("device", PlatformUtilities.GetDeviceTypeString());
            mMetaData.Add("device_version", SystemInfo.deviceModel);
            mMetaData.Add("device_os", SystemInfo.operatingSystem);
            mMetaData.Add("app_version", ProductSettings.GetProductVersion());
            mMetaData.Add("build_number", ProductSettings.pInstance._BuildNumber);
            mMetaData.Add("env", PlatformUtilities.GetEnvironment().ToString());
            mMetaData.Add("app", ProductSettings.pInstance._AppName);
            mMetaData.Add("time", WebService.pTicks);
            mMetaData.Add("uuid", Utilities.GetUniqueID());
        }

        public static void AddOrUpdateMeta(string key, object value)
        {
            mMetaData[key] = value;
        }

        protected static void CreatePool(Type type)
        {
            GameObject webRequestTemplate = new GameObject("WebRequestObj");
            webRequestTemplate.name = type.ToString();
            webRequestTemplate.AddComponent(type);
            mPool = PoolManager.CreatePool("WebRequests", WebRequestSettings.pInstance ? WebRequestSettings.pInstance._PoolSize : 10, webRequestTemplate, true);
        }

        protected static void CreatePool<T>() where T : WebRequestV2
        {
            GameObject webRequestTemplate = new GameObject("WebRequestObj");
            webRequestTemplate.name = typeof(T).ToString();
            webRequestTemplate.AddComponent<T>();
            mPoolDict.Add(webRequestTemplate.name, PoolManager.CreatePool(webRequestTemplate.name, WebRequestSettings.pInstance ? WebRequestSettings.pInstance._PoolSize : 10, webRequestTemplate, true));
        }

        /// <summary>
        /// Returns an instance of WebRequest class from pool.
        /// </summary>
        /// <returns>WebRequest instance from pool</returns>
        public static WebRequestV2 Create(string endPoint, WSParams wsParams, WebServiceEventHandler callbacks, object internalPayload, bool useCompression, bool skipResponseProcessing = false, Dictionary<string, string> requestHeaders = null)
        {
            WebRequestV2 request = mPool.Spawn().GetComponent<WebRequestV2>();
            request.Init(endPoint, wsParams, requestHeaders, callbacks, internalPayload, useCompression, skipResponseProcessing);
            return request;
        }

        public static T Create<T>(string endPoint, WSParams wsParams, WebServiceEventHandler callbacks, object internalPayload, bool useCompression, bool skipResponseProcessing = false, Dictionary<string, string> requestHeaders = null) where T : WebRequestV2
        {
            T request = mPoolDict[typeof(T).ToString()].Spawn().GetComponent<T>();
            request.Init(endPoint, wsParams, requestHeaders, callbacks, internalPayload, useCompression, skipResponseProcessing);
            return request;
        }
        #endregion
        #region Public Methods & Constructor

        /// <summary>
        /// Builds the WebRequest object.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="inEvent"></param>
        /// <param name="inUserData"></param>
        public virtual void Init(string endPoint, WSParams wsParams, Dictionary<string, string> requestHeaders, WebServiceEventHandler callbacks, object internalPayload, bool useCompression, bool skipResponseProcessing)
        {
            mConfig = WebRequestSettings.GetConfigData(endPoint);
            if (mConfig != null)
            {
                mMaxRetries = mConfig.config.maxRetries;
            }
            //if(callbacks != null)
            //    Debug.Log($"{endPoint}:->{callbacks.GetInvocationList()[0].Target}::{callbacks.GetInvocationList()[0].Method}");
            mWebRequestData = new WebRequestData();
            mWebRequestData.api = endPoint;
            string encryptedData = wsParams == null ? null : wsParams.ToJson();
            if (pUseEncryption)
                encryptedData = CryptoUtilities.EncryptOrDecrypt(wsParams.ToJson());
            mWebRequestData.data = encryptedData;
            string ticks = WebService.pTicks.ToString();
            mWebRequestData.ticks = ticks;
            mWebRequestData.secure = CryptoUtilities.GetMd5Hash(Utilities.GetUniqueID(), ticks, encryptedData);

            mWebRequestData.requestHeaders = requestHeaders;
            transform.SetParent(null);
            gameObject.SetActive(true);

            //if(mCallbacks != null)
            //    Debug.LogError($"{endPoint}:{mCallbacks.GetInvocationList()[0].Method} assigning to {callbacks.GetInvocationList()[0].Method}");
            if (callbacks != null)
                mCallbacks = callbacks;
            mInternalPayload = internalPayload;
            mUseCompression = useCompression;
            mSkipResponseProcessing = skipResponseProcessing;
        }

        /// <summary>
        /// Kill the webservice call
        /// </summary>
        public virtual bool Kill()
        {
            if (mCanBeKilled)
                Dispose();
            return mCanBeKilled;
        }

        /// <summary>
        /// Kill the webservice call
        /// </summary>
        public virtual void ClearCallBack()
        {
            mCallbacks = null;
        }

        protected virtual string GetRequestData()
        {
            return mWebRequestData.ToJson();
        }

        public IEnumerator Execute(WebRequestType requestType)
        {
            mRequestType = requestType;
            string json = GetRequestData();

            UnityWebRequest request = null;
            mWebRequestData.meta = mMetaData.ToJson();
            if (mRequestType == WebRequestType.POST)
            {
#if UNITY_2022_3
                request = UnityWebRequest.PostWwwForm(mWebRequestData.api, json);
#else
                request = UnityWebRequest.Post(mWebRequestData.api, json);
#endif
            }
            else if (mRequestType == WebRequestType.PUT)
                request = UnityWebRequest.Put(mWebRequestData.api, json);
            else if (mRequestType == WebRequestType.GET)
                request = UnityWebRequest.Get(mWebRequestData.api);
            else if (mRequestType == WebRequestType.DELETE)
                request = UnityWebRequest.Delete(mWebRequestData.api);

            UnityWebRequest.ClearCookieCache();

            if (json.IsNullOrEmpty())
                request.uploadHandler = new UploadHandlerRaw(null);
            else
                request.uploadHandler = new UploadHandlerRaw(mUseCompression ? json.Zip() : System.Text.Encoding.UTF8.GetBytes(json));

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("secure", mWebRequestData.secure);
            request.SetRequestHeader("ticks", mWebRequestData.ticks);
            if (mWebRequestData.requestHeaders != null)
            {
                foreach (var item in mWebRequestData.requestHeaders)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }
            }
            if (mUseCompression)
                request.SetRequestHeader("Content-Encoding", "gzip");
            AddHeaders(request);

            if (XDebug.CanLog(XDebug.Mask.WebService))
            {
                string data = $"Sending WebRequest to API : {mWebRequestData.api} \nData : \"{json}\" \nheaders: \n{mWebRequestData.requestHeaders.Printable()}";
                XDebug.Log(data, XDebug.Mask.WebService);
                if (mUseCompression)
                    XDebug.Log($"API : {mWebRequestData.api} \nUncompressed size : {Utilities.FormatBytes(json.Length)}, Compressed size: {Utilities.FormatBytes(json.Zip().Length)}", XDebug.Mask.WebService);
            }

            mRequestTimeStamp = Time.timeSinceLevelLoadAsDouble;
            bool isError = false;
            bool offline = false;
#if DEV_BUILD || QA_BUILD
            if (mConfig != null && WebRequestSettings.pInstance._SimulationEnabled && mConfig.simulation != null && mConfig.simulation.APIErrorType == APIErrorType.BeforeHittingServer)
            {
                isError = UnityEngine.Random.Range(1, 100) < mConfig.simulation.failProbability;
            }
#endif
            if (!isError)
            {
                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                    mCallbacks?.Invoke(WebServiceEvent.PROGRESS, null, request.uploadProgress, null);
                    yield return request;
#else
                    if (!ConnectivityMonitor.pIsInternetAvailable)
                    {
                        offline = true;
                        break;
                    }
                    else
                    {
                        mCallbacks?.Invoke(WebServiceEvent.PROGRESS, null, request.uploadProgress, null);
                        yield return request;
                    }
#endif
                }
            }

#if DEV_BUILD || QA_BUILD
            if (mConfig != null && mConfig.simulation != null)
                yield return new WaitForSeconds(UnityEngine.Random.Range(0, mConfig.simulation.delay));
#endif
            OnRequestComplete(request, offline, isError);
            request.Dispose();
        }

        #endregion Public Methods & Constructor

        #region Private Methods

        /// <summary>
        /// Delete the object & free the resources.
        /// </summary>
        protected virtual void Dispose()
        {
            mCallbacks = null;
            mInternalPayload = null;

            mPool.Despawn(gameObject);
        }

        protected virtual void AddHeaders(UnityWebRequest request)
        {

        }

        protected virtual void OnRequestComplete(UnityWebRequest request, bool isOffline, bool isError = false)
        {
            if (XDebug.CanLog(XDebug.Mask.WebService))
                XDebug.Log($"WebService Complete : API : {request.url} \nData : \"{request.downloadHandler?.text}\", responseCode: \"{request.responseCode}\", result: \"{request.result}\" ", XDebug.Mask.WebService);

            bool dispose = false;
#if DEV_BUILD || QA_BUILD
            if (mConfig != null && WebRequestSettings.pInstance._SimulationEnabled && mConfig.simulation != null && mConfig.simulation.APIErrorType == APIErrorType.AfterHittingServer)
            {
                isError = UnityEngine.Random.Range(1, 100) < mConfig.simulation.failProbability;
            }
#endif
            if (isError)
                Debug.LogWarning($"Simulating WebRequest Error for: {request.url}");
            if (request.responseCode == 0)
                isError = true;
            if (request.result == UnityWebRequest.Result.Success && !isError && !isOffline)
            {
                if (request.responseCode >= 400)
                {
                    //This means Unity itself bumped into an error : Ex: network drop, bad gateway
                    ProcessError(request);
                }
                else
                {
                    ProcessResponse(request.downloadHandler?.text, request.GetResponseHeaders());
                }
                dispose = true;
            }
            else
            {
                if (isOffline || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    if (XDebug.CanLog(XDebug.Mask.WebService))
                        XDebug.Log($"Offline while processing : API : {request.url}, isOffline?: {isOffline}, Result: {request.result}", XDebug.Mask.WebService);
                    mCallbacks?.Invoke(WebServiceEvent.OFFLINE, null, mInternalPayload, null);
                    dispose = true;
                    //ConnectivityMonitor.AddListener(OnNetworkStatusChanged);
                }
                else
                {
                    if (mInternalTimeOut || mRetryCount >= mMaxRetries)
                    {
                        ProcessError(request);
                        dispose = true;
                    }
                    else
                    {
                        mRetryCount++;
                        if (XDebug.CanLog(XDebug.Mask.WebService))
                            XDebug.Log($"Warning! WebService Failed : {request.url} Retrying {mRetryCount}", XDebug.Mask.WebService);
                        StopAllCoroutines();
                        StartCoroutine(Execute(mRequestType));
                    }
                }
            }
            if (dispose)
                Dispose();
        }

        //private void OnNetworkStatusChanged(ConnectivityMonitor.Status status)
        //{
        //    if(status == ConnectivityMonitor.Status.Online)
        //    {
        //        StopAllCoroutines();
        //        StartCoroutine(Execute(mRequestType));
        //    }
        //}

        protected virtual void ProcessResponse(string response, Dictionary<string, string> resposeHeaders)
        {
            try
            {
                if (mSkipResponseProcessing)
                {
                    CallCompleteCallback(response, resposeHeaders);
                }
                else
                {
                    JObject obj = JObject.Parse(response);
                    JToken token = null;
                    //Every API response returns server time under meta. use it to initialze server time.
                    if (obj.TryGetValue("meta", out token))
                    {
                        InitServerTime(obj["meta"]["ts"].Value<long>());
                    }
                    else if (obj.TryGetValue("ts", out token))
                    {
                        InitServerTime(token.Value<long>());
                    }

                    void InitServerTime(long ts)
                    {
                        double latency = (Time.timeSinceLevelLoadAsDouble - mRequestTimeStamp) / 2f;
                        ServerTime.Init(ts + latency);
                    }

                    string decryptedData = null;
                    if (obj.TryGetValue("data", out token))
                    {
                        decryptedData = obj["data"].ToString();
                    }
                    else
                        decryptedData = response;
                    if (pUseEncryption)
                        decryptedData = CryptoUtilities.EncryptOrDecrypt(decryptedData);
                    if (XDebug.CanLog(XDebug.Mask.WebService))
                        XDebug.Log($"Decrypted : {decryptedData}", XDebug.Mask.WebService);

                    CallCompleteCallback(decryptedData, resposeHeaders);
                }
            }
            catch (Exception e)
            {
                XDebug.LogException($"Failed to deserialize server response: Exception: {e.Message}, response: {response}");
                Debug.LogError(e.StackTrace);
                RaiseError("Failed to deserialize server response.", resposeHeaders);
            }
        }

        protected virtual void ProcessError(UnityWebRequest request)
        {
            string error = request.error;
            if (mInternalTimeOut)
                error = "System internal error : Timed out";
            else if (request.responseCode == 400)
            {
                try
                {
                    error = "Something went wrong";
                    string responseText = request.downloadHandler?.text;
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        JObject obj = JObject.Parse(responseText);
                        if (obj != null && obj.TryGetValue("error", out _))
                        {
                            obj = JObject.Parse(obj["error"].ToString());
                            if (obj != null && obj.TryGetValue("message", out _))
                                error = obj["message"].Value<string>();
                        }
                    }
                }
                catch (Exception e)
                {
                    XDebug.LogException($"Failed to parse response code 400 message Exception: \n {e.Message} \n url :{request.url} \n Response Data :{request.downloadHandler?.text}");
                }
            }
            if (XDebug.CanLog(XDebug.Mask.WebService))
                XDebug.LogError($"Error! WebService Failed : {request.url} Error : {error}", XDebug.Mask.WebService);
            RaiseError(error, request.GetResponseHeaders());
        }

        protected virtual void CallCompleteCallback(string response, Dictionary<string, string> resposeHeaders)
        {
            //if(mCallbacks != null)
            //    Debug.Log($"Callback-> {mWebRequestData.api}:->{mCallbacks.GetInvocationList().Length} {mCallbacks.GetInvocationList()[0].Target}::{mCallbacks.GetInvocationList()[0].Method} : {response}");

            mCallbacks?.Invoke(WebServiceEvent.COMPLETE, response, mInternalPayload, resposeHeaders);
        }

        protected virtual void RaiseError(object reason, Dictionary<string, string> resposeHeaders)
        {
            bool errorHandled = false;
            if (mCallbacks != null)
            {
                errorHandled = true;
                string r = null;
                if (reason != null)
                    r = reason.ToString();
                mCallbacks.Invoke(WebServiceEvent.ERROR, r, mInternalPayload, resposeHeaders);
            }
            //
            if (!errorHandled)
            {
                /**
                 * Probably show a reload popup.
                 **/
                //UiDialogBox.Show("loading_failed_msg", "error_title", UiDialogBox.DialogBoxType.OKAY);
                XDebug.LogException("Failed to load data from server");
            }
        }

        #endregion Private Methods

        //        #region Editor Only
        //#if UNITY_EDITOR
        //        #region Network Fail
        //        private static float mSimulateNetworkFail = -1;
        //        private const string mSimulateNetworkFailKey = "-SimulateNetworkFail-";

        //        public static float pSimulateNetworkFail
        //        {
        //            get
        //            {
        //                if (mSimulateNetworkFail == -1)
        //                    mSimulateNetworkFail = EditorPrefs.GetFloat(mSimulateNetworkFailKey, 0f);

        //                return mSimulateNetworkFail;
        //            }
        //            set
        //            {
        //                if (value != mSimulateNetworkFail)
        //                {
        //                    mSimulateNetworkFail = value;
        //                    EditorPrefs.SetFloat(mSimulateNetworkFailKey, value);
        //                }
        //            }
        //        }
        //        #endregion Network Fail
        //        #region Network Delay
        //        private static int mSimulateNetworkDelay = -1;
        //        private const string mSimulateNetworkDelayKey = "-SimulateNetworkDelay-";

        //        public static bool pSimulateNetworkDelay
        //        {
        //            get
        //            {
        //                if (mSimulateNetworkDelay == -1)
        //                    mSimulateNetworkDelay = EditorPrefs.GetBool(mSimulateNetworkDelayKey, false) ? 1 : 0;
        //                ;

        //                return mSimulateNetworkDelay != 0;
        //            }
        //            set
        //            {
        //                int newValue = value ? 1 : 0;
        //                if (newValue != mSimulateNetworkDelay)
        //                {
        //                    mSimulateNetworkDelay = newValue;
        //                    EditorPrefs.SetBool(mSimulateNetworkDelayKey, value);
        //                }
        //            }
        //        }
        //        #endregion Network Delay

        //#endif
        //        #endregion Editor Only
    }
}