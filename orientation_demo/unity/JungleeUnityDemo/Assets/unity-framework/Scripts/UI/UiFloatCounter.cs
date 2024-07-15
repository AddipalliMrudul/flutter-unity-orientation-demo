using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiFloatCounter : UiCounterBase<float> 
    {
        [SerializeField] int _NoOfDecimalPlaces = 2;
        private string mPattern = "0";
        protected override float GetValue()
        {
            return Mathf.Min((mStart + ((mEnd - mStart) * Progress)), mEnd);
        }

        public override void Init(float? start, float end, float duration, System.Action<UiCounterBase<float>> progress = null, System.Action<UiCounterBase<float>> completion = null)
        {
            SetPattern();
            if (start.HasValue)
                mStart = start.Value;
            else
                float.TryParse(_TextItem.text, out mStart);
            mEnd = end;

            base.Init(start, end, duration, progress, completion);
        }

        public void SetNoOfDecimalPlace(int value)
        {
            _NoOfDecimalPlaces = value;
        }

        private void SetPattern()
        {
            mPattern = "0";
            if (_NoOfDecimalPlaces > 0) mPattern = "0.";
            for (int i = 0; i < _NoOfDecimalPlaces; ++i)
                mPattern += "#";
        }

        public override void SetText(float value)
        {
            _TextItem.text = value.FormatRegardingPattern(mPattern);
        }

        public override void SetText(string v)
        {
            _TextItem.text = Value.FormatRegardingPattern(mPattern);
        }
    }
}