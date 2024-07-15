#if USE_BEST_WEBSOCKET
using System;
using System.Collections.Generic;
using Best.WebSockets;
using Best.HTTP;
using Best.HTTP.Shared;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class BestWebSocket : XGBase, IWSClient
    {
        private const int DefaultTimeout = 500;
        public bool IsAlive { get { return mWebSocket.IsOpen; } }
        public WSStates ReadyState { get { return (WSStates)mWebSocket.State; } }
        private WebSocket mWebSocket = null;
        private Dictionary<string, string> mHeaders = new Dictionary<string, string>();
        private EventHandler mEventCallback;
        [InjectModel] private RemoteConfigModel mRemoteConfigModel;

        public void Connect(string url, Dictionary<string, string> headers, EventHandler eventCallback)
        {
            mEventCallback = eventCallback;
            RemoveWebSocketListeners();
            mWebSocket = new WebSocket(new Uri(url));
#if DEV_BUILD || QA_BUILD
            HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.Error;
#else
            HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.None;

#endif
            AddWebSocketListeners();
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"BestWebSocket :: Connect web socket {url}", XDebug.Mask.Networking);
            int timeOut = mRemoteConfigModel == null ? DefaultTimeout : (int)mRemoteConfigModel.GetLong("best_socket_timeout", DefaultTimeout);
            mWebSocket.InternalRequest.TimeoutSettings.ConnectTimeout = new TimeSpan(0,0,0,0, timeOut);
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    if (mWebSocket.InternalRequest.HasHeader(kvp.Key))
                        mWebSocket.InternalRequest.SetHeader(kvp.Key, kvp.Value);
                    else
                        mWebSocket.InternalRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }
            mWebSocket.Open();
        }

        public void Disconnect()
        {
            if (mWebSocket != null)
            {
                if (ReadyState != WSStates.Closing && ReadyState != WSStates.Closed)
                {
                    mWebSocket.Close(WebSocketStatusCodes.GoingAway,"");
                    Dispose();
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.LogError($"BestWebSocket :: Connection status is {ReadyState}", XDebug.Mask.Networking);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.LogError($"BestWebSocket :: mWebSocket is null. How?", XDebug.Mask.Networking);
            }
        }

        public void Dispose()
        {
            
        }

        public void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && ReadyState == WSStates.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                {
                    XDebug.Log($"BestWebSocket :: Network Msg Sending : {DateTime.Now.ToString("HH:mm:ss.fff")} {packetData}", XDebug.Mask.Networking);
                }
                mWebSocket.Send(packetData);
            }
        }

        public void OnSendSocketMessage(PacketBase packetData)
        {
            if (mWebSocket != null && ReadyState == WSStates.Open)
            {
                mWebSocket.Send(packetData.ToJson());
            }
        }

        void RemoveWebSocketListeners()
        {
            if (mWebSocket != null)
            {
                mWebSocket.OnOpen -= SocketOnOpen;
                mWebSocket.OnMessage -= SocketOnMessage;
                mWebSocket.OnClosed -= SocketOnClose;
                mWebSocket.OnInternalRequestCreated -= OnInternalRequestCreated;
            }
        }

        void AddWebSocketListeners()
        {
            if (mWebSocket != null)
            {
                mWebSocket.OnOpen += SocketOnOpen;
                mWebSocket.OnMessage += SocketOnMessage;
                mWebSocket.OnClosed += SocketOnClose;
                mWebSocket.OnInternalRequestCreated += OnInternalRequestCreated;
            }
        }

        private void OnInternalRequestCreated(WebSocket webSocket, HTTPRequest request)
        {
            if (mHeaders != null)
            {
                foreach (var kvp in mHeaders)
                {
                    if (request.HasHeader(kvp.Key))
                        request.SetHeader(kvp.Key, kvp.Value);
                    else
                        request.AddHeader(kvp.Key, kvp.Value);
                }
            }
        }

        private void SocketOnOpen(WebSocket webSocket)
        {
            mEventCallback?.Invoke(webSocket, null);
        }

        private void SocketOnMessage(WebSocket webSocket, string message)
        {
            mEventCallback?.Invoke(webSocket, new WSMessageEventArgs() { Data = message});
        }

        private void SocketOnClose(WebSocket webSocket, WebSocketStatusCodes code, string message)
        {
            mEventCallback?.Invoke(webSocket, new WSCloseEventArgs((ushort)code,message));
        }

        private void SocketOnError(object sender, EventArgs e)
        {
            mEventCallback?.Invoke(sender, e);
        }
    }
}
#endif