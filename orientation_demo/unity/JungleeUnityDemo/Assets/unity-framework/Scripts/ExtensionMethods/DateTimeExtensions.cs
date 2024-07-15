using System;
using System.Globalization;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for DateTime object. Helps in getting formatted date & time.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns full name of the day. Sunday, Monday, Tuesday etc
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Monday, Tuesday, Friday etc</returns>
        public static string GetDayName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dateTime.DayOfWeek);
        }

        /// <summary>
        /// Returns name of the day in two letters Su, Mo, Tu etc
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns><Su, Mo, Tu etc/returns>
        public static string GetShortestDayName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(dateTime.DayOfWeek);
        }

        /// <summary>
        /// Returns name of tha day in three letters Sun, Mon, Tue etc
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Sun, Mon, Tue etc</returns>
        public static string GetAbbreviatedDayName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(dateTime.DayOfWeek);
        }

        /// <summary>
        /// Returns three letter name of the month. Example Jan, Feb, Mar etc
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Jan, Feb, Mar etc</returns>
        public static string GetAbbreviatedMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        /// <summary>
        /// Returns full name of the month. Ex January, February, March etc
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>January, February, March etc</returns>
        public static string GetMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        /// <summary>
        /// Returns {abbreviated day}, {Abbreviated Month} {day}. Ex : Sun, Jan 20
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Returns date in the form of Sun, Jan 20</returns>
        public static string Format(this DateTime dateTime)
        {
            return $"{dateTime.GetAbbreviatedDayName()}, {dateTime.GetAbbreviatedMonthName()}, {dateTime.Day}";
        }

        /// <summary>
        /// Returns {abbreviated day}, {Abbreviated Month} {day} {hours:Minites}. Ex : Sun, Jan 20 10:55
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Returns date in the form of Sun, Jan 20 10:55</returns>
        public static string FormatWithTime(this DateTime dateTime)
        {
            return $"{dateTime.GetAbbreviatedDayName()}, {dateTime.GetAbbreviatedMonthName()} {dateTime.Day} {dateTime.Hour}:{dateTime.Minute}";
        }

        //Returns {Abbreviated Month} {Year} {day}. Ex : Jan 20, 2021 10:55
        public static string FormatWithTime2(this DateTime dateTime)
        {
            return $"{dateTime.GetAbbreviatedMonthName()} {dateTime.Day}, {dateTime.Year} {dateTime.Hour}:{dateTime.Minute}";
        }

        /// <summary>
        /// Returns date formatted with milliseconds. Ex: yyyy-MM-dd HH:mm:ss.fff
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Return date in yyyy-MM-dd HH:mm:ss.fff format</returns>
        public static string FormatDateTimeWithMS(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Returns {day}, {Abbreviated Month} {year}. Ex : 17, Jan 1980
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Returns date in the form: 17, Jan 1980</returns>
        public static string FormatWithDate(this DateTime dateTime)
        {
            return $"{dateTime.Day}, {dateTime.GetAbbreviatedMonthName()} {dateTime.Year}";
        }

        /// <summary>
        /// Returns {day}, {Abbreviated Month} {year}. Ex : Tue, Jun 17, 1980
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>Returns date in the form: Tue, Jun 17, 1980</returns>
        public static string FormatWithDateAndYear(this DateTime dateTime)
        {
            return $"{dateTime.GetAbbreviatedDayName()}, {dateTime.GetAbbreviatedMonthName()} {dateTime.Day}, {dateTime.Year}";
        }

        /// <summary>
        /// Returns {day}, {Abbreviated Month} {year}. Ex : Tue, Jun 17, 1980, 10:10 AM
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>return date in the form: Tue, Jun 17, 1980, 10:10 AM</returns>
        public static string FormatWithDateYearAndTime(this DateTime dateTime)
        {
            return $"{dateTime.GetAbbreviatedDayName()}, {dateTime.GetAbbreviatedMonthName()} {dateTime.Day}, {dateTime.Year}, {dateTime.ToString("hh:mm tt")}";
        }

        /// <summary>
        /// Returns DateTime object by extracting Time from given string. Default format is 24 hours format
        /// For 12 hour format use "hh:mm:ss"
        /// </summary>
        /// <param name="time">time</param>
        /// <param name="format">format to be used</param>
        /// <returns></returns>
        public static DateTime ToTime(this string time, string format = "HH:mm:ss")
        {
            return DateTime.ParseExact(time, format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns age as of now with respect to DOB.
        /// </summary>
        /// <param name="dateOfBirth">DateTime object</param>
        /// <returns>Age of of today</returns>
        public static int GetAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

            return (a - b) / 10000;
        }

        /// <summary>
        /// Returns epoch time
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>epoch time in seconds</returns>
        public static long GetEpochTime(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Returns epoch time in milliseconds
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns>epoch time in milliseconds</returns>
        public static long GetEpochTimeInMilliseconds(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Returns epoch time in milliseconds for the current DateTime
        /// </summary>
        /// <returns></returns>
        public static long GetEpochTimeInMilliseconds()
        {
            return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Helper function to get date & time stamp in milliseconds.
        /// Written to be used for debuggig purpose
        /// </summary>
        /// <returns></returns>
        public static string DebugTimeStamp()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }

        /// <summary>
        /// Helper function to get time stamp in milliseconds
        /// Written to be used for debuggig purpose
        /// </summary>
        /// <returns></returns>
        public static string DebugDateTimeStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
