#if USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using WebSocketSharp;

namespace XcelerateGames.Socket
{
    public class WSSharp : IWSClient
    {
        public bool IsAlive { get { return mWebSocket.IsAlive; } }
        public WSStates ReadyState { get { return (WSStates)mWebSocket.ReadyState; } }
        private WebSocket mWebSocket = null;
        private EventHandler mEventHandler;

        public void Connect(string url, Dictionary<string, string> headers, EventHandler eventCallback)
        {
            mEventHandler = eventCallback;
            RemoveWebSocketListeners();
            if (mWebSocket != null)
            {
                ((IDisposable)mWebSocket).Dispose();
            }

            mWebSocket = new WebSocket(url);
            mWebSocket.AddHeaders(headers);
            mWebSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default;

#if DEV_BUILD || QA_BUILD
            mWebSocket.Log.Level = LogLevel.Trace;
#endif
            AddWebSocketListeners();
            mWebSocket.EmitOnPing = true;
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"WebSocketSharp :: Connect web socket {url}", XDebug.Mask.Networking);
            mWebSocket.ConnectAsync();
        }

        public void Disconnect()
        {
            if (mWebSocket != null)
            {
                if (mWebSocket.ReadyState != WebSocketState.Closing && mWebSocket.ReadyState != WebSocketState.Closed)
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.LogError($"WebSocketSharp :: Closing web socket {mWebSocket.ReadyState}", XDebug.Mask.Networking);
                    mWebSocket.CloseAsync(CloseStatusCode.Away);
                    Dispose();
                }
                else
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.LogError($"WebSocketSharp :: Connection status is {mWebSocket.ReadyState}", XDebug.Mask.Networking);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    XDebug.LogError($"WebSocketSharp :: mWebSocket is null. How?", XDebug.Mask.Networking);
            }
        }

        public void Dispose()
        {
            ((IDisposable)mWebSocket).Dispose();
        }

        public void OnSendSocketMessage(string packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                {
                    XDebug.Log($"WebSocketSharp::Network Msg Sending : {DateTime.Now.ToString("HH:mm:ss.fff")} {packetData}", XDebug.Mask.Networking);
                }
                mWebSocket.SendAsync(packetData, (completed) =>
                {
                    if (!completed)
                    {
                        if (XDebug.CanLog(XDebug.Mask.Networking))
                        {
                            XDebug.Log($"WebSocketSharp::Unable to send packet", XDebug.Mask.Networking);
                        }
                    }
                });
            }
        }

        public void OnSendSocketMessage(PacketBase packetData)
        {
            if (mWebSocket != null && mWebSocket.ReadyState == WebSocketState.Open)
            {
                mWebSocket.SendAsync(packetData.ToJson(), (completed) =>
                {
                    if (!completed)
                    {
                        if (XDebug.CanLog(XDebug.Mask.Networking))
                        {
                            XDebug.Log($"WebSocketSharp :: Unable to send ping", XDebug.Mask.Networking);
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
                mWebSocket.OnOpen -= SocketOnOpen;
                mWebSocket.OnMessage -= SocketOnMessage;
                mWebSocket.OnClose -= SocketOnClose;
                mWebSocket.OnError -= SocketOnError;
            }
        }

        void AddWebSocketListeners()
        {
            if (mWebSocket != null)
            {
                mWebSocket.OnOpen += SocketOnOpen;
                mWebSocket.OnMessage += SocketOnMessage;
                mWebSocket.OnClose += SocketOnClose;
                mWebSocket.OnError += SocketOnError;
            }
        }

        private void SocketOnOpen(object sender, EventArgs e)
        {
            mEventHandler?.Invoke(sender, e);
        }

        private void SocketOnMessage(object sender, MessageEventArgs e)
        {
            mEventHandler?.Invoke(sender, new WSMessageEventArgs() { Data = e.Data});
        }

        private void SocketOnClose(object sender, CloseEventArgs e)
        {
            mEventHandler?.Invoke(sender, new WSCloseEventArgs(e.Code, e.Reason));
        }

        private void SocketOnError(object sender, ErrorEventArgs e)
        {
            mEventHandler?.Invoke(sender, e);
        }
    }
}
#endif