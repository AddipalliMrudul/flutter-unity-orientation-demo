using System;
using System.Collections.Generic;

namespace XcelerateGames.Socket
{
    public interface IWSClient
    {
        public WSStates ReadyState { get; }

        public bool IsAlive { get; }

        public void Connect(string url, Dictionary<string, string> headers, EventHandler eventCallback);

        public void OnSendSocketMessage(string packetData);

        public void Disconnect();

        public void OnSendSocketMessage(PacketBase packetData);

        public void Dispose();
    }
}