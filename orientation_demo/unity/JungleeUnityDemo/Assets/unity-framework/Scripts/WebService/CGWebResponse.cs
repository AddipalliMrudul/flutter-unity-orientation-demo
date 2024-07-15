namespace XcelerateGames
{
    [System.Serializable]
    public class CGWebResponse
    {
        public int code = -1;
        public string message = null;

        public override string ToString()
        {
            return "Code : " + code + ", Message : " + message;
        }

        public static string GetErorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 200:
                    return "Success";

                case 300:
                    return "Warning - Suspicious operation";

                case 500:
                    return "Internal Error";

                case 501:
                    return "Hash Check Failed";

                case 502:
                    return "Guest Init Failed";

                case 503:
                    return "FB Connect Failed";

                case 504:
                    return "Sync: Guest Sync Failed";

                case 505:
                    return "Sync: FB Sync Failed";

                case 506:
                    return "Payments: Invalid Platform";

                case 507:
                    return "Payments: Verification Failed";

                default:
                    return "Unknown Error";
            }
        }
    }
}