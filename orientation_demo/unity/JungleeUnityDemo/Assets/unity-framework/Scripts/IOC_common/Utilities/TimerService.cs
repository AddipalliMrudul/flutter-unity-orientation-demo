using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Timer
{
    public class SigTimerRegister : Signal<Action<float>, float, bool, int> // Handler, Interval, is Interval in sec(true) or frames(false), Repeat
    {
        public void Dispatch(Action<float> handler)
        {
            Dispatch(handler, 1f, true, 0);
        }

        public void Dispatch(Action<float> handler, float timeInSec)
        {
            Dispatch(handler, timeInSec, true, 0);
        }

        public void Dispatch(Action<float> handler, float timeInSec, int repeat)
        {
            Dispatch(handler, timeInSec, true, repeat);
        }

        //public new void Dispatch(Action<float> handler, float interval, bool isSec)
        //{
        //    Dispatch(handler, interval, isSec, 0);
        //}
    }
    public class SigTimerUnregister : Signal<Action<float>> { }
    public class SigTimerUnregisterAll : Signal { }

    /// <summary>
    /// Class TimerService
    /// </summary>
    public class TimerService : BaseBehaviour
    {
        private class Data
        {
            public int Repeat { get; internal set; }
            public float Interval { get; internal set; }
            public Action<float> Handler { get; internal set; }
            public float Elapsed { get; internal set; }
            public bool IsSec { get; internal set; }
        }

        private List<Data> mListeners;

        [InjectSignal] private SigTimerRegister mSigRegister = null;
        [InjectSignal] private SigTimerUnregister mSigUnregister = null;
        [InjectSignal] private SigTimerUnregisterAll mSigUnregisterAll = null;

        private static TimerService mInstance = null;

        protected override void Awake()
        {
            if (mInstance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                mInstance = this;
                base.Awake();
            }
        }

        private void Start()
        {
            mListeners = new List<Data>();
            mSigRegister.AddListener(OnRegister);
            mSigUnregister.AddListener(OnUnregister);
            mSigUnregisterAll.AddListener(OnUnRegisterAll);
            DontDestroyOnLoad(gameObject);
            enabled = false;
        }

        private void Update()
        {
            if (mListeners.Count == 0)
            {
                return;
            }
            List<Data> deadHandlers = new List<Data>();
            mListeners.ForEach(x => x.Elapsed += x.IsSec ? Time.deltaTime : 1.0000001f);
            mListeners.FindAll(x => x.Elapsed > x.Interval).ForEach(d =>
            {
                if (d.Handler.GetInvocationList()[0].Target != null && d.Handler.GetInvocationList()[0].Target.ToString() != "null")
                {
                    d.Handler(d.Elapsed);
                }
                else
                {
                    deadHandlers.Add(d);
                }
                d.Elapsed -= d.Interval;
                if (d.Repeat > 0)
                {
                    --d.Repeat;
                }
                if (d.Repeat == 0)
                {
                    deadHandlers.Add(d);
                }
            });

            deadHandlers.ForEach(x => OnUnregister(x));
        }

        protected override void OnDestroy()
        {
            if (mInstance == this)
            {
                base.OnDestroy();
                mSigRegister.RemoveListener(OnRegister);
                mSigUnregister.RemoveListener(OnUnregister);
                mSigUnregisterAll.RemoveListener(OnUnRegisterAll);
            }
        }

        private void OnRegister(Action<float> inHandler, float inInterval, bool inIsSec, int inRepeat)
        {
            Data d1 = new Data
            {
                Repeat = inRepeat,
                Interval = inInterval,
                Handler = inHandler,
                Elapsed = 0.0f,
                IsSec = inIsSec
            };
            mListeners.Add(d1);
            enabled = true;
        }

        private void OnUnregister(Action<float> inHandler)
        {
            mListeners.RemoveAll(x => x.Handler == inHandler);
            enabled = mListeners.Count > 0;
        }

        private void OnUnregister(Data inHandlerData)
        {
            mListeners.Remove(inHandlerData);
            enabled = mListeners.Count > 0;
        }

        private void OnUnRegisterAll()
        {
            mListeners.Clear();
            enabled = false;
        }
    }
}
