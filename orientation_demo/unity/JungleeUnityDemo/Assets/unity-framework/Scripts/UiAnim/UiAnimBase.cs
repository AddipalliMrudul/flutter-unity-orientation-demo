using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XcelerateGames.Audio;

namespace XcelerateGames.UI.Animations
{
    [System.Serializable]
    public class CGUiAnimEvent : UnityEvent<UiAnim, string>
    {
        public bool HasEventName(string eventName)
        {
            for (int i = 0; i < GetPersistentEventCount(); i++)
            {
                if (eventName == GetPersistentMethodName(i))
                    return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public enum Mode
    {
        STOP,
        FORWARD_ONESHOT, // Animation will be played only once. 0 -> n
        FORWARD_INFINITE, // Animation will be played infinitely. 0 -> n-1 frame & again 0 -> n-1 frames

        REVERSE_ONESHOT, // Animation will be played only once. n -> 0
        REVERSE_INFINITE, // Animation will be played infinitely. n -> 0, & again 0 -> n

        PINGPONG, // Animation will be played infinitely. 0 -> n-1 frame & again n-1 -> 0 frames
    };

    [System.Serializable]
    public class UiAnimBase
    {
        public enum Category
        {
            General,
            In,
            Out,
            OnClick,
        }

        public string _Name = "";
        public Mode _Mode = Mode.FORWARD_ONESHOT;
        public int _LoopCount = -1;

        public bool _IgnoreTimeScale = false;

        public bool _SnapToStart = false;
        public bool _StartInActive = false;
        public bool _Reference = false;

        public Category _Category = Category.General;

        public float _Delay = 0f;
        public float _TimeMultiplier = 1.0f;

        public PositionAnimData _PositionData = null;
        public AnimData _RotationData = null;
        public AnimData _ScaleData = null;
        public ColorData _ColorData = null;

        public bool _DestroyOnComplete = false;

        //As per design requirements, we do not send evenst when In & Out anims are played, To override that enable this flag.
        public bool _TriggerEventOverride = false;

        //public string _AudioClip = null;
        //public AudioClip _AudioClipRef = null;
        //public string _AudioCategory = AudioController.CategorySFX;
        public AudioVars _AudioVars = null;

        //[Range(0, 1)]
        //public float _Volume = 1f;

        //public float _AudioDelay = 0f;

        public CGUiAnimEvent _OnAnimationStart = null;
        public CGUiAnimEvent _OnAnimationDone = null;

        private Vector3 mDefaultPosition = Vector3.zero;
        protected float mLength;

        protected float mTime;

        protected int mLoopCount = 0;
        protected bool mLengthCalculated = false;
        protected bool mDone = false;
        protected bool mPlayInvoked = false;
        protected float mFirstKeyTime = 0f;

        protected float mDelay = 0f;
        protected Transform mTransform = null;
        private UiAnim mAnimObj = null;
        protected MaskableGraphic mWidget = null;
        protected CanvasGroup mCanvasGroup = null;
        protected TextMesh mTextMesh = null;
        protected List<Material> mMaterials = null;
        protected bool mTriggerEvent = true;
        protected Vector3 mStartScale = Vector3.one;

        public static bool pDebugAll = false;
        public bool pDone { get { return mDone; } }
        public bool _Debug = false;

#if UNITY_EDITOR
        [HideInInspector] public bool _Draw = false;
#endif

        public Mode pMode
        {
            get { return _Mode; }
            set
            {
                if (_Mode != value)
                {
                    mLengthCalculated = false;
                    _Mode = value;
                }
            }
        }

        public void Init(Transform inTransform, UiAnim animObj)
        {
            mTransform = inTransform;
            mAnimObj = animObj;
            Cache();
            if (_StartInActive)
            {
                animObj.gameObject.SetActive(false);
            }

            CalculateLength();
        }

        public void Stop()
        {
            if (mTime >= mLength && !mDone)
            {
                mDone = true;
                //First send the animation from the Anim3D class, If we call this later this will set mCurrentAnim to null & if the target of the anim is self, then it wont play the anim
                if (mAnimObj != null)
                    mAnimObj.OnAnimationDone(_Name);
                if (mTriggerEvent || _TriggerEventOverride)
                    SendEvent(false);
            }
        }

        public void Reset(bool toBeginning, bool triggerEvent)
        {
            mTriggerEvent = triggerEvent;
            if (_ScaleData != null && _ScaleData._Keys.Length > 0)
                mTransform.localScale = toBeginning ? _ScaleData._Keys[0] : _ScaleData._Keys[_ScaleData._Keys.Length - 1];

            if (_PositionData != null && _PositionData._Keys.Length > 0)
            {
                Vector3 pos = toBeginning ? _PositionData._Keys[0] : _PositionData._Keys[_PositionData._Keys.Length - 1];
                if (_PositionData._UsePositionAsOffset)
                    mTransform.localPosition = mDefaultPosition + pos;
                else
                    mTransform.localPosition = pos;
            }

            if (_RotationData != null && _RotationData._Keys.Length > 0)
                mTransform.localRotation = Quaternion.Euler(toBeginning ? _RotationData._Keys[0] : _RotationData._Keys[_RotationData._Keys.Length - 1]);

            if (_ColorData != null && _ColorData._Keys.Length > 0)
            {
                Color clr = toBeginning ? _ColorData._Keys[0] : _ColorData._Keys[_ColorData._Keys.Length - 1];
                if (mWidget != null)
                    mWidget.color = clr;
                if (mCanvasGroup != null)
                    mCanvasGroup.alpha = clr.a;
            }
        }

        public virtual void Play(bool triggerEvent)
        {
            if(_StartInActive)
            {
                mAnimObj.gameObject.SetActive(true);
            }
            if (pDebugAll || _Debug)
            {
                UnityEngine.Debug.LogError("Playing : " + _Name + " anim on " + mAnimObj.GetObjectPath());
                XDebug.Assert((pDebugAll || _Debug) && mAnimObj.gameObject.activeInHierarchy, "Play called for in-active object : " + mAnimObj.GetObjectPath());
            }
            mTriggerEvent = triggerEvent;
            if (pMode == Mode.REVERSE_INFINITE || pMode == Mode.REVERSE_ONESHOT)
                mTime = mLength;
            else
                mTime = 0f;

            mDone = false;
            mDelay = _Delay;
            mLoopCount = 0;
            if (_PositionData != null)
                _PositionData.Reset();
            if (_RotationData != null)
                _RotationData.Reset();
            if (_ScaleData != null)
                _ScaleData.Reset();
            if (_ColorData != null)
                _ColorData.Reset();

            mPlayInvoked = false;
            if (_PositionData._StartFromCurrentPos && _PositionData._Keys.Length > 0)
                _PositionData._Keys[0] = mTransform.localPosition;
            Update();

            if (mAnimObj != null)
            {
                _AudioVars.Play();
                //if (_AudioClipRef != null)
                //    AudioController.Play(_AudioClipRef, false, _AudioCategory, _Volume, 1f, _AudioDelay);
                //else if (!string.IsNullOrEmpty(_AudioClip))
                //    AudioController.Play(_AudioClip, false, _AudioCategory, _Volume, 1f, _AudioDelay);
            }
        }

        public virtual void CalculateLength()
        {
            if (mAnimObj == null)
                return;
            if (_ScaleData != null && _ScaleData._Keys.Length > 0)
            {
                mLength = Mathf.Max(mLength, _ScaleData._Duration + _ScaleData._Delay);
                if (_SnapToStart)
                    mTransform.localScale = _ScaleData._Keys[0];
            }

            if (_PositionData != null && _PositionData._Keys.Length > 0)
            {
                mLength = Mathf.Max(mLength, _PositionData._Duration + _PositionData._Delay);

                if (_SnapToStart)
                    mTransform.localPosition = _PositionData._Keys[0];
            }

            if (_RotationData != null && _RotationData._Keys.Length > 0)
            {
                mLength = Mathf.Max(mLength, _RotationData._Duration + _RotationData._Delay);
                if (_SnapToStart)
                    mTransform.localRotation = Quaternion.Euler(_RotationData._Keys[0]);
            }

            if (_ColorData != null && _ColorData._Keys.Length > 0)
            {
                mLength = Mathf.Max(mLength, _ColorData._Duration + _ColorData._Delay);
                if (_SnapToStart)
                {
                    if (mWidget != null)
                        mWidget.color = _ColorData._Keys[0];
                    if (mCanvasGroup != null)
                        mCanvasGroup.alpha = _ColorData._Keys[0].a;
                }
            }

            if (Mode.REVERSE_ONESHOT == _Mode || Mode.REVERSE_INFINITE == _Mode)
                mTime = mLength;
            else
                mTime = 0.0f;

            mLengthCalculated = true;
            //if (Mode.REVERSE_ONESHOT == _Mode || Mode.REVERSE_INFINITE == _Mode)
            //    mTime = mLength;
            //else
            //mTime = 0.0f;
        }

        internal void UpdateLoopCount()
        {
            if (_LoopCount < 0)
                return;

            ++mLoopCount;
            if (mLoopCount >= _LoopCount)
            {
                mTime = mLength;
                Stop();
            }
        }

        protected virtual void Cache()
        {
            if (_PositionData._UsePositionAsOffset)
                mDefaultPosition = mTransform.localPosition;

            if (_ColorData != null && _ColorData._Keys.Length > 1)
            {
                mWidget = mTransform.GetComponent<MaskableGraphic>();
                mCanvasGroup = mTransform.GetComponent<CanvasGroup>();
            }
        }

        public virtual void Update()
        {
            if (mDone)
                return;
            float dt = _IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime; ;
            if (mDelay > 0f)
            {
                mDelay -= dt;
                return;
            }
            else if (!mPlayInvoked)
            {
                if (mTriggerEvent || _TriggerEventOverride)
                    SendEvent(true);
                mPlayInvoked = true;
            }

            UpdateTime(dt);
            if (mTransform == null)
                return;

            if (_PositionData != null && _PositionData._Keys != null && _PositionData._Keys.Length > 0)
            {
                if (_PositionData.mDelay > 0)
                    _PositionData.mDelay -= dt;
                else
                {
                    _PositionData.UpdateTime(dt, _Mode, this);
                    if(_PositionData._UseWorldPosition)
                        mTransform.position = GetPos();
                    else
                        mTransform.localPosition = GetPos();
                }
            }

            if (_RotationData != null && _RotationData._Keys != null && _RotationData._Keys.Length > 0)
            {
                if (_RotationData.mDelay > 0)
                    _RotationData.mDelay -= dt;
                else
                {
                    _RotationData.UpdateTime(dt, _Mode, this);
                    mTransform.localRotation = GetRotation();
                }
            }
            if (_ScaleData != null && _ScaleData._Keys != null && _ScaleData._Keys.Length > 1)
            {
                if (_ScaleData.mDelay > 0)
                    _ScaleData.mDelay -= dt;
                else
                {
                    _ScaleData.UpdateTime(dt, _Mode, this);
                    mTransform.localScale = GetScale();
                }
            }
            if (_ColorData != null && _ColorData._Keys != null && _ColorData._Keys.Length > 0)
            {
                if (_ColorData.mDelay > 0)
                    _ColorData.mDelay -= dt;
                else
                {
                    _ColorData.UpdateTime(dt, _Mode, this);
                    SetColor(mTime);
                }
            }
        }

        public void SetColor(float time)
        {
            Color clr = GetColor(time);
            if (mWidget != null)
                mWidget.color = clr;
            if (mCanvasGroup != null)
                mCanvasGroup.alpha = clr.a;
        }

        public void ReInitDefaultPosition(bool usePosAsOffset)
        {
            _PositionData._UsePositionAsOffset = usePosAsOffset;
            mDefaultPosition = mTransform.localPosition;
        }

        public void SetPositionKeys(Vector3[] keys, bool useWorldPosition)
        {
            _PositionData._UseWorldPosition = useWorldPosition;
            _PositionData._Keys = keys;
        }

        public void SetRotationKeys(Vector3[] keys)
        {
            _RotationData._Keys = keys;
        }

        private void UpdateTime(float dt)
        {
            if (!mLengthCalculated)
                CalculateLength();

            switch (_Mode)
            {
                case Mode.FORWARD_INFINITE:
                    {
                        mTime += (dt * _TimeMultiplier);
                        if (mTime >= mLength)
                        {
                            mDelay = _Delay;
                            mTime = 0.0f;
                        }
                    }
                    break;

                case Mode.FORWARD_ONESHOT:
                    {
                        mTime += (dt * _TimeMultiplier);
                        //if (mTime >= mLength)
                        //{
                            ////No more updates required
                            //Stop();
                            //return;
                        //}
                    }
                    break;

                case Mode.REVERSE_INFINITE:
                    {
                        mTime -= (dt * _TimeMultiplier);
                        //if (mTime <= 0.0f)
                        //{
                            //mTime = mLength;
                            //// Dispatch looped signal here
                        //}
                    }
                    break;

                case Mode.REVERSE_ONESHOT:
                    {
                        mTime -= (dt * _TimeMultiplier);
                        //if (mTime <= 0.0f)
                        //{
                        //    mTime = 0f;
                        //    //No more updates required
                        //    Stop();
                        //    return;
                        //}
                    }
                    break;

                case Mode.PINGPONG:
                    {
                        mTime += (dt * _TimeMultiplier);
                        //if (_TimeMultiplier > 0.0f && mTime >= mLength)
                        //    _TimeMultiplier = -1.0f;
                        //else if (_TimeMultiplier < 0.0f && mTime <= mFirstKeyTime)
                        //_TimeMultiplier = 1.0f;
                    }
                    break;
            }
        }

        public Vector3 GetPos()
        {
            Vector3 pos = _PositionData.GetValue();
            if (_PositionData._UsePositionAsOffset)
                pos += mDefaultPosition;
            return pos;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(_RotationData.GetValue());
        }

        public Vector3 GetScale()
        {
            return _ScaleData.GetValue();
        }

        public Color GetColor(float time)
        {
            time /= _ColorData._Duration;

            float scaledT = time * (_ColorData._Keys.Length - 1);
            int index = (int)Mathf.Clamp(scaledT, 0, _ColorData._Keys.Length - 2);
            Color prevC = _ColorData._Keys[index];
            Color nextC = _ColorData._Keys[index + 1];
            float newt = scaledT - index;
            return Color.LerpUnclamped(prevC, nextC, _ColorData._Curve.Evaluate(newt));
        }

        private void SendEvent(bool start)
        {
            if (start)
            {
                if (_OnAnimationStart != null)
                {
                    if (pDebugAll || _Debug)
                    {
                        int count = _OnAnimationStart.GetPersistentEventCount();
                        string data = mAnimObj.GetObjectPath() + ", Anim Name : " + _Name + " Started";
                        if (count > 0)
                        {
                            for (int i = 0; i < count; ++i)
                                data += "\n" + _OnAnimationStart.GetPersistentTarget(i) + ":" + _OnAnimationStart.GetPersistentMethodName(i);
                        }
                        else
                            data += " No Listeners";
                        UnityEngine.Debug.LogError(data, mTransform);
                    }

                    _OnAnimationStart.Invoke(mAnimObj, _Name);
                }
            }
            else
            {
                if (_OnAnimationDone != null)
                {
                    if (pDebugAll || _Debug)
                    {
                        int count = _OnAnimationDone.GetPersistentEventCount();
                        string data = mAnimObj.GetObjectPath() + ", Anim Name : " + _Name + " Completed";
                        if (count > 0)
                        {
                            for (int i = 0; i < count; ++i)
                                data += "\n" + _OnAnimationDone.GetPersistentTarget(i) + ":" + _OnAnimationDone.GetPersistentMethodName(i);
                        }
                        else
                            data += " No Listeners";
                        UnityEngine.Debug.LogError(data, mTransform);
                    }

                    _OnAnimationDone.Invoke(mAnimObj, _Name);
                }
            }
        }

        //Clones the current object
        public UiAnimBase Clone()
        {
            UiAnimBase clone = (UiAnimBase)MemberwiseClone();
            if (_PositionData != null && _PositionData._Keys != null && _PositionData._Keys.Length > 0)
                clone._PositionData = _PositionData.Clone();

            if (_RotationData != null && _RotationData._Keys != null && _RotationData._Keys.Length > 0)
                clone._RotationData = _RotationData.Clone();

            if (_ScaleData != null && _ScaleData._Keys != null && _ScaleData._Keys.Length > 0)
                clone._ScaleData = _ScaleData.Clone();

            if (_ColorData != null && _ColorData._Keys != null && _ColorData._Keys.Length > 0)
                clone._ColorData = _ColorData.Clone();

            clone._OnAnimationDone = new CGUiAnimEvent();
            clone._OnAnimationStart = new CGUiAnimEvent();

            return clone;
        }

        //Create a new object
        public static UiAnimBase Create()
        {
            UiAnimBase obj = new UiAnimBase();
            return obj;
        }

#if UNITY_EDITOR
        public Color GetColor(Transform animObj)
        {
            if (mWidget == null)
                mWidget = animObj.GetComponent<Image>();
            if (mWidget != null)
                return mWidget.color;
            
            if (mCanvasGroup == null)
                mCanvasGroup = animObj.GetComponent<CanvasGroup>();
            if (mCanvasGroup != null)
                return new Color(1,1,1, mCanvasGroup.alpha);

            return Color.white;
        }

        public void SetColor(Color color)
        {
            if (mWidget != null)
                mWidget.color = color;
            if (mCanvasGroup != null)
                mCanvasGroup.alpha = color.a;
        }

        public void SetTime(float time)
        {
            if (_PositionData != null && _PositionData.NumKeys() > 0 && _PositionData._Curve.length > 1)
                _PositionData.SetTime(time);
            if (_RotationData != null && _RotationData.NumKeys() > 0 && _RotationData._Curve.length > 1)
                _RotationData.SetTime(time);
            if (_ScaleData != null && _ScaleData.NumKeys() > 0 && _ScaleData._Curve.length > 1)
                _ScaleData.SetTime(time);
            //if (_ColorData != null && _ColorData.NumKeys() > 0 && _ColorData._Curve.length > 1)
             //   SetColor(mTimeLineTimer);
        }
#endif
    }
}