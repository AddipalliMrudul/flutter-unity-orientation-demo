using System;
using XcelerateGames.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [ExecuteInEditMode]
    public class UiProgressBarBase : UiItem
    {
        [Range(0, 1)] public float _Progress = 0.5f;

        protected float mStartProgress = 0f;
        protected float mEndProgress = 1f;
        protected float mTime = 5f;
        protected float mElapsedTime = 0f;

        public Action<UiProgressBarBase> OnAnimComplete;
        public Action<UiProgressBarBase> OnFull;
        public Action<UiProgressBarBase> OnEmpty;

        public AudioVars _ProgressStartSFX, _OnFullSFX, _OnEmptySFX;

        protected override void Awake()
        {
            base.Awake();
            enabled = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mElapsedTime = 0f;
        }

        /// <summary>
        /// Set the progress of progress bar. The value should be between 0 & 1.
        /// </summary>
        /// <param name="inProgress"></param>
        public virtual void SetProgress(float inProgress, float time)
        {
            mStartProgress = _Progress;
            mEndProgress = Mathf.Clamp(inProgress, 0, 1);
            mTime = time;
            mElapsedTime = 0f;
            enabled = true;
            if (_ProgressStartSFX != null)
                _ProgressStartSFX.Play();
        }

        public virtual void SetProgress(float startProgress, float endProgress, float time)
        {
            mStartProgress = Mathf.Clamp(startProgress, 0, 1);
            mEndProgress = Mathf.Clamp(endProgress, 0, 1);
            mTime = time;
            mElapsedTime = 0f;
            enabled = true;
            if (_ProgressStartSFX != null)
                _ProgressStartSFX.Play();
        }

        public virtual void SetProgress(float inProgress)
        {
            _Progress = Mathf.Clamp(inProgress, 0, 1);
        }

        public void SetProgressAndtext(float min, float max)
        {
            SetProgress(min / max);
            SetText($"{min}/{max}");
        }

        /// <summary>
        /// This is called in IDE only when a value in Inspector changes.
        /// </summary>
#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            SetProgress(_Progress);
        }

#endif

        protected override void Update()
        {
            base.Update();
            mElapsedTime += Time.deltaTime;
            if (mElapsedTime <= mTime)
                SetProgress(Mathf.Lerp(mStartProgress, mEndProgress, mElapsedTime / mTime));
            else
            {
                _Progress = mEndProgress;
                enabled = false;
                OnAnimComplete?.Invoke(this);
                if (mEndProgress >= mStartProgress)
                {
                    if (_Progress >= 0.98f)
                    {
                        if (OnFull != null)
                            OnFull.Invoke(this);
                        if (_OnFullSFX != null)
                            _OnFullSFX.Play();
                    }
                }
                else if(_Progress <= 0f)
                {
                    if (OnEmpty != null)
                        OnEmpty.Invoke(this);
                    if (_OnEmptySFX != null)
                        _OnEmptySFX.Play();
                }
            }
        }
    }
}