using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for Arrays.
    /// @see ListExtensionMethods
    /// </summary>
    public static class ArrayExtensionMethods
    {
        #region List
        /// <summary>
        /// Returns the first element in the array
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <returns>First element in array</returns>
        public static T First<T>(this T[] array)
        {
            return array[0];
        }

        /// <summary>
        /// Returns the last element in the array
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <returns>Last element in array</returns>
        public static T Last<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

        /// <summary>
        /// Get elements specified by count from the array starting from the given index
        /// </summary>
        /// <typeparam name="T">type of elements in the array</typeparam>
        /// <param name="list">array of elements</param>
        /// <param name="index">start index</param>
        /// <param name="count">no of items to return</param>
        /// <returns>List of elements</returns>
        public static List<T> GetItems<T>(this T[] array, int index, int count)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < count; ++i)
            {
                newList.Add(array[index + i]);
            }
            return newList;
        }

        /// <summary>
        /// Get random items in the list specified by the count
        /// </summary>
        /// <typeparam name="T">type of elements in the array</typeparam>
        /// <param name="list">Array of elements</param>
        /// <param name="count">item count</param>
        /// <returns>List of random items</returns>
        public static List<T> GetRandomItems<T>(this T[] array, int count)
        {
            List<T> newList = new List<T>();
            List<int> randomSequene = RandomSequence.GetList(0, array.Length, count);
            for (int i = 0; i < randomSequene.Count; ++i)
            {
                newList.Add(array[randomSequene[i]]);
            }
            return newList;
        }

        /// <summary>
        /// Returns a random element from the array
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <returns>Random element within the array</returns>
        public static T Random<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new InvalidDataException();
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Returns true if the list is either null or empty
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <returns>true if the list is either null or empty else false</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            if (array == null || !array.Any())
                return true;
            return false;
        }

        /// <summary>
        /// Returns no of elements in array. if null returns zero
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <returns>Element count</returns>
        public static int ItemCount<T>(this T[] array)
        {
            if (array == null)
                return 0;
            return array.Length;
        }

        /// <summary>
        /// Compares two arrays. Compares array by null, length & each element
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array1">Array of type T</param>
        /// <param name="array2">Array of type T</param>
        /// <returns>true if each element in one array matches the other one</returns>
        public static bool CompareTo<T>(this T[] array1, T[] array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null && array2 != null)
                return false;
            if (array1 != null && array2 == null)
                return false;
            if (array1.Length != array2.Length)
                return false;
            for(int i = 0; i < array1.Length; ++i)
            {
                if (!array1[i].Equals(array2[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Shuffle the given list.
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        public static void Shuffle<T>(this T[] array)
        {
            if (array == null)
            {
                XDebug.LogWarning("Trying to shuffle an empty array");
                return;
            }
            int n = array.Length;
            System.Random rnd = new System.Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }

        /// <summary>
        /// Checks if the given element is part of the array
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Array of type T</param>
        /// <param name="element">Element to search</param>
        /// <returns>true if found, else false</returns>
        public static bool Contains<T>(this T[] array, T element)
        {
            if(element == null || array == null)
                return false;

            for (int i = array.Length - 1; i != -1; --i)
            {
                if (element.Equals(array[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a string representation of all elements in the array. Useful while debugging to dump array items.
        /// In case of custom classes create a ToString function in the custom class to have better control on what gets printed
        /// </summary>
        /// <typeparam name="T">type of elements in the array</typeparam>
        /// <param name="array">Array to be printed</param>
        /// <param name="separator">Separator between each element. Default is next line</param>
        /// <returns></returns>
        public static string Printable<T>(this T[] array, char separator = '\n')
        {
            string data = null;
            if (array != null)
            {
                foreach (T i in array)
                {
                    data += i.ToString() + separator;
                }
            }
            return data;
        }
        #endregion List
    }
}