namespace XcelerateGames.Socket
{
    public enum WSClient
    {
        WebSocketSharp = 0,//https://github.com/sta/websocket-sharp.git
        BestWebSocket = 1 //https://assetstore.unity.com/packages/tools/network/best-websockets-bundle-268838
    }

    public class WSClientFactory
    {
        public IWSClient GetWebSockeClient(WSClient clientType)
        {
            if (clientType == WSClient.WebSocketSharp)
            {
#if USE_WEBSOCKET_SHARP
                return new WSSharp();
#endif
            }
            else if (clientType == WSClient.BestWebSocket)
            {
#if USE_BEST_WEBSOCKET
             return new BestWebSocket();
#endif
            }
            XDebug.LogError($"WSClientFactory :: Couldn't find the implementation of the client type {clientType}");
            return null;
        }
    }
}
