#if USE_NATIVE_WEBSOCKET
using NativeWebSocket;
#endif
#if USE_WEBSOCKET_SHARP
using System.Collections.Generic;
using WebSocketSharp;
#endif
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class WebSocketModel : XGModel
    {
#if USE_NATIVE_WEBSOCKET
        public WebSocket mWebsocket = null;
        public bool IsConnected => mWebsocket.State == WebSocketState.Open;
#elif USE_WEBSOCKET_SHARP
        public Dictionary<long, WebSocket> mWebsockets = null;

        public WebSocket mWebsocket = null;//Older version
        public bool IsConnected => mWebsocket == null && mWebsocket.IsAlive;
#else
        public bool IsConnected => false;
#endif

#if USE_WEBSOCKET_SHARP
        public WebSocketModel()
        {
            mWebsockets = new Dictionary<long, WebSocket>();
        }

        public void AddConnection(long id, WebSocket webSocket)
        {
            mWebsockets[id] = webSocket;
        }

        public void RemoveConnection(long socketId)
        {
            if (mWebsockets.ContainsKey(socketId))
                mWebsockets.Remove(socketId);
        }
#elif USE_NATIVE_WEBSOCKET
        public WebSocketModel()
        {
        }
#endif
    }
}
