#if USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using WebSocketSharp;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class WebSocketSharpConnectionManager : BaseBehaviour
    {
        #region data
        List<WebSocketConnection> mWebSockets = null;
        #endregion

        #region Signals
        [InjectSignal] protected SigConnectSocketV2 mSigConnectSocketV2 = null;
        #endregion Signals

        protected override void Awake()
        {
            base.Awake();
            mWebSockets = new List<WebSocketConnection>();
            mSigConnectSocketV2.AddListener(OnConnectSocket);
        }

        protected override void OnDestroy()
        {
            mSigConnectSocketV2.RemoveListener(OnConnectSocket);
            base.OnDestroy();
        }

        private void Update()
        {
            if (mWebSockets.Count > 0)
            {
                for (int i = 0; i < mWebSockets.Count; ++i)
                {
                    mWebSockets[i].Update();
                }
            }
        }

        #region Private functions
        private void OnConnectSocket(string url, long id, Dictionary<string, string> headers)
        {
            try
            {
                if (mWebSockets.Find(x => x.mSocketId == id) == null)
                {
                    WebSocketConnection webSocket = new WebSocketConnection(url, id, headers);
                    webSocket.OnConectionClosed = OnConectionClosed;
                    mWebSockets.Add(webSocket);
                    XDebug.Log($"OnConnectSocket Count:{mWebSockets.Count}", XDebug.Mask.Networking);
                }
                else
                {
                    XDebug.Log($"OnConnectSocket {id} already in list.so ignoring connection request.", XDebug.Mask.Networking);

                }
            }
            catch (Exception ex)
            {
                XDebug.LogException($"Exception while connecting URL:{url}, ID:{id}, Headers:{headers.Printable()}: Message:{ex.Message}");
            }
        }

        void OnConectionClosed(WebSocketConnection webSocketConnection)
        {
            mWebSockets.Remove(webSocketConnection);
            XDebug.Log($"OnConectionClosed Count:{ mWebSockets.Count} id: {webSocketConnection.mSocketId}",XDebug.Mask.Networking);
        }
        #endregion
    }
}
#endif //WEBSOCKET_SHARP