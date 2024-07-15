using System.Globalization;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for Integer
    /// </summary>
    public static class IntExtensions
    {
        /// <summary>
        /// Check if the given integer value falls in the given range
        /// @note range check is inclusive of max value
        /// </summary>
        /// <param name="num">the number to check</param>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <returns>true if in range, else false</returns>
        public static bool IsInRange(this int num, int min, int max)
        {
            return num >= min && num <= max;
        }

        /// <summary>
        /// Returns the string by padding required number of character at the beginning.
        /// </summary>
        /// <param name="number">number of digits to add</param>
        /// <param name="digitToPad">the number</param>
        /// <returns></returns>
        public static string ToString(this int number, int digitToPad)
        {
            string text = number.ToString();
            int digitsToPad = digitToPad - text.Length;
            for (int i = 0; i < digitsToPad; ++i)
            {
                text = "0" + text;
            }
            return text;
        }

        public static string ToUSNumber(this int num)
        {
            return num.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
        }

        /// <summary>
        /// Returns the next number in loop
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(this int num, int min, int max)
        {
            ++num;
            if (num > max)
                num = min;
            return num;
        }

        /// <summary>
        /// Returns the previous number in loop
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Previous(this int num, int min, int max)
        {
            --num;
            if (num < min)
                num = max;
            return num;
        }
    }
}