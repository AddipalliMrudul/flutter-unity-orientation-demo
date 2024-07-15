using System;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Timer
{
    internal class TimeUpdater : BaseBehaviour
    {
        //This is because, in Rummy we have time in seconds & it is set by socket connection
        [SerializeField] bool _SyncMilliSecondsToSeconds = false;

        [InjectSignal] private SigAppResumedFromBackground mSigAppResumedFromBackground = null;

        public static TimeUpdater Instance { get; private set; }
        private double mElapsedTime = 0;
        private double mElapsedTimeMilliSeconds = 0;
        private DateTime mMinimisedTime = DateTime.Now;
        private bool? mAppPaused = null;

        public double pElapsedTime
        {
            get { return mElapsedTime; }
        }

        public double pElapsedTimeMilliSeconds
        {
            get { return mElapsedTimeMilliSeconds; }
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new GameObject("TimeUpdater").AddComponent<TimeUpdater>();
                GameObject.DontDestroyOnLoad(Instance.gameObject);
            }
        }

        public void ResetSec()
        {
            mElapsedTime = 0;
            if (_SyncMilliSecondsToSeconds)
                ResetMS();
        }

        public void ResetMS()
        {
            mElapsedTimeMilliSeconds = 0;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                mMinimisedTime = DateTime.Now;
                mAppPaused = true;
            }
            else
            {
                if (mAppPaused.HasValue)
                {
                    TimeSpan timeSpan = DateTime.Now - mMinimisedTime;
                    XDebug.Log($"Was in BG for {timeSpan.TotalMilliseconds} milli sec");
                    if (timeSpan.TotalMilliseconds > 0)
                    {
                        mElapsedTime += timeSpan.TotalSeconds;
                        mElapsedTimeMilliSeconds += timeSpan.Milliseconds;
                        mSigAppResumedFromBackground.Dispatch(timeSpan.TotalMilliseconds);
                    }
                    else
                        XDebug.LogWarning($"Cant go back in time, Time machine is`nt invented yet {timeSpan.TotalSeconds}");
                }
            }
        }

        private void Update()
        {
            mElapsedTime += Time.deltaTime;
            mElapsedTimeMilliSeconds += Time.deltaTime * 1000f;
        }
    }
}
