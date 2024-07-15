#if USE_NATIVE_WEBSOCKET || USE_WEBSOCKET_SHARP

using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames.Socket
{
    #region Network 
    public class SigConnectSocket : Signal<string/*websocket url*/, float/*ping time in seconds*/, Dictionary<string, string> /*headers*/> { }
    public class SigSocketConnectionStatus : Signal<bool, WebSocketStatus> { }
    public class SigDisconnectSocket : Signal { }
    public class SigOnSocketMesageReceived : Signal<string, long /*connection id*/> { }
    public class SigSendSocketMesage : Signal<string> { }
    public class SigOnSocketMessageErrorReceived : Signal<SM_PacketBase> { }
    public class SigClearSocketIncomingMessage : Signal { }

    public class SigSocketConnectionStatusV2 : Signal<long/*socketid*/,bool, WebSocketStatus> { }
    public class SigConnectSocketV2 : Signal<string/*websocket url*/, long/*id*/, Dictionary<string, string> /*headers*/> { }
    public class SigDisconnectSocketV2 : Signal<long/*socket id*/> { }
    public class SigDisconnectAllSockets: Signal { }
    public class SigSendSocketMessageV2 : Signal<long, string> { }
    public class SigOnMaxConnectionRetryExceededV2 : Signal<long> { }

    #endregion//================================================================[ Network ]
}
#endif