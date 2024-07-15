using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiDecimalCounter : UiCounterBase<decimal>
    {
        [SerializeField] int _NoOfDecimalPlaces = 0;
        private string mPattern = "0";
        public bool pIsNonDecimalValue { get; private set; }

        protected override decimal GetValue()
        {
            return mStart + ((mEnd - mStart) * (decimal)Progress);
        }

        public override void Init(decimal? start, decimal end, float duration, System.Action<UiCounterBase<decimal>> progress = null, System.Action<UiCounterBase<decimal>> completion = null)
        {
            if (start.HasValue)
                mStart = start.Value;
            else
            {
                if (!decimal.TryParse(_TextItem.text, out mStart))
                {
                    SetText(end);
                    XDebug.LogError($"No start value present for DecimalCounter");
                    return;
                }
            }
            mEnd = end;
            SetPattern();
            base.Init(start, end, duration, progress, completion);
        }

        public void SetNoOfDecimalPlace(int decimalPlaces)
        {
            _NoOfDecimalPlaces = decimalPlaces;
        }

        private void SetPattern()
        {
            var splittedArray = mStart.ToString().Split('.');
            if (splittedArray.Length > 1 && _NoOfDecimalPlaces > 0)
            {
                mPattern = "0.";
                for (int i = 0; i < _NoOfDecimalPlaces; ++i)
                    mPattern += "#";
                pIsNonDecimalValue = false;
            }
            else
            {
                mPattern = "0";
                pIsNonDecimalValue = true;
            }
        }

        public override void SetText(decimal value)
        {
            _TextItem.text = value.ToString(mPattern);
        }
    }
}