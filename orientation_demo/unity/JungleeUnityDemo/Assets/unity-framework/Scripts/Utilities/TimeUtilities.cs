using System;
using System.Globalization;

namespace XcelerateGames
{
    public static class TimeUtilities
    {
        /// <summary>
        /// Use this function to show time in Days, Hours, mins & seconds.
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerString(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d {1:D1}h {2:D1}m", t.Days, t.Hours, t.Minutes);
            else if (t.Hours > 0)
                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
            else if (t.Minutes > 0)
                return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
            else
                return string.Format("{0:D2}s", t.Seconds);
        }

        /// <summary>
        /// Use this function to show time in Hours, mins & seconds.
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringFull(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes,t.Seconds);
        }

        /// <summary>
        /// Gets the standard timer string in HH:MM:SS format if within 24 hours, else in DD:HH:MM format
        /// </summary>
        /// <returns>The standard timer string.</returns>
        /// <param name="secs">Secs.</param>
        public static string GetStandardTimerString(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d {1:D1}h {2:D1}m", t.Days, t.Hours, t.Minutes);
            else
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
        }

        /// <summary>
        /// Returns time in DD:HH format if th time is above 24 hours else HH:MM,
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringShort(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d {1:D1}h", t.Days, t.Hours);
            else if (t.Hours > 0)
            {
                if (t.Minutes == 0)
                    return string.Format("{0:D2}h", t.Hours);
                else
                    return string.Format("{0:D2}h {1:D2}m", t.Hours, t.Minutes);
            }
            else if (t.Minutes > 0)
            {
                if (t.Seconds == 0)
                    return string.Format("{0:D2}m", t.Minutes);
                else
                    return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
            }
            else
                return string.Format("{0:D2}s", t.Seconds);
        }

        /// <summary>
        /// Returns time in DD:HH format if th time is above 24 hours else HH:MM, Doesnt check for seconds
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringShortWithoutSeconds(long secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d {1:D1}h", t.Days, t.Hours);
            else if (t.Hours > 0)
            {
                if (t.Minutes == 0)
                    return string.Format("{0:D2}h", t.Hours);
                else
                    return string.Format("{0:D2}h {1:D2}m", t.Hours, t.Minutes);
            }
            else
            {
                return string.Format("{0:D2}m", t.Minutes);
            }
        }

        /// <summary>
        /// Returns time in DD:HH format if th time is above 24 hours else HH:MM, with no h m & s
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringShortNoAbbreviations(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}:{1:D1}", t.Days, t.Hours);
            else if (t.Hours > 0)
                return string.Format("{0:D2}:{1:D2}", t.Hours, t.Minutes);
            else
                return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        /// <summary>
        /// Returns time in DD:HH format if th time is above 24 hours else HH:MM:SS, with no h m & s
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringShortNoAbbreviationsWithHour(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d {1:D1}h", t.Days, t.Hours);
            else if (t.Hours > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes,t.Seconds);
            else
                return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        /// <summary>
        /// Use this when time less than a day, else use the other function that takes a long as parameter
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerString(int secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);

            return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
        }

        /// <summary>
        /// Returns time in DD:HH format if th time is above 24 hours else HH:MM else MM:SS,
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string GetTimerStringShortWithTwoFields(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            if (t.Days >= 1)
                return string.Format("{0:D1}d:{1:D1}h", t.Days, t.Hours);
            else if (t.Hours > 0)
                return string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);

            return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
        }

        public static string GetTimerString(TimeSpan ts)
        {
            if (ts.Days > 0)
                return ts.Days + "d " + ts.Hours + "h " + ts.Minutes + "m";
            else if (ts.Hours > 0)
                return string.Format("{0:D2}h {1:D2}m", ts.Hours, ts.Minutes);
            else if (ts.Minutes > 0)
                return string.Format("{0:D2}m:{1:D2}s", ts.Minutes, ts.Seconds);
            else
                return string.Format("{0:D2}s", ts.Seconds);
        }

        //Returns time in mmm dd, yyyy format. Ex Jan 04, 2017
        public static string GetTimerString(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month) + " " + dateTime.Day + ", " + dateTime.Year;
        }

        /// <summary>
        /// Converts the given epoch time to himan readable format
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public static string GetEpoch2String(long epoch)
        {
            return GetEpoch2DateTime(epoch).ToShortDateString();
        }

        /// <summary>
        /// Converts epoch to date time
        /// </summary>
        /// <returns>The epoch2 date time.</returns>
        /// <param name="epoch">Epoch.</param>
        public static DateTime GetEpoch2DateTime(double epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }

        /// <summary>
        /// Converts epoch to date local time
        /// </summary>
        /// <returns>The epoch2 local date time.</returns>
        /// <param name="epoch">Epoch.</param>
        public static DateTime GetEpoch2LocalDateTime(double epoch)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(epoch).ToLocalTime();
            return dateTime;
        }

        public static TimeSpan GetEpoch2TimeSpan(long epoch)
        {
            return TimeSpan.FromSeconds(epoch);
        }

        /// <summary>
        /// Returns current time in epoch format
        /// </summary>
        /// <returns></returns>
        public static long GetEpochTime()
        {
            return GetEpochTime(DateTime.UtcNow);
        }

        public static long GetEpochTimeInMilliseconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
        /// <summary>
        /// Get Epoch from DateTime
        /// </summary>
        /// <returns></returns>
        public static long GetEpochTime(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long SecondsToMinutes(long seconds)
        {
            return (long)(seconds / 60f);
        }
    }
}