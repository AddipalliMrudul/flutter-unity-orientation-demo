using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace XcelerateGames
{
    public enum ConnectionType
    {
        WIFI,           // User is Connected to WIFI
        MobileNetwork  // User is Connected to Mobile Network
    }

    public class ConnectivityMonitor : MonoBehaviour
    {
        public enum Status
        {
            PendingVerification,    // Internet access is being verified
            Offline,                // No internet access
            Online                  // Internet access is verified and functional
        }

        public bool _ForceInternetDown = false; //For testing purpose only.

        private float mUpdateFrequency = 5f;
        public int _MaxRetries = 2;
        //We hit both the URL`s one by one. We start with first URL(this should be pointing to CDN), If first URL fails, we hit the second URL, which is an API.
        //Both URL`s return "true" as response
        public string[] RemoteURL = null;

        private Status mStatus = Status.PendingVerification;
        private bool mIsChecking = false;
        private float mTimer = 0f;
        private int mRetryCount = 0;
        private int mIndex = 0;
        private float mRoundTripTime = 0; //to calculate network strength
        private bool mCheckRTT = false;

        private ConnectionType mConnectionType = ConnectionType.MobileNetwork; /*"Stores the connection type whether user is connected to WIFI or mobile network"*/
        private ConnectionType mLastConnectionType = ConnectionType.MobileNetwork;
        #region Static Methods

        private static Action<Status> OnInternetStateChange = null;
        private static Action<ConnectionType> OnConnectionTypeChange = null; /*"Action called everytime user switches connection type"*/
        private static ConnectivityMonitor mInstance = null;

        public static Action<float> OnGetNetworkStrength;  //to calculate network strength

        public static bool pIsInternetAvailable
        {
            get
            {
                if (mInstance != null)
                {
                    if (mInstance._ForceInternetDown)
                        return false;
                    return pStatus == Status.Online;
                }
                return true;
            }
        }

        public static ConnectionType pConnectionType /*"return the current Connection Type of user"*/
        {
            get
            {
                if (mInstance != null)
                {
                    return mInstance.mConnectionType;
                }

                return ConnectionType.WIFI;
            }
        }

        public static Status pStatus
        {
            get
            {
                if (mInstance != null)
                {
                    if (mInstance._ForceInternetDown)
                        return Status.Offline;
                    return mInstance.mStatus;
                }
                return Status.Offline;
            }
            private set
            {
                if (mInstance.mStatus != value)
                {
                    XDebug.Log($"ConnectivityMonitor: status :{value}, previous: {mInstance.mStatus}");
                    mInstance.mStatus = value;
                    if (mInstance.mStatus == Status.Online)
                        mInstance.mRetryCount = 0;

                    if (OnInternetStateChange != null)
                        OnInternetStateChange(mInstance.mStatus);
                }
            }
        }

        public static void SetInternetState(bool isDown)
        {
            if (mInstance != null)
                mInstance._ForceInternetDown = isDown;
        }

        public static void AddListener(Action<Status> callback, bool triggerCallbackOnAdd = true)
        {
            if (callback != null)
            {
                OnInternetStateChange += callback;
                if (mInstance != null)
                {
                    if (triggerCallbackOnAdd)
                        callback(pStatus);
                }
                else
                    callback(Status.PendingVerification);
            }
        }

        public static void AddConnectionTypeListner(Action<ConnectionType> callback)
        {
            if (callback != null)
            {
                OnConnectionTypeChange += callback;
            }
        }

        public static void RemoveListener(Action<Status> callback)
        {
            if (callback != null)
                OnInternetStateChange -= callback;
        }

        public static void RemoveConnectionTypeListener(Action<ConnectionType> callback)
        {
            if (callback != null)
                OnConnectionTypeChange -= callback;
        }

        public static void ForceCheckConnection()
        {
            if (mInstance != null)
                mInstance.mTimer = 0;
        }

        public static void SetRemoteUrls(string[] remoteUrls)
        {
            if (mInstance != null)
                mInstance.RemoteURL = remoteUrls;
        }

        public static void Init()
        {
            if (mInstance != null)
            {
                pStatus = mInstance.GetUnityStatus();
                if (mInstance.RemoteURL.Length > 0)
                {
                    mInstance.enabled = true;
                }
                else
                {
                    Debug.Log("Remote URLs are empty, Checking for Unity status, status : " + pStatus);
                    pStatus = mInstance.GetUnityStatus();
                }
            }
            else
                XDebug.LogException("ConnectivityMonitor instance is null");
        }

        #endregion Static Methods

        #region Private Methods

        private void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                DontDestroyOnLoad(gameObject);
                if (!PlatformUtilities.IsEditor())
                    _ForceInternetDown = false;
                mUpdateFrequency = RemoteSettings.GetFloat(RemoteKeys.NetConnectCheckFrequency, 5f);
                RemoteSettings.Updated += new RemoteSettings.UpdatedEventHandler(HandleRemoteUpdate);
                if (RemoteURL.Length == 0)
                {
                    //enabled = false;
                    pStatus = GetUnityStatus();
                }
#if UNITY_WEBGL
                enabled = false;
                pStatus = Status.Online;
#endif
                CheckConnectionType();
            }
            else
                Destroy(gameObject);
        }

        private void HandleRemoteUpdate()
        {
            mUpdateFrequency = RemoteSettings.GetFloat(RemoteKeys.NetConnectCheckFrequency, 5f);
            Debug.Log("NetConnectCheckFrequency " + mUpdateFrequency);
        }

        private void Update()
        {
            Status status = GetUnityStatus();
            if (status == Status.Offline)
            {
                pStatus = status;
                mRoundTripTime = 0;
                mCheckRTT = false;
            }
            else
            {
                if (!mIsChecking && RemoteURL.Length > 0)
                {
                    mTimer -= Time.unscaledDeltaTime;
                    if (mTimer <= 0)
                    {
                        mTimer = mUpdateFrequency;
                        mIndex = ++mIndex % RemoteURL.Length;
                        StartCoroutine(CheckConnection(RemoteURL[mIndex]));
                    }
                }
            }
            if (mCheckRTT)
            {
                mRoundTripTime += Time.unscaledDeltaTime;
            }
        }

        private IEnumerator CheckConnection(string url)
        {
            mIsChecking = true;
            float RTT = 0;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                mRoundTripTime = 0;
                mCheckRTT = true;
                request.timeout = 30;
                yield return request.SendWebRequest();
                mIsChecking = false;
                mTimer = mUpdateFrequency;
                RTT = mRoundTripTime * 1000;
                if (mCheckRTT)
                {
                    mCheckRTT = false;
                    //                    XDebug.Log("RTT CM in ms " + RTT, XDebug.Mask.Networking);
                    OnGetNetworkStrength?.Invoke(RTT);
                }
                mRoundTripTime = 0;
#if UNITY_2020_2_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || _ForceInternetDown)
#else
                if (request.isNetworkError || request.isHttpError || _ForceInternetDown)
#endif
                {
                    Debug.LogErrorFormat("Response URL : {0}, Error : {1}, Response Code : {2}", request.url, request.error, request.responseCode);
                    mRetryCount++;
                    if (mRetryCount >= _MaxRetries)
                    {
                        OnConnectionVerified(false);
                    }
                }
                else
                {
                    //if (XDebug.CanLog(XDebug.Mask.Networking))
                    //    Debug.LogFormat("Response URL : {0}, Text : {1}, Response Code : {2}", request.url, request.downloadHandler.text, request.responseCode);
                    //Call succeeded, now check if the data returned is what we expect.
                    if (request.downloadHandler.text.Equals("true") || request.downloadHandler.text.Equals("true\n"))
                        OnConnectionVerified(true);
                    else
                    {
                        //The call was successfull, but content was wrong, this happens in case of ISP login.
                        mRetryCount++;
                        if (mRetryCount >= _MaxRetries)
                            OnConnectionVerified(false);
                    }
                }

            }
        }

        private void OnConnectionVerified(bool isConnected)
        {
            if (isConnected)
            {
                mIndex = 0;
                pStatus = _ForceInternetDown ? Status.Offline : Status.Online;
            }
            else
            {
                ++mIndex;
                if (mIndex >= RemoteURL.Length)
                {
                    mIndex = 0;
                    pStatus = Status.Offline;

                }
                else
                    StartCoroutine(CheckConnection(RemoteURL[mIndex]));
            }
            CheckConnectionType();
        }


        /// <summary>
        /// Updated the connection type and invoke OnMobileNetworkTypeChange action on change in connection type
        /// </summary>
        private void CheckConnectionType()
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                mConnectionType = ConnectionType.WIFI;
            else
                mConnectionType = ConnectionType.MobileNetwork;

            if (mLastConnectionType == mConnectionType)
                return;
            else
            {
                mLastConnectionType = mConnectionType;
                OnConnectionTypeChange?.Invoke(mConnectionType);
            }
        }


        private void OnApplicationFocus(bool focus)
        {
            //If we just got focus, check for network right away
            if (focus)
            {
                mRoundTripTime = 0;
                ForceCheckConnection();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            mCheckRTT = !pause;
            if (!pause)
            {
                mRoundTripTime = 0;
            }
        }

        /// <summary>
        /// Gets the network status by using unity`s API.
        /// </summary>
        /// <returns>The network status.</returns>
        private Status GetUnityStatus()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return Status.Offline;
            return Status.Online;
        }

        #endregion Private Methods
    }
}