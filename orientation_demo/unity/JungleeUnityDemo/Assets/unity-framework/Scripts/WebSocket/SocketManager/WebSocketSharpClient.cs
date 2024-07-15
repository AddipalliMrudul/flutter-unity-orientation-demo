#if USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using UnityEngine;
using WebSocketSharp;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    [DisallowMultipleComponent]
    public class WebSocketSharpClient : MonoBehaviour
    {
        #region data
        private Queue<EventArgs> mEventQueue = new Queue<EventArgs>();
        private readonly object mLockObject = new object();
        WebSocket mWebSocket = null;
        WebSocketStatus mWebSocketStatus = WebSocketStatus.None;
        float mPingRepeatTime;
        float mPingTime;

        public Action<string> OnMessageReceived = null;
        public Action OnConnectionOpen = null;
        public Action OnConnectionClose = null;
        #endregion

        protected virtual void Awake()
        {
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
        }

        protected virtual void OnDestroy()
        {
            RemoveWebSocketListeners();
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

        public void Connect(string url, float pingTime, Dictionary<string, string> headers)
        {
            try
            {
                mWebSocketStatus = WebSocketStatus.Connecting;
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.Log($"WebSocketSharpClient::Trying to connect to :{url}", XDebug.Mask.Networking);
                CancelInvoke();
                RemoveWebSocketListeners();
                if (mWebSocket != null)
                {
                    ((IDisposable)mWebSocket).Dispose();
                }

                mWebSocket = new WebSocket(url);
                mWebSocket.AddHeaders(headers);
                mWebSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default | SslProtocols.Tls12 | SslProtocols.Tls13;

#if DEV_BUILD || QA_BUILD
                mWebSocket.Log.Level = LogLevel.Trace;
#endif
                mWebSocket.EmitOnPing = true;
                AddWebSocketListeners();
                mPingTime = 0f;
                mPingRepeatTime = pingTime;
                mWebSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                XDebug.LogException($"WebSocketSharpClient::Exception while connecting:-> URL:{url}, Message:{ex.Message}");
            }
        }

        public void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                {
                    XDebug.Log($"WebSocketSharpClient::Network Msg Sending : {DateTime.Now.ToString("HH:mm:ss.fff")} {packetData}", XDebug.Mask.Networking);
                }
                mWebSocket.SendAsync(packetData, (completed) =>
                 {
                     if (!completed)
                     {
                         if (XDebug.CanLog(XDebug.Mask.Networking))
                         {
                             XDebug.Log($"WebSocketSharpClient::Unable to send packet", XDebug.Mask.Networking);
                         }
                     }
                 });
            }
        }

        public void Disconnect()
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
                    RemoveWebSocketListeners();
                    ((IDisposable)mWebSocket).Dispose();
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
            OnConnectionClose?.Invoke();
        }

        private void OnSocketMessage(MessageEventArgs e)
        {
            string message = e.Data;
            OnMessageReceived?.Invoke(message);
        }

        private void OnSocketOpen(EventArgs e)
        {
            mWebSocketStatus = WebSocketStatus.Connected;

            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Connection open! {e}", XDebug.Mask.Networking);
            InvokeRepeating(nameof(SendPingMessage), mPingTime, mPingRepeatTime);
            OnConnectionOpen?.Invoke();
        }

        private void HandleOnSocketClose(CloseStatusCode errorCode)
        {
            if (errorCode != CloseStatusCode.Abnormal && errorCode >= CloseStatusCode.ProtocolError)
                mWebSocketStatus = WebSocketStatus.AbnormalDisconnection;
            else if (errorCode == CloseStatusCode.Away)
                mWebSocketStatus = WebSocketStatus.Closed;
            else
            {
                if (mWebSocketStatus == WebSocketStatus.Connected)
                    mWebSocketStatus = WebSocketStatus.Disconnected;
                if (mWebSocketStatus == WebSocketStatus.Disconnecting)
                    mWebSocketStatus = WebSocketStatus.Closed;
            }
            CancelInvoke();
        }
        #endregion
    }
}
#endif //WEBSOCKET_SHARP