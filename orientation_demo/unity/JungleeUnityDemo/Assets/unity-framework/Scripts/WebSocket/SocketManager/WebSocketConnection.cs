//#define PING_PONG_LOGS
#if USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class WebSocketConnection : XGBase
    {
        #region data
        WebSocket mWebSocket = null;
        WebSocketStatus mWebSocketStatus = WebSocketStatus.None;
        private Queue<EventArgs> mEventQueue = new Queue<EventArgs>();
        private readonly object mLockObject = new object();
        private bool mSendPing = false;
        private bool mReconnectSocket = true;
        private bool mShowingLoader = false;
        private DateTime mLastPingTime;
        PacketBase mPacketBase = null;

        float mLastPingTimeStamp = 3;
        float mPongThreshodTime = 6;

        #endregion

        public long mSocketId { get; private set; }
        public bool IsConnected => mWebSocket == null && mWebSocket.IsAlive;

        #region Callbacks
        public Action<WebSocketConnection> OnConectionClosed = null;
        #endregion

        #region Signals
        [InjectSignal] protected SigSocketConnectionStatus mSigSocketConnectionStatus = null;
        [InjectSignal] protected SigOnSocketMesageReceived mSigOnSocketMesageReceived = null;
        [InjectSignal] protected SigSendSocketMesage mSigSendSocketMesage = null;
        [InjectSignal] private SigDisconnectSocket mSigDisconnectSocket = null;

        [InjectSignal] protected SigSocketConnectionStatusV2 mSigSocketConnectionStatusV2 = null;
        [InjectSignal] protected SigSendSocketMessageV2 mSigSendSocketMesageV2 = null;
        [InjectSignal] private SigDisconnectSocketV2 mSigDisconnectSocketV2 = null;
        [InjectSignal] private SigDisconnectAllSockets mSigDisconnectAllSockets = null;

        //ServerConnectionMonitor
        [InjectSignal] private SigClearSocketIncomingMessage mSigClearSocketIncomingMessage = null;
        [InjectModel] private WebSocketModel mWebSocketModel = null;

        #endregion//============================================================[ Signals ]

        public WebSocketConnection(string url, long id, Dictionary<string, string> headers)
        {
            mPacketBase = new PacketBase() { type = BaseCommandType.REQ_PING_PONG };
            mSigDisconnectSocket.AddListener(OnDisconnectSocket);
            mSigClearSocketIncomingMessage.AddListener(OnClearSocketIncomingMessage);

            mSigSendSocketMesageV2.AddListener(OnSendSocketMessage);
            mSigDisconnectSocketV2.AddListener(OnDisconnectSocket);
            mSigDisconnectAllSockets.AddListener(OnForceDisconnectSocket);
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
            OnConnectSocket(url, id, headers);
        }

        ~WebSocketConnection()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")} Destructor: id:{mSocketId}", XDebug.Mask.Networking);
            }
        }

        private void DestroyObject()
        {
            //mWebSocket = null;
            mSendPing = false;
            RemoveWebSocketListeners();
            mSigDisconnectSocket.RemoveListener(OnDisconnectSocket);
            mSigSendSocketMesageV2.RemoveListener(OnSendSocketMessage);
            mSigDisconnectSocketV2.RemoveListener(OnDisconnectSocket);
            mSigDisconnectAllSockets.RemoveListener(OnForceDisconnectSocket);
            mSigClearSocketIncomingMessage.RemoveListener(OnClearSocketIncomingMessage);
            ConnectivityMonitor.RemoveListener(OnConnectionStatusChanged);
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")} Destructor id:{mSocketId}", XDebug.Mask.Networking);
            }
            if (mWebSocket != null)
            {
                ((IDisposable)mWebSocket).Dispose();
            }
        }

        public void Update()
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
            if (mSendPing)
            {
                //PING 
                mLastPingTimeStamp -= Time.deltaTime;
                if (mLastPingTimeStamp <= 0)
                {
                    mLastPingTimeStamp = 3.0f;
                    OnSendSocketMessage(mPacketBase);
                }

                //PONG threshold
                mPongThreshodTime -= Time.deltaTime;
                if (mPongThreshodTime <= 0)
                {
                    OnForceDisconnectSocket();
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.Log($"SocketId:{mSocketId} : PING/PONG PONG Reconnecting: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")}", XDebug.Mask.Networking);
                    mSigSocketConnectionStatusV2.Dispatch(mSocketId, false, WebSocketStatus.Closed);
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

        private void OnConnectSocket(string url, long id, Dictionary<string, string> headers)
        {
            try
            {
                mSocketId = id;
                mWebSocketStatus = WebSocketStatus.Connecting;
                mShowingLoader = true;
                mReconnectSocket = true;
                UiLoadingCursor.Show(true);
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")}: Trying to connect to :{url}, id:{id}", XDebug.Mask.Networking);
                //CancelInvoke();
                RemoveWebSocketListeners();
                if (mWebSocket != null)
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")}: Trying to connect to 2", XDebug.Mask.Networking);
                    ((IDisposable)mWebSocket).Dispose();
                }

                mWebSocket = new WebSocket(url);
                mWebSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                Dictionary<string, string> dict = headers["cookie"].SplitCookie();
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    if (kvp.Key.Equals("Expires", StringComparison.InvariantCultureIgnoreCase))
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
                mWebSocket.ConnectAsync();
                mWebSocketModel.AddConnection(id, mWebSocket);
            }
            catch (Exception ex)
            {
                DisableLoader();
                XDebug.LogException($"Exception while connecting: url:{url}, id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, Message:{ex.Message}");
            }
        }


        private void OnSendSocketMessage(long id, string packetData)
        {
            if (id != mSocketId)
                return;
            OnSendSocketMessage(packetData);
        }

        private void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.Log($"SocketId:{mSocketId} : {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")} CM:{packetData}", XDebug.Mask.Networking);

                mWebSocket.SendAsync(packetData, (completed) =>
                 {
                     if (!completed)
                     {
                         if (XDebug.CanLog(XDebug.Mask.Networking))
                         {
                             XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")} Unable to send packet id:{mSocketId}", XDebug.Mask.Networking);
                         }
                     }
                 });
            }
        }

        private void OnForceDisconnectSocket()
        {
            DisableLoader();
            mSendPing = false;
            mReconnectSocket = false;
            mWebSocket.CloseAsync();
            mWebSocketModel.RemoveConnection(mSocketId);
            OnConectionClosed?.Invoke(this);
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"{DateTime.Now.ToString("HH:mm:ss.fff")} OnForceDisconnectSocket:{mSocketId}", XDebug.Mask.Networking);
            DestroyObject();
        }

        private void OnDisconnectSocket(long socketId)
        {
            if (mSocketId != socketId)
                return;
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"OnDisconnectSocket: id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, web socket is null ? {mWebSocket == null}", XDebug.Mask.Networking);
            }
            OnForceDisconnectSocket();
            //OnDisconnectSocket();
        }
        private void OnDisconnectSocket()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"OnDisconnectSocket>>>>>> id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, web socket is null ? {mWebSocket == null}", XDebug.Mask.Networking);
            }

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
                        XDebug.LogError($"Connection status is {mWebSocket.ReadyState}, id:{mSocketId}", XDebug.Mask.Networking);
                    HandleOnSocketClose(CloseStatusCode.Normal);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.LogError($"mWebSocket is null. How?, id:{mSocketId}", XDebug.Mask.Networking);
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

        string log = string.Empty;
        private void OnSendSocketMessage(PacketBase packetData)
        {
            if (mWebSocket != null)
            {
                if (mWebSocket.ReadyState == WebSocketState.Open)
                {
                    log = $"SocketId:{mSocketId} : PING/PONG SENT :{this.mLastPingTime.ToString("HH:mm:ss.fff")}:{mWebSocket.ReadyState}";
                    mWebSocket.SendAsync(packetData.ToJson(), (Action<bool>)((completed) =>
                    {
                        this.mLastPingTime = DateTime.Now;
                        if (!completed)
                        {
                            log = $"SocketId:{mSocketId} : PING/PONG FAILED AT :{this.mLastPingTime.ToString("HH:mm:ss.fff")}:{mWebSocket.ReadyState}";
                            SendPingLogs(log);
                        }
                    })
                    );
                    SendPingLogs(log);
                }
                else
                {
                    log = $"SocketId:{mSocketId} : PING/PONG FAILED AT :{mLastPingTime.ToString("HH:mm:ss.fff")}:{mWebSocket.ReadyState}";
                    SendPingLogs(log);
                }
            }
        }

        private void SendPingLogs(string data)
        {
#if PING_PONG_LOGS
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log(data, XDebug.Mask.Networking);
#endif
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
                XDebug.LogError($"OnSocketError! id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")},  Message: {e.Message}\nException: {e.Exception}", XDebug.Mask.Networking);
            HandleOnSocketClose(CloseStatusCode.ServerError);
        }

        private void OnSocketClose(CloseEventArgs e)
        {
            mSendPing = false;
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"OnSocketClose! id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")},  {e.Reason}, {e.Code}", XDebug.Mask.Networking);
            HandleOnSocketClose((CloseStatusCode)e.Code);
        }

        private void OnSocketMessage(MessageEventArgs e)
        {
            if (!mSendPing)
                return;

            string message = e.Data;
            mSigOnSocketMesageReceived.Dispatch(message, mSocketId);
            SM_PacketBase packetBase = message.FromJson<SM_PacketBase>();

            if (packetBase.type == BaseCommandType.REQ_PING_PONG)
            {
                mPongThreshodTime = 6;
            }
#if PING_PONG_LOGS
            if (XDebug.CanLog(XDebug.Mask.Networking) && packetBase.type == BaseCommandType.REQ_PING_PONG)
                XDebug.Log($"SocketId:{mSocketId} : PING/PONG PONG Received: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")}", XDebug.Mask.Networking);
#endif
        }

        private void OnSocketOpen(EventArgs e)
        {
            DisableLoader();
            mWebSocketStatus = WebSocketStatus.Connected;
            mSendPing = true;
            if (mSocketId == 0)
                mSigSocketConnectionStatus.Dispatch(true, mWebSocketStatus);
            else
                mSigSocketConnectionStatusV2.Dispatch(mSocketId, true, mWebSocketStatus);

            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Connection open!, id:{mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, {e}", XDebug.Mask.Networking);
        }

        private void HandleOnSocketClose(CloseStatusCode errorCode)
        {
            DisableLoader();
            if (mReconnectSocket)
            {
                if (errorCode == CloseStatusCode.Abnormal)
                {
                    mWebSocketStatus = WebSocketStatus.Disconnected;
                    XDebug.LogError($"Abnormal Disconnection: {errorCode}! id: {mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, LastPingTime: {mLastPingTime.ToString("HH:mm:ss.fff")}");
                }

                if (errorCode != CloseStatusCode.Abnormal && errorCode >= CloseStatusCode.ProtocolError)
                {
                    mWebSocketStatus = WebSocketStatus.AbnormalDisconnection;
                    XDebug.LogError($"Abnormal Disconnection: {errorCode}! id: {mSocketId}, {DateTime.Now.ToString("HH:mm:ss.fff")}, LastPingTime: {mLastPingTime.ToString("HH:mm:ss.fff")}");
                }
                else if (errorCode == CloseStatusCode.Away)
                    mWebSocketStatus = WebSocketStatus.Closed;
                else
                {
                    if (mWebSocketStatus == WebSocketStatus.Connected)
                        mWebSocketStatus = WebSocketStatus.Disconnected;
                    if (mWebSocketStatus == WebSocketStatus.Disconnecting)
                        mWebSocketStatus = WebSocketStatus.Closed;
                }
                if (mSocketId == 0)
                    mSigSocketConnectionStatus.Dispatch(false, mWebSocketStatus);
                else
                    mSigSocketConnectionStatusV2.Dispatch(mSocketId, false, mWebSocketStatus);
                mWebSocketModel.RemoveConnection(mSocketId);
                OnConectionClosed?.Invoke(this);
            }
        }

        private void DisableLoader()
        {
            if (mShowingLoader)
            {
                mShowingLoader = false;
                UiLoadingCursor.Show(false);
            }
        }

        #endregion
    }
}
#endif //WEBSOCKET_SHARP