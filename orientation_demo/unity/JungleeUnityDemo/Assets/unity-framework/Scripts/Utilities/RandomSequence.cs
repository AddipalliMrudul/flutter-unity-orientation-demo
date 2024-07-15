using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames
{
    public class RandomSequence
    {
        /// <summary>
        ///Returns a random sequence from min to max-1 (excluding max)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        static public List<int> GetList(int min, int max)
        {
            if (min >= max)
            {
                UnityEngine.Debug.LogError("min has to be less than max");
                return null;
            }
            int size = max - min;
            List<int> sequence = new List<int>(size);
            for (int i = min; i < max; ++i)
                sequence.Add(i);
            sequence.Shuffle();

            return sequence;
        }

        /// <summary>
        /// Returns a random sequence for the given count from min to max-1 (excluding max) & excluding anything from excludeList
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="count"></param>
        /// <param name="excludeList"></param>
        /// <returns></returns>
        static public List<int> GetList(int min, int max, int count, List<int> excludeList)
        {
            if (min >= max)
            {
                UnityEngine.Debug.LogError("min has to be less than max");
                return null;
            }
            List<int> sequence = new List<int>(count);
            while (sequence.Count != count)
            {
                int num = Random.Range(min, max);
                if (!sequence.Contains(num))
                {
                    if (excludeList == null || !excludeList.Contains(num))
                        sequence.Add(num);
                }
            }

            return sequence;
        }

        /// <summary>
        /// Gets the list of random ints  with the given count, making sure none of the numbers are repeated.
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        /// <param name="count">Count.</param>
        static public List<int> GetList(int min, int max, int count)
        {
            int num = min;
            if (min >= max)
            {
                UnityEngine.Debug.LogError("min has to be less than max");
                return null;
            }
            List<int> randNumbers = new List<int>();
            //Just a higher number to avoid infinite loop :)
            int maxCount = 1000; 
            while (randNumbers.Count < count && maxCount >= 0)
            {
                maxCount--;
                num = Random.Range(min, max);
                if (randNumbers.Contains(num))
                    continue;
                else
                    randNumbers.Add(num);
            }

            return randNumbers;
        }

        /// <summary>
        /// Returns a random number in the range specified min, max exluding numbers in excludeList.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        /// <param name="excludeList">Exclude list.</param>
        static public int Get(int min, int max, List<int> excludeList)
        {
            int num = min;
            if (min >= max)
            {
                Debug.LogError("min has to be less than max");
                return min;
            }
            while (true)
            {
                num = Random.Range(min, max);
                if (excludeList == null || !excludeList.Contains(num))
                    break;
            }

            return num;
        }
		
        static public int Get(int min, int max, int excluding)
        {
            int num = min;
            if (min >= max)
            {
                UnityEngine.Debug.LogError("min has to be less than max");
                return min;
            }
            while (true)
            {
                num = Random.Range(min, max);
                if (num != excluding)
                    break;
            }

            return num;
        }
    }
}