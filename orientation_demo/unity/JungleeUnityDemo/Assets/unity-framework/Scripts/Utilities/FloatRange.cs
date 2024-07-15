using System;

namespace XcelerateGames
{
    /// <summary>
    /// Float Range to deal with a min & max range of values
    /// </summary>
    [Serializable]
    public class FloatRange
    {
        public float _Min;

        public float _Max;

        public FloatRange(float min, float max)
        {
            _Min = min;
            _Max = max;
        }

        /// <summary>
        /// Get Random value in the specified range
        /// </summary>
        /// <returns>random value/returns>
        public float GetRandomValue() { return UnityEngine.Random.Range(_Min, _Max); }

        /// <summary>
        /// Checks is the given value is in range
        /// </summary>
        /// <param name="inValue">Value to check</param>
        /// <returns>true if within range, else false</returns>
        public bool IsInRange(float inValue)
        {
            if (inValue < _Min || inValue > _Max)
                return false;
            return true;
        }

        /// <summary>
        /// Return range values in string format. Used for debugging purposes only
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Min : {_Min}, Max : {_Max}";
        }
    }
}
