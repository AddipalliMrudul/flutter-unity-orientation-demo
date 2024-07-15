using System;

namespace XcelerateGames.Socket
{
    public class WSCloseEventArgs : EventArgs
    {
        public ushort Code { get; private set; }
        public string Reason { get; private set; }

        public WSCloseEventArgs(ushort code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }

    public class WSMessageEventArgs : EventArgs
    {
        public string Data;
    }

    public class WSErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public string Message { get; private set; }

        public WSErrorEventArgs(Exception exception, string msg)
        {
            Exception = exception;
            Message = msg;
        }
    }
}