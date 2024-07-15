#if USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using UnityEngine;
using WebSocketSharp;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class WebSocketSharpManager : BaseBehaviour
    {
        #region data
        private Queue<EventArgs> mEventQueue = new Queue<EventArgs>();
        private readonly object mLockObject = new object();
        WebSocket mWebSocket = null;
        WebSocketStatus mWebSocketStatus = WebSocketStatus.None;
        float mPingRepeatTime;
        float mPingTime;
        #endregion


        #region Signals
        [InjectSignal] protected SigConnectSocket mSigConnectSocket = null;
        [InjectSignal] protected SigSocketConnectionStatus mSigSocketConnectionStatus = null;
        [InjectSignal] protected SigOnSocketMesageReceived mSigOnSocketMesageReceived = null;
        [InjectSignal] protected SigSendSocketMesage mSigSendSocketMesage = null;
        [InjectSignal] private SigDisconnectSocket mSigDisconnectSocket = null;
        //ServerConnectionMonitor
        [InjectSignal] private SigOnPingSend mXSigOnPingSend = null;
        [InjectSignal] private SigClearSocketIncomingMessage mSigClearSocketIncomingMessage = null;
        [InjectModel] private WebSocketModel mWebSocketModel = null;

        #endregion//============================================================[ Signals ]

        protected override void Awake()
        {
            base.Awake();
            mSigConnectSocket.AddListener(OnConnectSocket);
            mSigSendSocketMesage.AddListener(OnSendSocketMessage);
            mSigDisconnectSocket.AddListener(OnDisconnectSocket);
            mSigClearSocketIncomingMessage.AddListener(OnClearSocketIncomingMessage);
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveWebSocketListeners();
            mSigConnectSocket.RemoveListener(OnConnectSocket);
            mSigSendSocketMesage.RemoveListener(OnSendSocketMessage);
            mSigDisconnectSocket.RemoveListener(OnDisconnectSocket);
            mSigClearSocketIncomingMessage.RemoveListener(OnClearSocketIncomingMessage);
            ConnectivityMonitor.RemoveListener(OnConnectionStatusChanged);
        }

        private void Update()
        {
            lock (mLockObject)
            {
                try
                {
                    if (mEventQueue.Count > 0)
                    {
                        EventArgs eventArgs = mEventQueue.Dequeue();
                        ProcessEvents(eventArgs);
                    }
                }
                catch (Exception e)
                {
                    XDebug.LogException(e);
                }
            }
        }

        #region Listeners
        private void AddEventsToQueue(object sender, EventArgs eventArgs)
        {
            lock (mLockObject)
            {
                mEventQueue.Enqueue(eventArgs);
            }
        }

        private void OnConnectSocket(string url, float pingTime, Dictionary<string, string> headers)
        {
            try
            {
                mWebSocketStatus = WebSocketStatus.Connecting;
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.Log($"<color=red>Trying to connect to :{url} WebSocket is null before creating instance? {mWebSocket == null}</color>", XDebug.Mask.Networking);
                CancelInvoke();
                RemoveWebSocketListeners();
                if (mWebSocket != null)
                {
                    ((IDisposable)mWebSocket).Dispose();
                }

                mWebSocket = new WebSocket(url);
                mWebSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default|SslProtocols.Tls11|SslProtocols.Tls12|SslProtocols.Ssl2|SslProtocols.Ssl3;

                Dictionary<string, string> dict = headers["cookie"].SplitCookie();
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    if (kvp.Key.Equals("Expires",StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    mWebSocket.SetCookie(new WebSocketSharp.Net.Cookie(kvp.Key, kvp.Value));
                }

#if DEV_BUILD || QA_BUILD
                mWebSocket.Log.Level = LogLevel.Trace;
#endif
                mWebSocket.EmitOnPing = true;
                AddWebSocketListeners();
                mPingTime = 0f;
                mPingRepeatTime = pingTime;
                mWebSocket.ConnectAsync();
                mWebSocketModel.mWebsocket = mWebSocket;
            }
            catch (Exception ex)
            {
                XDebug.LogException($"Exception while connecting:-> URL:{url}, Message:{ex.Message}");
            }
        }

        private void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                {
                    XDebug.Log($"Network Msg Sending : {DateTime.Now.ToString("HH:mm:ss.fff")} {packetData}", XDebug.Mask.Networking);
                }
                mWebSocket.SendAsync(packetData, (completed) =>
                 {
                     if (!completed)
                     {
                         if (XDebug.CanLog(XDebug.Mask.Networking))
                         {
                             XDebug.Log($"Unable to send packet", XDebug.Mask.Networking);
                         }
                     }
                 });
            }
        }

        private void OnDisconnectSocket()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"OnDisconnectSocket>>>>>> web socket is null ? {mWebSocket == null}", XDebug.Mask.Networking);
            }
            CancelInvoke();
            if (mWebSocket != null)
            {
                mWebSocketStatus = WebSocketStatus.Disconnecting;

                if (mWebSocket.ReadyState != WebSocketState.Closing && mWebSocket.ReadyState != WebSocketState.Closed)
                {
                    mWebSocket.CloseAsync(CloseStatusCode.Away);
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.LogError($"Connection status is {mWebSocket.ReadyState}", XDebug.Mask.Networking);
                    HandleOnSocketClose(CloseStatusCode.Normal);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.LogError($"mWebSocket is null. How?", XDebug.Mask.Networking);
            }
        }

        private void OnClearSocketIncomingMessage()
        {
            lock (mLockObject)
            {
                if (mEventQueue != null) mEventQueue.Clear();
            }
        }

        private void OnConnectionStatusChanged(ConnectivityMonitor.Status status)
        {

        }
        #endregion

        #region Private functions
        void SendPingMessage()
        {
            if (mWebSocket != null)
            {
                PacketBase data = new PacketBase() { type = BaseCommandType.REQ_PING_PONG };
                OnSendSocketMessage(data);
                mXSigOnPingSend.Dispatch();
            }
        }

        private void OnSendSocketMessage(PacketBase packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                mWebSocket.SendAsync(packetData.ToJson(), (completed) =>
                {
                    if (!completed)
                    {
                        if (XDebug.CanLog(XDebug.Mask.Networking))
                        {
                            XDebug.Log($"Unable to send ping", XDebug.Mask.Networking);
                        }
                    }
                }
                );
            }
        }

        void RemoveWebSocketListeners()
        {
            if (mWebSocket != null)
            {
                mWebSocket.OnOpen -= AddEventsToQueue;
                mWebSocket.OnMessage -= AddEventsToQueue;
                mWebSocket.OnClose -= AddEventsToQueue;
                mWebSocket.OnError -= AddEventsToQueue;
            }
        }

        void AddWebSocketListeners()
        {
            if (mWebSocket != null)
            {
                mWebSocket.OnOpen += AddEventsToQueue;
                mWebSocket.OnMessage += AddEventsToQueue;
                mWebSocket.OnClose += AddEventsToQueue;
                mWebSocket.OnError += AddEventsToQueue;
            }
        }

        private void ProcessEvents(EventArgs e)
        {
            if (e is MessageEventArgs)
            {
                MessageEventArgs eventArgs = ((MessageEventArgs)e);
                OnSocketMessage(eventArgs);
            }
            else if (e is ErrorEventArgs)
            {
                ErrorEventArgs eventArgs = ((ErrorEventArgs)e);
                OnSocketError(eventArgs);
            }
            else if (e is CloseEventArgs)
            {
                CloseEventArgs eventArgs = ((CloseEventArgs)e);
                OnSocketClose(eventArgs);
            }
            else
            {
                OnSocketOpen(e);
            }
        }

        private void OnSocketError(ErrorEventArgs e)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.LogError($"OnSocketError! Message: {e.Message}\nException: {e.Exception}", XDebug.Mask.Networking);
            HandleOnSocketClose(CloseStatusCode.ServerError);
        }

        private void OnSocketClose(CloseEventArgs e)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"OnSocketClose! {e.Reason}, {e.Code}", XDebug.Mask.Networking);
            HandleOnSocketClose((CloseStatusCode)e.Code);
        }

        private void OnSocketMessage(MessageEventArgs e)
        {
            string message = e.Data;
            mSigOnSocketMesageReceived.Dispatch(message, 0);
        }

        private void OnSocketOpen(EventArgs e)
        {
            mWebSocketStatus = WebSocketStatus.Connected;

            mSigSocketConnectionStatus.Dispatch(true, mWebSocketStatus);
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Connection open! {e}", XDebug.Mask.Networking);
            InvokeRepeating(nameof(SendPingMessage), mPingTime, mPingRepeatTime);
        }

        private void HandleOnSocketClose(CloseStatusCode errorCode)
        {
            if (errorCode!=CloseStatusCode.Abnormal && errorCode >= CloseStatusCode.ProtocolError)
                mWebSocketStatus = WebSocketStatus.AbnormalDisconnection;
            else if(errorCode==CloseStatusCode.Away)
                mWebSocketStatus = WebSocketStatus.Closed;
            else
            {
                if (mWebSocketStatus == WebSocketStatus.Connected)
                    mWebSocketStatus = WebSocketStatus.Disconnected;
                if (mWebSocketStatus == WebSocketStatus.Disconnecting)
                    mWebSocketStatus = WebSocketStatus.Closed;
            }
            CancelInvoke();
            mSigSocketConnectionStatus.Dispatch(false, mWebSocketStatus);
        }
        #endregion
    }
}
#endif //WEBSOCKET_SHARP