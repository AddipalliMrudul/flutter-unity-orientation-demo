using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for List class
    /// </summary>
    public static class ListExtensionMethods
    {
        #region List
        /// <summary>
        /// Get the first element in the list. It does not check for null or size.
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <returns>First element in the list</returns>
        public static T First<T>(this List<T> list)
        {
            return list[0];
        }

        /// <summary>
        /// Get the last element in the list. It does not check for null or size.
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <returns>First element in the list</returns>
        public static T Last<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        /// <summary>
        /// Get elements specified by count from the list starting from the given index
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns>List of elements</returns>
        public static List<T> GetItems<T>(this List<T> list, int index, int count)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < count; ++i)
            {
                newList.Add(list[index + i]);
            }
            return newList;
        }

        /// <summary>
        /// Get a random element from the list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <returns></returns>
        /// @exception Throws InvalidDataException  if list is null or empty
        public static T Random<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidDataException();
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Get random items in the list specified by the count
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <param name="count">item count</param>
        /// <returns>List of random items</returns>
        public static List<T> GetRandomItems<T>(this List<T> list, int count)
        {
            List<T> newList = new List<T>();
            List<int> randomSequene = RandomSequence.GetList(0, list.Count, count);
            for (int i = 0; i < randomSequene.Count; ++i)
            {
                newList.Add(list[randomSequene[i]]);
            }
            return newList;
        }

        /// <summary>
        /// Merge two lists in a new list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list1">list 1 to merge</param>
        /// <param name="list2">list 2 to merge</param>
        /// <returns>New list with items from list1 & list2</returns>
        public static List<T> Merge<T>(this List<T> list1, List<T> list2)
        {
            List<T> newList = new List<T>(list1);
            newList.AddRange(list2);
            return newList;
        }
        /// <summary>
        /// Adds an element to the list making sure only one element is present in the list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <param name="item">Element to add to list</param>
        public static void AddItemOnce<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        /// <summary>
        /// Checks if the given element is part of the list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <param name="item">Element to check</param>
        /// <returns>true if element is found, else false</returns>
        public static bool ContainsItem<T>(this List<T> list, T item)
        {
            if (list == null)
                return false;
            return list.Contains(item);
        }

        /// <summary>
        /// Checks if both the lists contains one common element
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list1">List of elements 1</param>
        /// <param name="list2">List of elements 2</param>
        /// <returns>true if atleast one element is common else false</returns>
        public static bool HasCommonElements<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null || list2 == null)
                return false;
            foreach(T item in list1)
            {
                if (list2.Contains(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the list is either null or empty
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <returns>true if null or empty, else false</returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null || !list.Any())
                return true;
            return false;
        }

        /// <summary>
        /// Return element count in the list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        /// <returns>no of items in the list</returns>
        public static int ItemCount<T>(this List<T> list)
        {
            if (list == null)
                return 0;
            return list.Count;
        }

        /// <summary>
        /// Compare one list to another element by element
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list1">list of elements</param>
        /// <param name="list2">list of elements</param>
        /// <returns>true if both the lists match completely else false</returns>
        public static bool CompareTo<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null && list2 != null)
                return false;
            if (list1 != null && list2 == null)
                return false;
            if (list1.Count != list2.Count)
                return false;
            for(int i = 0; i < list1.Count; ++i)
            {
                if (!list1[i].Equals(list2[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Shuffle the given list.
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List of elements</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null)
            {
                XDebug.LogWarning("Trying to shuffle an empty list");
                return;
            }
            int n = list.Count;
            System.Random rnd = new System.Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Clear all elements in the list
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List of elements</param>
        public static void ClearAll<T>(this List<T> list)
        {
            if (list != null)
                list.Clear();
        }

        /// <summary>
        /// Returns a string representation of all elements in the list. Useful while debugging to dump list items.
        /// In case of custom classes create a ToString function in the custom class to have better control on what gets printed
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">List to be printed</param>
        /// <param name="separator">Separator between each element. Default is next line</param>
        /// <returns></returns>
        public static string Printable<T>(this List<T> list, char separator = '\n', Func<T, string> converter = null)
        {
            string data = null;
            if(list != null)
            {
                foreach(T i in list)
                {
                    if (converter == null)
                        data += i.ToString() + separator;
                    else
                        data += converter.Invoke(i) + separator;
                }
            }
            return data;
        }

        /// <summary>
        /// Returns the next index in the list by ensuring we go to first element when we reach last index
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">>List to be processed</param>
        /// <param name="index">current index</param>
        /// <returns>next index</returns>
        public static int NextIndex<T>(this List<T> list, int index)
        {
            ++index;
            if (index >= list.Count)
                index = 0;
            return index;
        }

        /// <summary>
        /// Returns the previous index in the list by ensuring we go to last element when we reach first index
        /// </summary>
        /// <typeparam name="T">type of elements in the list</typeparam>
        /// <param name="list">>List to be processed</param>
        /// <param name="index">current index</param>
        /// <returns>previous index</returns>
        public static int PreviousIndex<T>(this List<T> list, int index)
        {
            --index;
            if (index < 0)
                index = list.Count-1;
            return index;
        }
        #endregion List
    }
}