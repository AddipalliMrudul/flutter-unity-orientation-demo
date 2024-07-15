using System;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for Double
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// This extension method will format double upto 2 decimal places irrespective it has value
        /// at decimal points or not and returns a string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FormatUpToTwoDecimalPlaces(this double num)
        {
            return String.Format("{0:0.00}", num);
        }

        public static string FormatCommasInThousandsPlace(this double num)
        {
            return $"{num:n0}";
        }

        /// <summary>
        /// This extension method will format double upto 2 decimal places only if is has value
        /// at decimal points and returns a string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FormatUpToTwoDecimalPlacesOnlyIfItHasAValAtDecimalPlace(this double num)
        {
            //it doesn't have value past decimal point
            if ((num % 1) == 0)
                return num.ToString();
            else
                return String.Format("{0:0.00}", num);
        }

        /// <summary>
        /// This extension method will format double upto input given decimal places only if it has value
        /// at decimal points and returns a string
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimalPlaces">specify the required decimal places. Default value is 2</param>
        /// <returns></returns>
        public static string FormatUptoDecimalPlaces(this double num, int decimalPlaces = 2)
        {
            string value = num.ToString();
            try
            {
                if ((num % 1) == 0)
                    return value;
                else
                    return double.Parse(value.Substring(0, Mathf.Min(value.Length, value.IndexOf(".") + decimalPlaces + 1))).ToString();
            }
            catch (Exception e)
            {
                XDebug.LogException($"DoubleExtensions::FormatUptoDecimalPlaces:Failed to parse, {nameof(num)}:{num}, {nameof(value)}:{value}, {nameof(decimalPlaces)}:{decimalPlaces}\n{e.Message}\n{e.StackTrace}");
                return num.ToString();
            }
        }
    }
}