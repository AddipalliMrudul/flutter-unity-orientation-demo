#if USE_NATIVE_WEBSOCKET

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    public class NativeWebSocketsManager : BaseBehaviour
    {
        #region Data
        private WebSocket mWebsocket;
        #endregion//============================================================[ Data ]

        #region Signals
        [InjectSignal] protected SigConnectSocket mSigConnectSocket = null;
        [InjectSignal] protected SigSocketConnectioStatus mSigSocketConnectioStatus = null;
        [InjectSignal] protected SigOnSocketMesageReceived mSigOnSocketMesageReceived = null;
        [InjectSignal] protected SigSendSocketMesage mSigSendSocketMesage = null;
        [InjectSignal] private SigDisconnectSocket mSigDisconnectSocket = null;
        //ServerConnectionMonitor
        [InjectSignal] private SigOnPingSend mXSigOnPingSend = null;
        [InjectSignal] private SigOnGetPong mXSigOnGetPong = null;
        [InjectSignal] private SigClearSocketIncomingMessage mSigClearSocketIncomingMessage = null;
        [InjectModel] private WebSocketModel mWebSocketModel = null;

        #endregion//============================================================[ Signals ]

        protected override void Awake()
        {
            base.Awake();
            enabled = false;
            mSigConnectSocket.AddListener(OnConnectSocket);
            mSigSendSocketMesage.AddListener(OnSendSocketMessage);
            mSigDisconnectSocket.AddListener(OnDisconnectSocket);
            mSigClearSocketIncomingMessage.AddListener(OnClearSocketIncomingMessage);
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigConnectSocket.RemoveListener(OnConnectSocket);
            mSigSendSocketMesage.RemoveListener(OnSendSocketMessage);
            mSigDisconnectSocket.RemoveListener(OnDisconnectSocket);
            mSigClearSocketIncomingMessage.RemoveListener(OnClearSocketIncomingMessage);
            ConnectivityMonitor.RemoveListener(OnConnectionStatusChanged);
        }

        private async void OnDisconnectSocket()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                Debug.Log("OnDisconnectSocket>>>>>>");
            }
            CancelInvoke();
            await Close();
        }

        async private void OnSendSocketMessage(string packetData)
        {
            if (mWebsocket.State == WebSocketState.Open)
            { 
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    Debug.Log($"Sending network message {DateTime.Now.ToString("HH:mm:ss")} : {packetData}");

                await mWebsocket.SendText(packetData);
            }
        }

        async private void OnSendSocketMessage(PacketBase packetData)
        {
            await mWebsocket.SendText(packetData.ToJson());
        }

        /// <summary>
        /// Initiate WebSocket connection to the gives ws url & set thr ping time as well
        /// </summary>
        /// <param name="url">websocket url</param>
        /// <param name="pingTime">ping interval in seconds</param>
        async void OnConnectSocket(string url, float pingTime, Dictionary<string, string> headers)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                Debug.Log($"<color=red>Trying to connect to :{url} WebSocket is null before creating instance? {mWebsocket==null}</color>");
            //await Close();
            CancelInvoke();
            mWebsocket = WebSocketFactory.CreateInstance(url, headers);
            mWebSocketModel.mWebsocket = mWebsocket;

            mWebsocket.OnOpen += OnSocketOpen;
            mWebsocket.OnError += OnSocketError;
            mWebsocket.OnClose += OnSocketClose;
            mWebsocket.OnMessage += OnSocketMessage;

            // Keep sending a ping messages at every  {pingTime} second
            InvokeRepeating(nameof(SendPingMessage), 0.0f, pingTime);

            // waiting for messages
            await mWebsocket.Connect();
        }

        private void OnSocketMessage(byte[] data)
        {
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(data);
            mSigOnSocketMesageReceived.Dispatch(message);
            SM_PacketBase packetBase = message.FromJson<SM_PacketBase>();
            if (packetBase.type == BaseCommandType.REQ_PING_PONG)
            {
                mXSigOnGetPong.Dispatch();
            }
        }

        private void OnSocketClose(WebSocketCloseCode closeCode)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                Debug.Log($"Connection closed! {closeCode}");
            mSigSocketConnectioStatus.Dispatch(false);
        }

        private void OnSocketError(string errorMsg)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                Debug.LogError($"Error! {errorMsg}");
            mSigSocketConnectioStatus.Dispatch(false);
        }

        private void OnSocketOpen()
        {
            enabled = true;
            mSigSocketConnectioStatus.Dispatch(true);
            if (XDebug.CanLog(XDebug.Mask.Networking))
                Debug.Log("Connection open!");
        }

        private async Task Close()
        {
            if (mWebsocket != null)
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    Debug.Log($"Closing WebSocket connection, State: {mWebsocket.State}");
                if (mWebsocket.State == WebSocketState.Open)
                {
                    //RemoveListeners();
                    await mWebsocket.Close();
                }
                else
                    Debug.LogWarning($"Could not close WebSocket connection as current state is not Open, State: {mWebsocket.State}");
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Networking))
                    Debug.Log("Trying to close socket but socket is null");
            }
        }

        private void RemoveListeners()
        {
            if (mWebsocket == null)
                return;
            mWebsocket.OnOpen -= OnSocketOpen;
            mWebsocket.OnError -= OnSocketError;
            mWebsocket.OnClose -= OnSocketClose;
            mWebsocket.OnMessage -= OnSocketMessage;
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            mWebsocket.DispatchMessageQueue();
#endif
        }

        /// <summary>
        /// Invoked repeatedly every X seconds to notify server thatthe client is alive. Abhi hum zinda hain :)
        /// </summary>
        void SendPingMessage()
        {
            if (mWebsocket.State == WebSocketState.Open)
            {
                // Sending bytes
                //await mWebsocket.Send(new byte[] { 10, 20, 30 });

                // Sending plain text
                PacketBase data = new PacketBase() { type = BaseCommandType.REQ_PING_PONG };
                OnSendSocketMessage(data);
                mXSigOnPingSend.Dispatch();
            }
        }

        private async void OnApplicationQuit()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
            {
                Debug.Log("OnApplicationQuit>>>>>> Close Socket");
            }
            await Close();
        }

        private void OnConnectionStatusChanged(ConnectivityMonitor.Status status)
        {
            try
            {
                if (status == ConnectivityMonitor.Status.Offline)
                {
                    //if (mWebsocket != null && mWebsocket.State == WebSocketState.Open)
                    //    await Close();
                    //else
                    //    Debug.LogWarning($"Not open: {mWebsocket.State}");
                }
            }
            catch(Exception ex)
            {
                XDebug.LogException(ex.Message);
            }
        }

        private void OnClearSocketIncomingMessage()
        {
            if (mWebsocket != null) mWebsocket.ClearIncomingMessagesList();
        }
    }
}
#endif //USE_NATIVE_WEBSOCKET