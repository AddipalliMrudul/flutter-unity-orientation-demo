namespace XcelerateGames.Socket
{
    public enum WebSocketStatus
    {
        None,
        Connecting,             /**<Trying to establish a connection */
        Connected,              /**<Connection established */
        Disconnecting,          /**<Trying to disconnect */
        Disconnected,           /**<Disconnected */
        Closed,                 /**<Explicitly closed connection by client */
        AbnormalDisconnection,  /**<Abnormal Disconnection by the server*/
    }
}
