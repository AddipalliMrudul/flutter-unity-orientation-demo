using TMPro;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiCounterBase<T> : MonoBehaviour where T : struct 
    {
        [SerializeField] protected TextMeshProUGUI _TextItem = null;

        public T Value;
        public float Progress { get; protected set; }

        protected float mDuration { get; set; }
        protected float mElapsedTime = 0f;
        protected T mStart, mEnd;

        protected System.Action<UiCounterBase<T>> ProgressCallback, CompleteCallback;

        private void Awake()
        {
            enabled = false;
        }

        protected virtual T GetValue()
        {
            return default(T);
        }

        protected virtual void Update()
        {
            mElapsedTime += Time.deltaTime;
            Progress = Mathf.Clamp01(mElapsedTime / mDuration);
            Value = GetValue();
            SetText(Value);
            if (Progress >= 1)
            {
                Stop();
            }
            else
            {
                ProgressCallback?.Invoke(this);
            }
        }

        public virtual void Init(T? start, T end, float duration, System.Action<UiCounterBase<T>> progress = null, System.Action<UiCounterBase<T>> completion = null) 
        {
            mDuration = duration;
            ProgressCallback = progress;
            CompleteCallback = completion;
            enabled = true;
            mElapsedTime = 0;
        }

        public virtual void Stop()
        {
            Value = mEnd;
            CompleteCallback?.Invoke(this);
            enabled = false;
        }

        public virtual void Reset()
        {
            SetText(string.Empty);
            enabled = false;
        }

        public virtual void SetText(string v)
        {
            _TextItem.text = v;
        }

        public virtual void SetText(T value)
        {
            _TextItem.text = value.ToString();
        }

    }
}