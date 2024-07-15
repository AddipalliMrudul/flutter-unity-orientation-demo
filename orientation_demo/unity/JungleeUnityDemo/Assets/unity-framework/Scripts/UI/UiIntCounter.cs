using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiIntCounter : UiCounterBase<int> 
    {
        protected override int GetValue()
        {
            return Mathf.Min((mStart + (int)((mEnd - mStart) * Progress)), mEnd);
        }

        public override void Init(int? start, int end, float duration, System.Action<UiCounterBase<int>> progress = null, System.Action<UiCounterBase<int>> completion = null)
        {
            if (start.HasValue)
                mStart = start.Value;
            else
                int.TryParse(_TextItem.text, out mStart);
            mEnd = end;

            base.Init(start, end, duration, progress, completion);
        }
    }
}