using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for Dictionary & SortedDictionary classes
    /// </summary>
    public static class DictionaryExtensionMethods
    {
        #region Dictionary
        /// <summary>
        /// Add or update an element. If the elemenst exists, its value is updated else added to the Dictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Dictionary with type T as key & W as value</param>
        /// <param name="key">Key in dict</param>
        /// <param name="value">Value to be added or updated</param>
        /// @see AddOrUpdate<T,W>(SortedDictionary<T, W>, T, W)
        public static void AddOrUpdate<T, W>(this Dictionary<T, W> dict, T key, W value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        /// <summary>
        /// Get a random key from dictionary.
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Dictionary with type T as key & W as value</param>
        /// <returns>Random key of type T</returns>
        public static T RandomKey<T, W>(this Dictionary<T, W> dict)
        {
            List<T> keys = new List<T>(dict.Keys);
            return keys[UnityEngine.Random.Range(0, keys.Count)];
        }

        /// <summary>
        /// Get a random value from dictionary.
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Dictionary with type T as key & W as value</param>
        /// <returns>Random value of type W</returns>
        public static W RandomValue<T, W>(this Dictionary<T, W> dict)
        {
            List<T> keys = new List<T>(dict.Keys);
            return dict[keys[UnityEngine.Random.Range(0, keys.Count)]];
        }

        /// <summary>
        /// Returns a key by value
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Dictionary with type T as key & W as value</param>
        /// <param name="val">value to search</param>
        /// <returns>key of type T that corresponds to given value</returns>
        public static T KeyByValue<T, W>(this Dictionary<T, W> dict, T val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (pair.Value.Equals(val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }

        /// <summary>
        /// Append one dictionary with elements of other dictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <param name="otherDict">Elements of this dict will be appended to dict</param>
        public static void Append<T, W>(this Dictionary<T, W> dict, Dictionary<T, W> otherDict)
        {
            if (otherDict != null)
            {
                foreach (KeyValuePair<T, W> valuePair in otherDict)
                {
                    dict.Add(valuePair.Key, valuePair.Value);
                }
            }
        }

        /// <summary>
        /// Checks if given dictionary is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <returns>true if null or empty, else false</returns>
        public static bool IsNullOrEmpty<T, W>(this Dictionary<T, W> dict)
        {
            if (dict == null || dict.Count == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Returns a subset of this dictionary containing elements that were added or modified when compared against the src dictionary.
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <param name="src"></param>
        /// <returns>new dictionary with a subset of elements</returns>
        public static Dictionary<T, W> AddedOrModifiedElements<T, W>(this Dictionary<T, W> dict, Dictionary<T, W> src)
        {
            Dictionary<T, W> addedOrModified = new Dictionary<T, W>();

            // Check for added or modified elements.
            foreach (var entry in dict)
            {
                if (src == null || !src.ContainsKey(entry.Key) || !entry.Value.Equals(src[entry.Key]))
                {
                    addedOrModified.Add(entry.Key, entry.Value);
                }
            }
            return addedOrModified;
        }

        /// <summary>
        /// Sort the dictionary by value
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <returns>new sorted dictionary</returns>
        public static List<KeyValuePair<T, W>> SortKeysByValue<T, W>(this Dictionary<T, W> dict) where W : IComparable
        {
            List<KeyValuePair<T, W>> myList = dict.ToList();
            myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            return myList;
        }

        /// <summary>
        /// Returns a string that can be printed with all key & values in the given dictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <returns>printable form of dictionary</returns>
        public static string Printable<T, W>(this Dictionary<T, W> dict)
        {
            string text = null;
            if (dict != null)
            {
                foreach (KeyValuePair<T, W> kvp in dict)
                {
                    text += $"{kvp.Key}:{kvp.Value}\n";
                }
            }
            return text;
        }

        /// <summary>
        /// Returns a string that can be printed with all key & values in the given dictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <param name="converter">Pointer to a function for conversion</param>
        /// <returns>printable form of dictionary</returns>
        public static string Printable<T, W>(this IDictionary<T, W> dict, Func<T,W,string> converter)
        {
            string text = null;
            if (dict != null)
            {
                foreach (KeyValuePair<T, W> kvp in dict)
                {
                    if (converter == null)
                        text += $"{kvp.Key}:{kvp.Value}\n";
                    else
                        text += converter(kvp.Key, kvp.Value) + "\n";
                }
            }
            return text;
        }
        /// <summary>
        /// Checks if the given dicttionary contains all keys given in the list
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <param name="keys">list of keys to check</param>
        /// <returns>true if all keys are present else false</returns>
        public static bool ContainsKeys<T, W>(this Dictionary<T, W> dict, List<T> keys)
        {
            bool found = true;
            if (dict.IsNullOrEmpty() || keys.IsNullOrEmpty())
                found = false;
            else
            {
                foreach (T key in keys)
                {
                    if (!dict.ContainsKey(key))
                        return false;
                }
            }
            return found;
        }

        /// <summary>
        /// Checks if the given dicttionary contains all keys given in the list
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Elements of otherDict will be appended to this dict</param>
        /// <param name="keys">array of keys to check</param>
        /// <returns>true if all keys are present else false</returns>
        public static bool ContainsKeys<T, W>(this Dictionary<T, W> dict, T[] keys)
        {
            bool found = true;
            if (dict.IsNullOrEmpty() || keys.IsNullOrEmpty())
                found = false;
            else
            {
                foreach (T key in keys)
                {
                    if (!dict.ContainsKey(key))
                        return false;
                }
            }
            return found;
        }

        /// <summary>
        /// Returns a query string to be appended to API URL
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string GetQueryString(this Dictionary<string,string> dict)
        {
            string queryString = "?";
            int count = 0;
            foreach(var e in dict)
            {
                queryString += $"{e.Key}={UnityWebRequest.EscapeURL(e.Value)}";
                if (count < dict.Count-1)
                    queryString += "&";
                ++count;
            }
            return queryString;
        }
        #endregion Dictionary

        #region Sorted Dictionary
        /// <summary>
        /// Add or update an element. If the elemenst exists, its value is updated else added to the SortedDictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in Dictionary</typeparam>
        /// <typeparam name="W">Type of value elements in Dictionary</typeparam>
        /// <param name="dict">Dictionary with type T as key & W as value</param>
        /// <param name="key">Key in dict</param>
        /// <param name="value">Value to be added or updated</param>
        /// @see AddOrUpdate<T,W>(Dictionary<T, W>, T, W)
        public static void AddOrUpdate<T, W>(this SortedDictionary<T, W> dict, T key, W value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        /// <summary>
        /// Returns a string that can be printed with all key & values in the given dictionary
        /// </summary>
        /// <typeparam name="T">Type of key elements in SortedDictionary</typeparam>
        /// <typeparam name="W">Type of value elements in SortedDictionary</typeparam>
        /// <param name="dict">Elements of this dict will be printed</param>
        /// <param name="converter">Pointer to a function for conversion</param>
        /// <returns>printable form of dictionary</returns>
        public static string Printable<T, W>(this SortedDictionary<T, W> dict, Func<T, W, string> converter = null)
        {
            string text = null;
            if (dict != null)
            {
                foreach (KeyValuePair<T, W> kvp in dict)
                {
                    if (converter == null)
                        text += $"{kvp.Key}:{kvp.Value}\n";
                    else
                        text += converter(kvp.Key, kvp.Value) + "\n";
                }
            }
            return text;
        }
        #endregion Sorted Dictionary
    }
}