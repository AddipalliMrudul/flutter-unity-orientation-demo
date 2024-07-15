using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [ExecuteInEditMode]
    public class UiProgressBar : UiProgressBarBase
    {
        public Image _Bar = null;
        protected float mStartTime = 0f;


        public override void SetProgress(float inProgress)
        {
            base.SetProgress(inProgress);
            if (_Bar != null)
                _Bar.fillAmount = _Progress;
        }

#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            if (_Bar != null)
                XDebug.Assert(_Bar.type == Image.Type.Filled, "Invalid Image Type, It must be set to Filled. " + gameObject.GetObjectPath());
        }
#endif
        public override void SetProgress(float startProgress, float endProgress, float time)
        {
            base.SetProgress(startProgress, endProgress, time);
            mStartTime = Time.realtimeSinceStartup;
            mElapsedTime = Time.realtimeSinceStartup - mStartTime;
        }

        public float GetRemainingSeconds()
        {
            return mTime - mElapsedTime;
        }

        public float GetElapsedSeconds()
        {
            return mElapsedTime;
        }

        public void AddElapsedTime(float addTime)
        {
            mStartTime += addTime;
        }

        protected override void Update()
        {
            mElapsedTime = Time.realtimeSinceStartup - mStartTime;
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
                else if (_Progress <= 0f)
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