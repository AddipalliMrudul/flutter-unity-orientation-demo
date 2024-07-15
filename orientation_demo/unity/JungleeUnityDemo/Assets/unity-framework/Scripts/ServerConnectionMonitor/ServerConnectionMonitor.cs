#if USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    /// <summary>
    /// This class checks connectivity with game server and internet connection and send updates to UI to show on Network Strength bar
    /// </summary>
    public class ServerConnectionMonitor : BaseBehaviour
    {
#region Signals
        [InjectSignal] protected SigSocketConnectionStatus mSigSocketConnectionStatus = null; /**<Dispatch these from SocketManager after connection done to start sending ping */
        //[InjectSignal] private SigServerConnectionClose mSigServerConnectionClose = null; /**<Dispatch these from SocketManager when connection closed by server*/
        [InjectSignal] private SigOnGetPong mSigOnGetPong = null; /**<Dispatch these from SocketManager won get pong*/
        [InjectSignal] private SigOnPingCountUpdate mSigOnPingCountUpdate = null; /**<Listenns this from Ui class to update network strength UI*/
        [InjectSignal] private SigOnDisconnection mSigOnDisconnection = null; /**<Dispatch these from SocketManager when disconnnection happenns*/
        //[InjectSignal] private SigGameQuit mSigGameQuit = null; /**<Dispatch this whenever game quits or player exits to lobby*/
        [InjectSignal] private SigOnReconnect mSigOnReconnect = null; /**<Listen this for try reconnection with server in socket manager*/
        [InjectSignal] private SigOnPingSend mSigOnPingSend = null; /**<Send ping to server using this signal*/
#endregion //Signals

#region Private Variables
        [SerializeField] protected int mPingDuration = 2; /**< Time interval between two pings to be send to server*/
        [SerializeField] protected int mTimeOutPing = 5; /**< if total number of consecutive ping count is greater than this number than consider time out*/
        [SerializeField] protected int mPoorConnectionPing = 13;  /**< Total number of ping counts after which we say that we are no longer connected to game server*/
        protected int _connectivityPingNum = 0; /**< range for ui status */
        protected int _consecutivePingMissCount = 0; /**< consecutively misseds ping count */

#endregion //Private Variables

#region Private Functions


        /// <summary>
        /// Add Listens on Start
        /// </summary>
        private void Start()
        {
            mSigSocketConnectionStatus.AddListener(OnSocketConnect);
            mSigOnGetPong.AddListener(OnPongRecieved);
            //mSigGameQuit.AddListener(ResetCounnts);
            //mSigServerConnectionClose.AddListener(ResetCounnts);
            mSigOnDisconnection.AddListener(OnDisconnect);
            mSigOnPingSend.AddListener(OnSendPing);
        }

        /// <summary>
        /// Remove listens OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            mSigSocketConnectionStatus.RemoveListener(OnSocketConnect);
            mSigOnGetPong.RemoveListener(OnPongRecieved);
            //mSigGameQuit.RemoveListener(ResetCounnts);
            //mSigServerConnectionClose.RemoveListener(ResetCounnts);
            mSigOnDisconnection.RemoveListener(OnDisconnect);
            mSigOnPingSend.RemoveListener(OnSendPing);
            base.OnDestroy();
        }

        /// <summary>
        /// Listen to SigSocketConnected
        /// Begin sending ping to server
        /// </summary>
        protected virtual void OnSocketConnect(bool connected, WebSocketStatus socketStatus)
        {
            XDebug.Log($"Is Socket Connected?: {connected}", XDebug.Mask.Networking);
            if(connected)
                ResetCounts();
        }

        /// <summary>
        /// Start sending ping to server
        /// </summary>
        protected virtual void ResetCounts()
        {
            _connectivityPingNum = 0;
            _consecutivePingMissCount = 0;
        }

        /// <summary>
        /// Send ping to server
        /// </summary>
        protected virtual void OnSendPing()
        {
            _connectivityPingNum += mPingDuration;
            _consecutivePingMissCount += 1;
            SendOutPingUpdateMsg(_connectivityPingNum);
        }

        /// <summary>
        /// Invokes whenever get pong from server
        /// Listen to SigOnGetPong
        /// </summary>
        protected void OnPongRecieved()
        {
            _connectivityPingNum -= mPingDuration;
            _consecutivePingMissCount = 0;
            SendOutPingUpdateMsg(_connectivityPingNum);
        }

        /// <summary>
        /// Dispatch signal SigOnPingCountUpdate to update Network strength bar on UI
        /// </summary>
        /// <param name="pingValue">total consecutive ping count</param>
        private void SendOutPingUpdateMsg(int pingValue)
        {
            mSigOnPingCountUpdate.Dispatch(pingValue);
        }

        /// <summary>
        /// On disconnection from server
        /// StopPing
        /// SendOutPingUpdateMsg
        /// Dispatch SigReconnect for try to connect again
        /// SendDisconnectionMessage
        /// </summary>
        protected virtual void OnDisconnect()
        {
            SendOutPingUpdateMsg(mPoorConnectionPing);
            mSigOnReconnect.Dispatch();
        }

#endregion //Private Functions
    }
}
#endif //USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP