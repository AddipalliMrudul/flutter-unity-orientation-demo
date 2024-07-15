using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using XcelerateGames.FuzzyStrings;
using System.Collections.Generic;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for string operations
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Shuffle the order of characters in the given string
        /// </summary>
        /// <param name="str">String to shuffle</param>
        /// <returns>Shuffled string</returns>
        public static string Shuffle(this string str)
        {
            char[] array = str.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }

        /// <summary>
        /// Check if the given string is email
        /// </summary>
        /// <param name="str">string to be evaluated</param>
        /// <returns>true if the string is email else false</returns>
        public static bool IsEmail(this string str)
        {
            string pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(str);
            return match.Success;
        }

        /// <summary>
        /// Check if the given string is a number
        /// </summary>
        /// <param name="str">string to be evaluated</param>
        /// <returns>true if the string is number else false</returns>
        public static bool IsNumber(this string str)
        {
            string pattern = @"^[0-9]*$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(str);
            return match.Success;
        }

        /// <summary>
        /// Helper function to validate UPI id.
        /// Rules for valid UPI id
        /// 1. Contains alphanumeric characters.
        /// 2. Should always contain a ‘@’ character.
        /// 3. Should not have white spaces.
        /// 4. May have a dot(.) or hyphen(-).
        /// 5. Atleast 3 characters before & after the @ symbol
        /// </summary>
        /// <param name="upiId">UPI ID to be validated</param>
        /// <returns>true if valid, false otherwise</returns>
        public static bool IsUPIID(this string upiId)
        {
            string pattern = "^[a-zA-Z0-9.-]{2,256}@[a-zA-Z][a-zA-Z]{2,64}$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(upiId);
            return match.Success;
        }
        /// <summary>
        /// Convert the given string to the specified type of enum
        /// </summary>
        /// <typeparam name="T">Type of enum to convert to</typeparam>
        /// <param name="str">string that has enum value</param>
        /// <returns>Enum of type T</returns>
        public static T ToEnum<T>(this string str) where T : struct
        {
            T t = default(T);
            if (Enum.TryParse<T>(str, out t))
                return t;
            else
            {
                XDebug.LogError($"Failed to parse {str} to type {typeof(T)}");
                return t;
            }
        }

        /// <summary>
        /// Extract a number within a string
        /// </summary>
        /// <param name="inString">String that has a number in it</param>
        /// @example: "Hi there 123 is here", returns 123
        /// <returns>extracted number</returns>
        public static int ExtractNumber(this string inString)
        {
            try
            {
                Match match = Regex.Match(inString, @"(\d+)");
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
                else
                    throw new Exception("Failed to extract number from string : " + inString);
            }
            catch (Exception e)
            {
                XDebug.LogException($"Function : ExtractNumber --> Exception while extracting a number from string \n{e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Extract a number within a string
        /// </summary>
        /// <param name="inString">String that has a number in it</param>
        /// @example: "Hi there 123 is here", returns 123 as string
        /// <returns>extracted number in string</returns>
        public static string ExtractNumberAsString(this string inString)
        {
            try
            {
                Match match = Regex.Match(inString, @"(\d+)");
                if (match.Success)
                    return match.Groups[1].Value;
                else
                    throw new Exception("Failed to extract number from string : " + inString);
            }
            catch (Exception e)
            {
                XDebug.LogException($"Function : ExtractNumberAsString --> Exception while extracting a number from string \n{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extract all numbers within a string
        /// </summary>
        /// <param name="inString"></param>
        /// <returns>list of numbers in the given string</returns>
        /// @note +/- signs are ignored
        public static List<long> ExtractNumbers(this string inString)
        {
            List<string> numberStrings = new List<string>();

            string s = null;
            foreach (char ch in inString)
            {
                if (char.IsDigit(ch))
                    s += ch;
                else
                {
                    if (!s.IsNullOrEmpty())
                    {
                        AddToList();
                    }
                }
            }
            //For the last number
            AddToList();
            List<long> numbers = numberStrings.ConvertAll<long>(e => long.Parse(e));

            void AddToList()
            {
                if (!s.IsNullOrEmpty())
                {
                    numberStrings.Add(s);
                    s = null;
                }
            }
            return numbers;
        }

        /// <summary>
        /// Check if the given string is null or empty
        /// </summary>
        /// <param name="str">string to evaluate</param>
        /// <returns>true is null or empty else false</returns>
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        /// <summary>
        /// Removes decimal points if the values after decimal point is zero
        /// @example: 5.0 returns 5 & 5.2 returns 5.2
        /// </summary>
        /// <param name="str">string to evaluate</param>
        /// <returns></returns>
        public static string StripDecimalPoints(this string str)
        {
            if (str.Contains("."))
                str = str.TrimEnd('0').TrimEnd('.');
            return str;
        }

        /// <summary>
        /// If number of characters in a given string is greater than maxChars then 
        /// Returns a string upto number of characters provided as argument along with ellipsis, intentionally omitting the letters beyond that
        /// else returns the string as it is
        /// </summary>
        /// <param name="value"> the string itself</param>
        /// <param name="maxChars">maximum characters required in the string</param>
        /// <returns>string with ellipsis if number of characters in string is greater than maxChars</returns>
        public static string Ellipsis(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        /// <summary>
        /// Converts the given string to title case. 
        /// </summary>
        /// <param name="s">string to format</param>
        /// <returns>string</returns>
        /// ### Example
        /// @code
        /// hello world => Hello World
        /// @endcode
        /// @note If all letters are in upper case, convert to lower case first & then call this function
        public static string ToTitleCase(this string s) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());

        /// <summary>
        /// Convert the first character of the given string to uppser case.
        /// </summary>
        /// <param name="input">string to format</param>
        /// <returns>string</returns>
        /// ### Example
        /// @code
        /// hello world => Hello world
        /// @endcode
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };

        /// <summary>
        /// Remove duplicate characters from the given string
        /// </summary>
        /// <param name="input">string to evaluate</param>
        /// <returns>string after removing duplicate characters</returns>
        public static string RemoveDuplicateCharacters(this string input)
        {
            return new string(input.ToCharArray().Distinct().ToArray());
        }

        /// <summary>
        /// Checks if the given string has duplicate characters.
        /// </summary>
        /// @example: if we pass ABCD, it returns false
        /// @example: if we pass ABCA, it returns true
        /// <param name="input">string to be evaluated</param>
        /// <returns>true if contains duplicate charatcres else false</returns>
        public static bool ContainsDuplicateCharacters(this string input)
        {
            return RemoveDuplicateCharacters(input).Length < input.Length;
        }

        /// <summary>
        /// Convert a string separated by a delimeter to int array
        /// </summary>
        /// <param name="str">delimiter separated string</param>
        /// <param name="separator">delimiter, default is comma separated</param>
        /// <returns>int array</returns>
        public static int[] ToIntArray(this string str, char separator = ',')
        {
            if (str.IsNullOrEmpty())
                return null;

            return Array.ConvertAll(str.Split(separator), e => int.Parse(e));
        }

        /// <summary>
        /// Convert a string separated by a delimeter to float array
        /// </summary>
        /// <param name="str">delimiter separated string</param>
        /// <param name="separator">delimiter, default is comma separated</param>
        /// <returns>float array</returns>
        public static float[] ToIntFloat(this string str, char separator = ',')
        {
            if (str.IsNullOrEmpty())
                return null;

            return Array.ConvertAll(str.Split(separator), e => float.Parse(e));
        }
        #region Fuzzy Comparisons
        public static bool FuzzyEqualsLD(this string strA, string strB, bool caseSensitive = false, double requiredProbabilityScore = 0.75, float coefficient = 0.2f)
        {
            if (caseSensitive)
            {
                if (strA.Equals(strB, StringComparison.Ordinal))
                    return true;
            }
            if (strA.Equals(strB, StringComparison.OrdinalIgnoreCase))
                return true;
            int leven = strA.LevenshteinDistance(strB, caseSensitive);
            double levenCoefficient = 1.0 / (1.0 * (leven + coefficient)); //may want to tweak offset

            return levenCoefficient > requiredProbabilityScore;
        }

        public static bool FuzzyStartsWithLD(this string strA, string strB, bool caseSensitive = false, double requiredProbabilityScore = 0.75, float coefficient = 0.2f)
        {
            if (caseSensitive)
            {
                if (strA.StartsWith(strB, StringComparison.Ordinal))
                    return true;
            }
            if (strA.StartsWith(strB, StringComparison.OrdinalIgnoreCase))
                return true;
            int length = Math.Min(strA.Length, strB.Length);
            int leven = strA.LevenshteinDistance(strB.Substring(0, length), caseSensitive);
            double levenCoefficient = 1.0 / (1.0 * (leven + coefficient)); //may want to tweak offset

            return levenCoefficient > requiredProbabilityScore;
        }

        #endregion Fuzzy Comparisons

        #region Compression
        /// <summary>
        /// Compress the given string
        /// </summary>
        /// <param name="str">uncompressed string</param>
        /// <returns>compressed byte array</returns>
        public static byte[] Zip(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// Uncompress the given bytes
        /// </summary>
        /// <param name="bytes">compressed bytes</param>
        /// <returns>Uncompressed string</returns>
        public static string Unzip(this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        /// <summary>
        /// Copy bytes
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Extension for comparing string value among multiple values/array 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valuesToCompare"></param>
        /// <returns>returns true, if value is in exists in array/multiple values</returns>
        public static bool IsIn(this string value, params string[] valuesToCompare)
        {
            foreach (var item in valuesToCompare)
            {
                if (value == item) return true;
            }
            return false;
        }
        #endregion Compression

        /// <summary>
        /// Extracts file name from the url. 
        /// </summary>
        /// <param name="inURL">url to extract from</param>
        /// <returns>file name</returns>
        /// ### Example
        /// http://aws.s3/somegame/pictures.png returns pictures.png 
        public static string FileNameFromURL(this string inURL)
        {
            try
            {
                string[] data = inURL.Split('/');
                return data[data.Length - 1];
            }
            catch (Exception e)
            {
                XDebug.LogException(e);
                return inURL;
            }
        }

        /// <summary>
        /// Returns a dict by splitting cookie data
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static Dictionary<string, string> SplitCookie(this string cookie)
        {
            List<string> dd = new List<string>(cookie.Split(';'));

            Dictionary<string, string> kvp = new Dictionary<string, string>();
            for (int i = 0; i < dd.Count; ++i)
            {
                if (dd[i].IsNullOrEmpty())
                    continue;
                string[] data = dd[i].Trim().Split('=');
                if (data.Length == 2)
                {
                    if (!kvp.ContainsKey(data[0]))
                        kvp.Add(data[0], data[1]);
                }
            }
            return kvp;
        }

        /// <summary>
        /// Returns params sent as query params
        /// ex: action=launch&ab_path=store&meta=subscriptions
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns>Dictionary of key & value</returns>
        public static Dictionary<string, string> GetQueryParams(this string queryParams)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (queryParams.IsNullOrEmpty())
                return data;
            queryParams = queryParams.TrimStart('?');
            string[] kvps = queryParams.Split('&');
            foreach (var kvp in kvps)
            {
                string[] item = kvp.Split('=');
                if (item.Length == 2)
                    data[item[0]] = item[1];
            }
            return data;
        }
    }
}
