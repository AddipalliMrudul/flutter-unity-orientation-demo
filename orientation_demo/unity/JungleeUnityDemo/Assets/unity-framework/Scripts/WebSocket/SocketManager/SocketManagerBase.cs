#if USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP
using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;
using XcelerateGames.Timer;

namespace XcelerateGames.Socket
{
    public class SocketManagerBase : BaseBehaviour
    {
        #region Properties
        [Header("[ Config ]")]
        [SerializeField] protected float _PingTime = 3.0f;
        [SerializeField] protected string _WebSocketUrl = null;

        protected Dictionary<string, string> mHeaders = new Dictionary<string, string>();
        protected bool? mDisconnected = null;
        protected float mTimeToReconnectAgain = 0f;
        protected float mMaxTimeToReconnectAgain = 3f;
        protected int mTotalRetryCountToConnect = 4;
        protected int mCurrentRetryCountToConnect = 0;
        protected bool mShowingLoadingCursor = false;
        #endregion Properties

        #region Signals
        [InjectSignal] protected SigSocketConnectionStatusV2 mSigSocketConnectionStatusV2 = null;
        [InjectSignal] protected SigConnectSocketV2 mSigConnectSocketV2 = null;
        [InjectSignal] protected SigOnMaxConnectionRetryExceededV2 mSigOnMaxConnectionRetryExceededV2 = null;

        [InjectSignal] protected SigSocketConnectionStatus mSigSocketConnectionStatus = null;
        [InjectSignal] protected SigOnSocketMessageErrorReceived mSigOnSocketMessageErrorReceived = null;
        [InjectSignal] protected SigConnectSocket mSigConnectSocket = null;
        [InjectSignal] protected SigDisconnectAllSockets mSigDisconnectAllSockets = null;
        [InjectSignal] protected SigClearSocketIncomingMessage mSigClearSocketIncomingMessage = null;
        [InjectSignal] protected SigSendMessageToFlutter mSigSendMessageToFlutter = null;
        [InjectSignal] protected SigTimerRegister mSigTimerRegister = null;

        #endregion

        #region Virtual Method
        /// <summary>
        /// Must override this method to update connection url
        /// </summary>
        /// <returns></returns>
        protected virtual string GetConnectionUrl() { return string.Empty; }

        /// <summary>
        /// Responsible to Add All your headers
        /// </summary>
        protected virtual void AddHeaders()
        {
            if (mHeaders == null)
                mHeaders = new Dictionary<string, string>();
        }

        protected virtual void ConnectSocket()
        {
            mSigConnectSocket.Dispatch(GetConnectionUrl(), _PingTime, GetHeaders());
        }

        protected virtual void Reconnect()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"{DateTime.Now.ToString("HH: mm:ss.fff")} Reconnecting...", XDebug.Mask.Networking);
            ConnectSocket();
        }

        protected virtual void Reconnect(long socketId)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"{DateTime.Now.ToString("HH: mm:ss.fff")} Reconnecting...", XDebug.Mask.Networking);
            ConnectSocket();
        }

        protected virtual void DisconnectSocket()
        {
            mDisconnected = true;
            mSigClearSocketIncomingMessage.Dispatch();
            mSigDisconnectAllSockets.Dispatch();
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Disconnecting all sockets,{DateTime.Now.ToString("HH:mm:ss.fff")}", XDebug.Mask.Networking);
        }

        protected virtual void OnSocketConnected()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Internet available?: {ConnectivityMonitor.pIsInternetAvailable} {DateTime.Now.ToString("HH:mm:ss.fff")}", XDebug.Mask.Networking);
        }
        protected virtual void OnSocketConnected(long socketId)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"SocketId:{socketId} {DateTime.Now.ToString("HH:mm:ss.fff")}, Internet available?: {ConnectivityMonitor.pIsInternetAvailable}", XDebug.Mask.Networking);
        }

        protected virtual void OnSocketDisconnected()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Internet available?: {ConnectivityMonitor.pIsInternetAvailable} {DateTime.Now.ToString("HH:mm:ss.fff")}", XDebug.Mask.Networking);
        }

        protected virtual void OnSocketDisconnected(long socketId)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"SocketId:{socketId} : {DateTime.Now.ToString("HH:mm:ss.fff")},Internet available?: {ConnectivityMonitor.pIsInternetAvailable}", XDebug.Mask.Networking);
        }

        protected virtual void OnInternetAvailable()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Internet Connected {DateTime.Now.ToString("HH:mm:ss.fff")}", XDebug.Mask.Networking);
            if (mDisconnected.HasValue && mDisconnected.Value)
            {
                mSigTimerRegister.Dispatch((dt) => {
                    Reconnect();
                }, mTimeToReconnectAgain);
            }
        }

        protected virtual void OnInternetNotAvailable()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"Internet Disconnected {DateTime.Now.ToString("HH:mm:ss.fff")}", XDebug.Mask.Networking);
            mSigDisconnectAllSockets.Dispatch();
        }

        protected virtual void ExceedMaxRetryOnSocketDisconnection()
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"ExceedMaxRetryOnSocketDisconnection {DateTime.Now.ToString("HH:mm:ss.fff")} - Can throw back to lobby", XDebug.Mask.Networking);
        }

        protected virtual void OnSocketMessageErrorReceived(SM_PacketBase packetBase)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.LogError($"{DateTime.Now.ToString("HH:mm:ss.fff")} OnSocketMessageErrorReceived: errorCode: {packetBase.error.code}", XDebug.Mask.Networking);
        }

        #endregion

        #region Unity
        protected override void Awake()
        {
            base.Awake();
            mSigSocketConnectionStatus.AddListener(OnSocketConnectionUpdate);
            mSigSocketConnectionStatusV2.AddListener(OnSocketConnectionUpdate);
            mSigOnSocketMessageErrorReceived.AddListener(OnSocketMessageErrorReceived);
            ConnectivityMonitor.AddListener(OnConnectionStatusChanged);
        }

        protected Dictionary<string, string> GetHeaders() { return mHeaders; }

        protected override void OnDestroy()
        {
            mSigSocketConnectionStatusV2.RemoveListener(OnSocketConnectionUpdate);
            mSigSocketConnectionStatus.RemoveListener(OnSocketConnectionUpdate);
            mSigOnSocketMessageErrorReceived.RemoveListener(OnSocketMessageErrorReceived);
            ConnectivityMonitor.RemoveListener(OnConnectionStatusChanged);
            base.OnDestroy();
        }

        private void OnConnectionStatusChanged(ConnectivityMonitor.Status status)
        {
            if (status == ConnectivityMonitor.Status.Online)
            {
                OnInternetAvailable();
            }
            else if (status == ConnectivityMonitor.Status.Offline)
            {
                mDisconnected = true;
                OnInternetNotAvailable();
            }
        }

        protected virtual void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                mTimeToReconnectAgain = 0;
                DisconnectSocket();
                Reconnect();
            }
        }

        protected virtual void OnSocketConnectionUpdate(long socketId, bool connected, WebSocketStatus socketStatus)
        {
            if (XDebug.CanLog(XDebug.Mask.Networking))
                XDebug.Log($"OnSocketConnectionUpdate=> socketId:{socketId}, connected:{connected}, socketStatus:{socketStatus}");

            if (!connected)
            {
                if (socketStatus == WebSocketStatus.AbnormalDisconnection)
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.Log($"AbnormalDisconnection by Server {DateTime.Now.ToString("HH: mm:ss.fff")}", XDebug.Mask.Networking);
                    mCurrentRetryCountToConnect = mTotalRetryCountToConnect;
                }

                if (mCurrentRetryCountToConnect >= mTotalRetryCountToConnect)
                {
                    ExceedMaxRetryOnSocketDisconnection();
                    mSigOnMaxConnectionRetryExceededV2.Dispatch(socketId);
                    return;
                }
                if (ConnectivityMonitor.pIsInternetAvailable)
                {
                    mSigTimerRegister.Dispatch((dt) =>
                    {
                        Reconnect(socketId);
                    }, mTimeToReconnectAgain);

                    mTimeToReconnectAgain = mMaxTimeToReconnectAgain;
                    mCurrentRetryCountToConnect++;
                }
                OnSocketDisconnected(socketId);
            }
            else
            {
                mDisconnected = null;
                mTimeToReconnectAgain = .5f;
                mCurrentRetryCountToConnect = 0;
                OnSocketConnected(socketId);
            }
        }

        protected virtual void OnSocketConnectionUpdate(bool connected, WebSocketStatus socketStatus)
        {
            if (!connected)
            {
                if (socketStatus == WebSocketStatus.AbnormalDisconnection)
                {
                    if (XDebug.CanLog(XDebug.Mask.Networking))
                        XDebug.Log("AbnormalDisconnection by Server", XDebug.Mask.Networking);
                    mCurrentRetryCountToConnect = mTotalRetryCountToConnect;
                }

                if (mCurrentRetryCountToConnect >= mTotalRetryCountToConnect)
                {
                    ExceedMaxRetryOnSocketDisconnection();
                    return;
                }
                if (ConnectivityMonitor.pIsInternetAvailable)
                {
                    mSigTimerRegister.Dispatch((dt) =>
                    {
                        Reconnect();
                    }, mTimeToReconnectAgain);

                    mTimeToReconnectAgain = mMaxTimeToReconnectAgain;
                    mCurrentRetryCountToConnect++;
                }
                OnSocketDisconnected();
            }
            else
            {
                mDisconnected = null;
                mTimeToReconnectAgain = .5f;
                mCurrentRetryCountToConnect = 0;
                OnSocketConnected();
            }
        }
        #endregion
    }
}
#endif