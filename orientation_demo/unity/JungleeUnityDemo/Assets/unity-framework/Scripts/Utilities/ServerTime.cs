using System;

namespace XcelerateGames.Timer
{
    public class ServerTime
    {
        private static double mUnixTime = 0;
        private static double mUnixTimeMilliseconds = 0;

        public static double pCurrentTime { get { return mUnixTime + TimeUpdater.Instance.pElapsedTime; } }
        public static double pCurrentTimeMilliSeconds { get { return mUnixTimeMilliseconds + TimeUpdater.Instance.pElapsedTimeMilliSeconds; } }
        public static DateTime pDateTime { get { return TimeUtilities.GetEpoch2DateTime(pCurrentTime); } }

        public static void Init(double unixTime)
        {
            mUnixTime = unixTime;

            Init();
            TimeUpdater.Instance.ResetSec();
            TimeUpdater.Instance.ResetMS();

            //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log($"Server Time : {pDateTime.ToString()}");
        }

        public static void InitMilliSeconds(double unixTime)
        {
            mUnixTimeMilliseconds = unixTime;

            Init();
            TimeUpdater.Instance.ResetMS();
        }

        public static void Init()
        {
            TimeUpdater.Init();
        }
    }
}