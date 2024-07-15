using System;
using System.Globalization;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for Float
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// This extension method will format double upto input given decimal places only if it has value
        /// at decimal points and returns a string
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimalPlaces">specify the required decimal places. Default value is 2</param>
        /// <returns></returns>
        public static string FormatUptoDecimalPlaces(this float num, int decimalPlaces = 2)//9.000001
        {
            string value = num.ToString();
            try
            {
                if ((num % 1) == 0)
                    return value;
                else
                    return float.Parse(value.Substring(0, Mathf.Min(value.Length, value.IndexOf(".") + decimalPlaces + 1)), CultureInfo.InvariantCulture).ToString();
            }
            catch (Exception e)
            {
                XDebug.LogException($"FloatExtensions::FormatUptoDecimalPlaces:Failed to parse, {nameof(num)}:{num}, {nameof(value)}:{value}, {nameof(decimalPlaces)}:{decimalPlaces}\n{e.Message}\n{e.StackTrace}");
                return num.ToString();
            }
        }

        /// <summary>
        /// Lerp between the range
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float Lerp(float from, float to, float time)
        {
            return from + ((to - from) * time);
        }

        public static string FormatUptoTwoDecimalPlaces(this float num)
        {
            return num.ToString("0.##");
        }

        public static string FormatRegardingPattern(this float num,string pattern)
        {
            try
            {
                return num.ToString(pattern);
            }
            catch (Exception e)
            {
                XDebug.LogException($"FloatExtensions::FormatUptoDecimalPlaces:Failed to parse, {nameof(num)}:{num}, {nameof(pattern)}:{pattern}\n{e.Message}\n{e.StackTrace}");
                return num.ToString();
            }
        }
    }
}