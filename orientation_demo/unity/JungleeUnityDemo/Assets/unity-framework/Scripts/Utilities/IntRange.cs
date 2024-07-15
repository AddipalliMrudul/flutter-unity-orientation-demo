using System;

namespace XcelerateGames
{

    [Serializable]
    public class IntRange
    {
        public int _Min;

        public int _Max;

        public int GetRandomValue() { return UnityEngine.Random.Range(_Min, _Max); }

        public bool IsInRange(float inValue)
        {
            if (inValue < _Min || inValue > _Max)
                return false;
            return true;
        }

        public override string ToString()
        {
            return ("Min : " + _Min + ", Max : " + _Max);
        }

        public IntRange() { }
        public IntRange(int minVal, int maxVal) { _Min = minVal; _Max = maxVal; }
    }
}
