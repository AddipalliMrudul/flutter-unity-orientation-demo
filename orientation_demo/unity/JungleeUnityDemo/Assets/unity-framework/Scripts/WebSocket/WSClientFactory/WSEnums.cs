namespace XcelerateGames.Socket
{
    public enum WSStates : ushort
    {
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3,
        Unknown
    }

    public enum WSCloseStatusCode : ushort
    {
        /// <summary>
        /// Equivalent to close status 1000. Indicates normal close.
        /// </summary>
        Normal = 1000,
        /// <summary>
        /// Equivalent to close status 1001. Indicates that an endpoint is
        /// going away.
        /// </summary>
        Away = 1001,
        /// <summary>
        /// Equivalent to close status 1002. Indicates that an endpoint is
        /// terminating the connection due to a protocol error.
        /// </summary>
        ProtocolError = 1002,
        /// <summary>
        /// Equivalent to close status 1003. Indicates that an endpoint is
        /// terminating the connection because it has received a type of
        /// data that it cannot accept.
        /// </summary>
        UnsupportedData = 1003,
        /// <summary>
        /// Equivalent to close status 1004. Still undefined. A Reserved value.
        /// </summary>
        Undefined = 1004,
        /// <summary>
        /// Equivalent to close status 1005. Indicates that no status code was
        /// actually present. A Reserved value.
        /// </summary>
        NoStatus = 1005,
        /// <summary>
        /// Equivalent to close status 1006. Indicates that the connection was
        /// closed abnormally. A Reserved value.
        /// </summary>
        Abnormal = 1006,
        /// <summary>
        /// Equivalent to close status 1007. Indicates that an endpoint is
        /// terminating the connection because it has received a message that
        /// contains data that is not consistent with the type of the message.
        /// </summary>
        InvalidData = 1007,
        /// <summary>
        /// Equivalent to close status 1008. Indicates that an endpoint is
        /// terminating the connection because it has received a message that
        /// violates its policy.
        /// </summary>
        PolicyViolation = 1008,
        /// <summary>
        /// Equivalent to close status 1009. Indicates that an endpoint is
        /// terminating the connection because it has received a message that
        /// is too big to process.
        /// </summary>
        TooBig = 1009,
        /// <summary>
        /// Equivalent to close status 1010. Indicates that a client is
        /// terminating the connection because it has expected the server to
        /// negotiate one or more extension, but the server did not return
        /// them in the handshake response.
        /// </summary>
        MandatoryExtension = 1010,
        /// <summary>
        /// Equivalent to close status 1011. Indicates that a server is
        /// terminating the connection because it has encountered an unexpected
        /// condition that prevented it from fulfilling the request.
        /// </summary>
        ServerError = 1011,
        /// <summary>
        /// Equivalent to close status 1015. Indicates that the connection was
        /// closed due to a failure to perform a TLS handshake. A Reserved value.
        /// </summary>
        TlsHandshakeFailure = 1015
    }
}