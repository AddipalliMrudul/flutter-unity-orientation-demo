using XcelerateGames.Timer;
using UnityEngine;
using XcelerateGames.Audio;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

namespace XcelerateGames.UI
{
    public class UiTimer : MonoBehaviour
    {
        public enum TimeFormat
        {
            None,
            Short,
            ShortNoAbbreviations,
            Standard,
            SmartTimer,
            ShortNoAbbreviations2,
            ShortWithTwoFields
        }

        public enum TimerUsage
        {
            LocalTime,
            ServerTime,
            CachedServerTime
        }

        public UiItem _TextItem = null;
        public TimeFormat _TimeFormat = TimeFormat.None;
        public Color _DefaultColor = Color.white;
        public Color _WarningColor = Color.red;
        public int _WarningTime = int.MaxValue;
        public AudioVars _WarningSFX = null;

        private System.Action<UiTimer> OnComplete, OnPause;
        private System.Action<UiTimer, double> OnTimerTick;

        private double mExpiryTime;
        private float mElapsedTime = 0;
        private bool mShowingWarningTime = false;
        private TimerUsage mTimerUsage;
        private double mCachedServerTime;

        private DateTime mMinimisedTime = DateTime.Now;
        private bool? mAppPaused = null;

        public bool IsPaused => !enabled;
        public bool IsTimeExpired => GetRemainingTime() <= 0;

        #region Public Methods

        /// <summary>
        /// Start Timer
        /// @deprecated this method is deprecated, Use \link StartTimer(long, TimerUsage) this \endlink method instead. 
        /// </summary>
        /// <param name="expiryTime">Total no of seconds</param>
        ///@see StartTimer(long, TimerUsage)
        //[System.Obsolete("Use Overloaded method instead")]
        public void StartTimer(long expiryTime, bool useServerTime, System.Action<UiTimer> complete = null, System.Action<UiTimer, double> tick = null, System.Action<UiTimer> pause = null)
        {
            StartTimer(expiryTime, useServerTime ? TimerUsage.ServerTime : TimerUsage.LocalTime, complete, tick, pause);
        }

        /// <summary>
        /// Start Timer
        /// </summary>
        /// <param name="expiryTime">Total no of seconds</param>
        public void StartTimer(long expiryTime, TimerUsage timerusage, System.Action<UiTimer> complete = null, System.Action<UiTimer, double> tick = null, System.Action<UiTimer> pause = null)
        {
            gameObject.SetActive(true);
            if (_TextItem != null)
            {
                _TextItem.SetActive(true);
                _TextItem._TextItem.color = _DefaultColor;
            }
            else
                XDebug.LogWarning($"Text Component is missing in UiTimer component. Object:{name}", XDebug.Mask.Game);
            OnComplete = complete;
            OnTimerTick = tick;
            OnPause = pause;

            mElapsedTime = 0;
            mTimerUsage = timerusage;
            mExpiryTime = expiryTime;
            mCachedServerTime = ServerTime.pCurrentTime;
            if (timerusage == TimerUsage.ServerTime || timerusage == TimerUsage.CachedServerTime)
                mExpiryTime += mCachedServerTime;
            mShowingWarningTime = false;
            enabled = true;
            ScheduleTick();
        }


        public void PauseTimer(bool pause)
        {
            enabled = !pause;
            CancelInvoke();
            if (pause)
            {
                if (mTimerUsage == TimerUsage.ServerTime || mTimerUsage == TimerUsage.CachedServerTime)
                    InvokeRepeating(nameof(UpdateExpiryTime), 1, 1);
                OnPause?.Invoke(this);
            }
            else
                ScheduleTick();
        }

        public void StopTimer()
        {
            enabled = false;
            _WarningSFX.Stop();
            CancelInvoke();
        }

        public double GetRemainingTime()
        {
            switch (mTimerUsage)
            {
                case TimerUsage.ServerTime:
                    return mExpiryTime - ServerTime.pCurrentTime;
                case TimerUsage.CachedServerTime:
                    return mExpiryTime - mCachedServerTime;
                //case TimerUsage.LocalTime:
                default:
                    return mExpiryTime - (long)mElapsedTime;
            }
        }

        public void SetText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                _TextItem?.SetText(text);
        }

        public void SetColor(bool isWarning)
        {
            if (_TextItem != null)
                _TextItem._TextItem.color = isWarning ? _WarningColor : _DefaultColor;
        }

        public void ClearOnCompleteCallback()
        {
            OnComplete = null;
        }

        public void ClearOnTickCallback()
        {
            OnTimerTick = null;
        }
        #endregion

        #region Private Methods

        private void Start() { }

        private void ScheduleTick()
        {
            CancelInvoke();
            switch (mTimerUsage)
            {
                case TimerUsage.LocalTime:
                    InvokeRepeating(nameof(UpdateLocalTimer), 0, 1);
                    break;
                case TimerUsage.ServerTime:
                    InvokeRepeating(nameof(UpdateServerTimer), 0, 1);
                    break;
                case TimerUsage.CachedServerTime:
                    InvokeRepeating(nameof(UpdateCachedServerTimer), 0, 1);
                    break;
            }
        }

        private void UpdateLocalTimer()
        {
            if (!IsPaused)
                mElapsedTime++;
            TimerTick(mExpiryTime - mElapsedTime);
        }

        private void UpdateServerTimer()
        {
            TimerTick(mExpiryTime - ServerTime.pCurrentTime);
        }

        private void UpdateCachedServerTimer()
        {
            TimerTick(mExpiryTime - mCachedServerTime);
            mCachedServerTime++;
        }

        private void TimerTick(double timeRemaining)
        {
            UpdateUi(timeRemaining);
            OnTimerTick?.Invoke(this, timeRemaining); 
        }

        private void UpdateExpiryTime() => mExpiryTime++;

        protected virtual void UpdateUi(double timeRemaining)
        {
            if (timeRemaining >= 0)
            {
                SetTime(timeRemaining);
                if (!mShowingWarningTime && timeRemaining <= _WarningTime)
                    ShowWarning();
                if (timeRemaining == 0)
                    TimeExpired();
            }
            else
                TimeExpired();
        }

        protected virtual void ShowWarning()
        {
            mShowingWarningTime = true;
            if (_TextItem != null)
                _TextItem._TextItem.color = _WarningColor;
            _WarningSFX.Play();
        }

        private void TimeExpired()
        {
            StopTimer();
            OnComplete?.Invoke(this);
            OnComplete = null;
            OnPause = null;
            OnTimerTick = null;
        }

        protected void SetTime(double timeRemaining)
        {
            float roundedValue = Mathf.RoundToInt((float)timeRemaining);
            string time = string.Empty;
            switch (_TimeFormat)
            {
                case TimeFormat.Short:
                    time = TimeUtilities.GetTimerStringShort(roundedValue);
                    break;
                case TimeFormat.ShortNoAbbreviations:
                    time = TimeUtilities.GetTimerStringShortNoAbbreviations(roundedValue);
                    break;
                case TimeFormat.Standard:
                    time = TimeUtilities.GetStandardTimerString(roundedValue);
                    break;
                case TimeFormat.SmartTimer:
                    time = TimeUtilities.GetTimerString(roundedValue);
                    break;
                case TimeFormat.ShortNoAbbreviations2:
                    time = TimeUtilities.GetTimerStringShortNoAbbreviationsWithHour(roundedValue);
                    break;
                case TimeFormat.ShortWithTwoFields:
                    time = TimeUtilities.GetTimerStringShortWithTwoFields(roundedValue);
                    break;

            }
            _TextItem?.SetText(time);
        }

        public void SetTime()
        {
            UpdateUi(GetRemainingTime());
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void OnApplicationPause(bool pause)
        {
            if (mTimerUsage != TimerUsage.LocalTime && mTimerUsage != TimerUsage.CachedServerTime) return;
            if (pause)
            {
                mMinimisedTime = DateTime.Now;
                mAppPaused = true;
            }
            else if (mAppPaused.HasValue)
            {
                TimeSpan timeSpan = DateTime.Now - mMinimisedTime;
                if (timeSpan.TotalSeconds > 0)
                {
                    if (mTimerUsage == TimerUsage.LocalTime)
                        mElapsedTime += (float)timeSpan.TotalSeconds;
                    else if (mTimerUsage == TimerUsage.CachedServerTime)
                        mCachedServerTime += timeSpan.TotalSeconds;
                }
                mAppPaused = null;
            }
        }
        #endregion
    }
}