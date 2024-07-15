using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Webrequets;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace JungleeGames.Analytics
{
    /// <summary>
    /// Analytic event is sent to server every {_TimeInterval} sec.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClickstreamManager : BaseBehaviour
    {
        #region Properties
        [SerializeField] protected EnvironmentPath _APIPath = null;
        //Time durtation between each API call.
        [SerializeField] protected float _TimeInterval = 5f;
        [SerializeField] protected Product _ProductId = Product.None;
        [SerializeField] protected ChannelId _ChannelId = ChannelId.None;
        [SerializeField] protected string _Source = "game_table";

        protected float mTimer = 0f;
        protected string mCookie = null;

        protected ClickstreamEvent mClickstreamEvent = null;

        #endregion //Properties

        #region Signals
        [InjectSignal] protected SigSendMessageToFlutter mSigSendMessageToFlutter = null;

        [InjectModel] protected ClickstreamDataModel mClickstreamDataModel = null;
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Awake()
        {
#if UNITY_EDITOR || CLICKSTREAM_EVENTS_FROM_UNITY
            base.Awake();
            mClickstreamEvent = new ClickstreamEvent();
            mClickstreamEvent.events = new List<ClickstreamEventData>();
            mClickstreamEvent.visit = new ClickstreamVisitData();
            mClickstreamEvent.visit.channelId = (int)_ChannelId;
            mClickstreamEvent.visit.productId = _ProductId;
            mClickstreamEvent.visit.osName = SystemInfo.operatingSystem;
            mClickstreamEvent.visit.model = SystemInfo.deviceModel;
            mClickstreamEvent.visit.networkType = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ? "WiFi" : "MobileData";
            enabled = false;

            mSigSendMessageToFlutter.AddListener(OnFlutterMessage);
            XDebug.Assert(_ProductId != Product.None, "Product ID not specified");
#else
            Destroy(gameObject);
#endif
        }

        protected override void OnDestroy()
        {
#if UNITY_EDITOR || CLICKSTREAM_EVENTS_FROM_UNITY
            mSigSendMessageToFlutter.RemoveListener(OnFlutterMessage);
#endif
            base.OnDestroy();
        }

        /// <summary>
        /// Process only AnalyticsEvent & ignore the rest
        /// </summary>
        /// <param name="flutterMessage"></param>
        protected virtual void OnFlutterMessage(FlutterMessage flutterMessage)
        {
            if (flutterMessage.type != FlutterMessageType.AnalyticsEvent)
                return;
            enabled = true;
            GetEventData(flutterMessage.data);
        }

        protected virtual ClickstreamEventData GetEventData(string flutterMessage)
        {
            mClickstreamEvent.visit.appVersion = mClickstreamDataModel.appVersion;
            mClickstreamEvent.visit.userId = mClickstreamDataModel.userId;

            ClickstreamEventData eventData = new ClickstreamEventData();
            eventData.userId = mClickstreamDataModel.userId;
            eventData.clientTimestamp = TimeUtilities.GetEpochTimeInMilliseconds();

            //Add common meta data for the event
            eventData.eventMetadata = new Dictionary<string, object>();
            eventData.eventMetadata.Add("productID", _ProductId);
            eventData.eventMetadata.Add("src", _Source);
            mClickstreamEvent.events.Add(eventData);
            return eventData;
        }

        protected virtual void Update()
        {
            mTimer += Time.deltaTime;
            if (mTimer >= _TimeInterval)
            {
                enabled = false;
                mTimer = 0f;
                SendEvent();
                mClickstreamEvent.events.Clear();
            }
        }

        protected virtual void SendEvent()
        {
            mClickstreamEvent.visit.clientTimestamp = TimeUtilities.GetEpochTimeInMilliseconds();
            WebRequestHandler2 webRequest = new WebRequestHandler2(_APIPath.Path, OnComplete, OnFailed, null);
            //WebRequestHandlerAsync webRequest = new WebRequestHandlerAsync(_APIPath.Path, OnComplete, OnFailed, null);
            webRequest.Run(mClickstreamEvent.ToJson(), mCookie);
        }

        protected virtual void OnFailed(string error)
        {
            XDebug.LogError($"Failed to publish clickstream events: {error}", XDebug.Mask.Analytics);
        }

        protected virtual void OnComplete(string data, WebHeaderCollection headers)
        {
            if (XDebug.CanLog(XDebug.Mask.Analytics))
                XDebug.Log($"Successfully published clickstream events\n {data} \n{headers.ToJson()}", XDebug.Mask.Analytics);
            mCookie = headers["set-cookie"];
        }
        #endregion //Private Methods
    }
}
