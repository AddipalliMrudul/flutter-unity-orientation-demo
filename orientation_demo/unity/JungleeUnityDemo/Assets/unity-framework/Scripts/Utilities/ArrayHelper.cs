using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Helper class to handle adding & removing array elements
    /// </summary>
    static public class ArrayHelper
    {
        /// <summary>
        /// Add a new element to the array by calling default constructor & returns the newly added element
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">array to which we are ading the element</param>
        /// <returns>Newly added element</returns>
        static public T AddArrayElement<T>(ref T[] array) where T : new()
        {
            return AddArrayElement<T>(ref array, new T());
        }

        /// <summary>
        /// Adds an element to the array & returns the newly added element
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">array to which we are adding the element</param>
        /// <param name="elToAdd">element to add</param>
        /// <returns>Newly added element</returns>
        static public T AddArrayElement<T>(ref T[] array, T elToAdd)
        {
            if (array == null)
            {
                array = new T[1];
                array[0] = elToAdd;
                return elToAdd;
            }
            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = elToAdd;
            array = newArray;
            return elToAdd;
        }

        /// <summary>
        /// Delete the element from the array at the given index
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">array to which we are removing the element</param>
        /// <param name="index">index at which the element is to be removed</param>
        /// <returns><true if element was removed else false/returns>
        static public bool DeleteArrayElement<T>(ref T[] array, int index)
        {
            if (index >= array.Length || index < 0)
            {
                Debug.LogWarning("invalid index in DeleteArrayElement: " + index);
                return false;
            }
            var newArray = new T[array.Length - 1];
            int i;
            for (i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }
            for (i = index + 1; i < array.Length; i++)
            {
                newArray[i - 1] = array[i];
            }
            array = newArray;
            return true;
        }
    }
}
