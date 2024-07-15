using System;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Socket
{
    public class WSClientConnection : XGBase
    {
        #region data
        private Queue<EventArgs> mEventQueue = new Queue<EventArgs>();
        private readonly object mLockObject = new object();
        IWSClient mWebSocket = null;
        WebSocketStatus mWebSocketStatus = WebSocketStatus.None;
        float mPingRepeatTime;
        private bool mSendPing = false;
        private float mPingTime;
        private float mPongThreshodTime;
        private bool mCheckPong = false;
        private PacketBase mPacketBase = null;

        public event Action<string> OnMessageReceived;
        public event Action OnConnectionOpen;
        public event Action OnConnectionClose;
        #endregion

        public WSClientConnection()
        {
            mPacketBase = new PacketBase() { type = BaseCommandType.REQ_PING_PONG };
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
        }

        ~ WSClientConnection()
        {
            ConnectivityMonitor.RemoveListener(OnConnectionStatusChanged);
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
                mPingTime -= Time.deltaTime;
                if (mPingTime <= 0)
                {
                    mPingTime = mPingRepeatTime;
                    OnSendSocketMessage(mPacketBase);
                }

                if (mCheckPong)
                {
                    //PONG threshold
                    mPongThreshodTime -= Time.deltaTime;
                    if (mPongThreshodTime <= 0)
                    {
                        //OnForceDisconnectSocket();
                        if (XDebug.CanLog(XDebug.Mask.Networking))
                            XDebug.Log($"SocketId: : PING/PONG PONG Reconnecting: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")}", XDebug.Mask.Networking);
                        //mSigSocketConnectionStatusV2.Dispatch(mSocketId, false, WebSocketStatus.Closed);
                    }
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

        public void Connect(string url, float pingTime, Dictionary<string, string> headers, IWebSocketClientDecider clientDecider = null)
        {
            WSClient client = clientDecider == null ? WSClient.WebSocketSharp : clientDecider.GetClient();
            Connect(url, pingTime, headers, client);
        }

        public void Connect(string url, float pingTime, Dictionary<string, string> headers, WSClient client = WSClient.WebSocketSharp)
        {
            try
            {
                mWebSocketStatus = WebSocketStatus.Connecting;
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.Log($"WebSocketBehaviour::Trying to connect to :{url} with client {client}", XDebug.Mask.Networking);
                mSendPing = false;
                if (mWebSocket != null)
                    mWebSocket.Dispose();
                
                mWebSocket = new WSClientFactory().GetWebSockeClient(client);
                mPingTime = 0f;
                mPingRepeatTime = pingTime;
                mWebSocket.Connect(url, headers, AddEventsToQueue);
            }
            catch (Exception ex)
            {
                XDebug.LogException($"WebSocketBehaviour::Exception while connecting:-> URL:{url}, Message:{ex.Message}");
            }
        }

        public void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WSStates.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                {
                    XDebug.Log($"WebSocketBehaviour::Network Msg Sending : {DateTime.Now.ToString("HH:mm:ss.fff")} {packetData}", XDebug.Mask.Networking);
                }
                mWebSocket.OnSendSocketMessage(packetData);
            }
        }

        public void Disconnect()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                XDebug.Log($"OnDisconnectSocket>>>>>> web socket is null ? {mWebSocket == null}", XDebug.Mask.Networking);
            }
            mSendPing = false;
            if (mWebSocket != null)
            {
                mWebSocketStatus = WebSocketStatus.Disconnecting;

                if (mWebSocket.ReadyState != WSStates.Closing && mWebSocket.ReadyState != WSStates.Closed)
                {
                    mWebSocket.Disconnect();
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.LogError($"Connection status is {mWebSocket.ReadyState}", XDebug.Mask.Networking);
                    HandleOnSocketClose(WSCloseStatusCode.Normal);
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
        private void OnSendSocketMessage(PacketBase packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WSStates.Open)
                mWebSocket.OnSendSocketMessage(packetData);
        }

        private void ProcessEvents(EventArgs e)
        {
            if (e is WSMessageEventArgs)
            {
                WSMessageEventArgs eventArgs = ((WSMessageEventArgs)e);
                OnSocketMessage(eventArgs);
            }
            else if (e is WSErrorEventArgs)
            {
                WSErrorEventArgs eventArgs = ((WSErrorEventArgs)e);
                OnSocketError(eventArgs);
            }
            else if (e is WSCloseEventArgs)
            {
                WSCloseEventArgs eventArgs = ((WSCloseEventArgs)e);
                OnSocketClose(eventArgs);
            }
            else
            {
                OnSocketOpen(e);
            }
        }

        private void OnSocketError(WSErrorEventArgs e)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.LogError($"OnSocketError! Message: {e.Message}\nException: {e.Exception}", XDebug.Mask.Networking);
            HandleOnSocketClose(WSCloseStatusCode.ServerError);
        }

        private void OnSocketClose(WSCloseEventArgs e)
        {
            mSendPing = false;
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"OnSocketClose! {e.Reason}, {e.Code}", XDebug.Mask.Networking);
            HandleOnSocketClose((WSCloseStatusCode)e.Code);
            OnConnectionClose?.Invoke();
        }

        private void OnSocketMessage(WSMessageEventArgs e)
        {
            string message = e.Data;
            OnMessageReceived?.Invoke(message);

            SM_PacketBase packetBase = message.FromJson<SM_PacketBase>();

            if (packetBase.type == BaseCommandType.REQ_PING_PONG)
            {
                mPongThreshodTime = 6;
            }
        }

        private void OnSocketOpen(EventArgs e)
        {
            mWebSocketStatus = WebSocketStatus.Connected;

            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Connection open! {e}", XDebug.Mask.Networking);
            //InvokeRepeating(nameof(SendPingMessage), mPingTime, mPingRepeatTime);
            mSendPing = true;
            OnConnectionOpen?.Invoke();
        }

        private void HandleOnSocketClose(WSCloseStatusCode errorCode)
        {
            if (errorCode != WSCloseStatusCode.Abnormal && errorCode >= WSCloseStatusCode.ProtocolError)
                mWebSocketStatus = WebSocketStatus.AbnormalDisconnection;
            else if (errorCode == WSCloseStatusCode.Away)
                mWebSocketStatus = WebSocketStatus.Closed;
            else
            {
                if (mWebSocketStatus == WebSocketStatus.Connected)
                    mWebSocketStatus = WebSocketStatus.Disconnected;
                if (mWebSocketStatus == WebSocketStatus.Disconnecting)
                    mWebSocketStatus = WebSocketStatus.Closed;
            }
            mSendPing = false;
        }
        #endregion
    }
}
