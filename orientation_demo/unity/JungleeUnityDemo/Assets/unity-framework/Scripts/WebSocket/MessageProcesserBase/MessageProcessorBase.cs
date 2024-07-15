#if USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;
using XcelerateGames.Timer;

namespace XcelerateGames.Socket
{
    public class MessageProcessorBase : BaseBehaviour
    {
        #region protected variables
        /// <summary>
        /// This dictionary is used to add actions regarding server packet code.
        /// </summary>
        protected Dictionary<int, Action<long, string>> mCmdMapping = null;
        #endregion

        #region Signals
        /// <summary>
        /// This signal is fired when we get message from server
        /// </summary>
        [InjectSignal] protected SigOnSocketMesageReceived mSigOnSocketMesageReceived = null;
        [InjectSignal] protected SigOnSocketMessageErrorReceived mSigOnSocketMessageErrorReceived = null;
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            mSigOnSocketMesageReceived.AddListener(OnSocketMessageReceived);
            AddCommandListeners();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigOnSocketMesageReceived.RemoveListener(OnSocketMessageReceived);
        }
        #endregion

        #region Other functions
        /// <summary>
        /// Override this functions to add action to  mCmdMapping Dictionary. Do not forget to call base.AddCommandListeners()
        /// to initialize this dictionary
        /// </summary>
        protected virtual void AddCommandListeners()
        {
            mCmdMapping = new Dictionary<int, Action<long, string>>();
        }

        /// <summary>
        /// This is the function which receive server message in string. You can override it to implement your
        /// code base or can leave it.
        /// </summary>
        /// <param name="msg">Server message</param>
        protected virtual void OnSocketMessageReceived(string msg, long id)
        {
            try
            {
                SM_PacketBase packetBase = msg.FromJson<SM_PacketBase>();
                packetBase.socketId = id;
                if (packetBase.meta != null && packetBase.type != BaseCommandType.REQ_PING_PONG)
                    ServerTime.Init(packetBase.meta.ts);
                if (XDebug.CanLog(XDebug.Mask.Networking) && packetBase.type != BaseCommandType.REQ_PING_PONG)
                    XDebug.Log($"Network Msg Received id:{id} : {GetRedablePacketName(packetBase.type)} : {msg}", XDebug.Mask.Networking);
                if (packetBase.success)
                {
                    OnSocketMessageReceivedSuccess(packetBase, msg);
                }
                else
                {
                    OnSocketMessageReceivedError(packetBase, msg);
                }
            }
            catch (JsonException e)
            {
                XDebug.LogException($"Failed to deserialize {msg} \nError = {e.Message}");
            }
        }

        /// <summary>
        /// This functions is called when server sends a success message. You can override it to implement your
        /// code base.
        /// </summary>
        /// <param name="packetBase">server message object</param>
        /// <param name="msg">server message</param>
        protected virtual void OnSocketMessageReceivedSuccess(SM_PacketBase packetBase, string msg)
        {
            if (packetBase.type == BaseCommandType.REQ_PING_PONG)
            {
            }
            else
            {
                if (mCmdMapping.ContainsKey(packetBase.type))
                {
                    try
                    {
                        string data = msg.GetJsonNode("data");
                        mCmdMapping[packetBase.type]?.Invoke(packetBase.socketId, data);
                    }
                    catch (Exception e)
                    {
                        XDebug.LogException($"Failed to parse server message: {e.Message}");
                    }
                }
                else
                {
                    XDebug.LogWarning($"No Mapping found for command: {packetBase.type}");
                }
            }
        }

        /// <summary>
        /// This functions is called when server sends an error message. You can override it to implement your
        /// code base.
        /// </summary>
        /// <param name="packetBase">server message object</param>
        /// <param name="msg">server message</param>
        protected virtual void OnSocketMessageReceivedError(SM_PacketBase packetBase, string msg)
        {
            mSigOnSocketMessageErrorReceived.Dispatch(packetBase);
        }

        /// <summary>
        /// Return a human readable packet name. Base class will not do any computation here.
        /// Derived class is responsible for handling the logic
        /// </summary>
        /// <param name="type">type of packet</param>
        /// <returns>packet name</returns>
        protected virtual string GetRedablePacketName(int type)
        {
            return null;
        }
        #endregion
    }
}
#endif //USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP