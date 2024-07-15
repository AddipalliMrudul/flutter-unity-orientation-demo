using System;
using System.Collections.Generic;
using UnityEngine;
// using WebSocketSharp;

namespace XcelerateGames.Socket
{
    [DisallowMultipleComponent]
    public class WebSocketBehaviour : BaseBehaviour
    {
        private WSClientConnection mClientConnection = null;

        public event Action<string> OnMessageReceived;
        public event Action OnConnectionOpen;
        public event Action OnConnectionClose;

        private void Update()
        {
            if (mClientConnection != null)
                mClientConnection.Update();
        }

        public void Connect(string url, float pingTime, Dictionary<string, string> headers, IWebSocketClientDecider clientDecider = null)
        {
            WSClient client = clientDecider == null ? WSClient.WebSocketSharp : clientDecider.GetClient();
            Connect(url, pingTime, headers, client);
        }

        public void Connect(string url, float pingTime, Dictionary<string, string> headers, WSClient client = WSClient.WebSocketSharp)
        {
            mClientConnection = new WSClientConnection();
            mClientConnection.OnMessageReceived += OnMessageReceived;
            mClientConnection.OnConnectionOpen += OnConnectionOpen;
            mClientConnection.OnConnectionClose += OnConnectionClose;
            mClientConnection.Connect(url, pingTime, headers, client);
        }

        public void Disconnect()
        {
            if (mClientConnection != null)
                mClientConnection.Disconnect();
        }

        public void OnSendSocketMessage(string packetData)
        {
            if (mClientConnection != null)
                mClientConnection.OnSendSocketMessage(packetData);
        }
    }
}