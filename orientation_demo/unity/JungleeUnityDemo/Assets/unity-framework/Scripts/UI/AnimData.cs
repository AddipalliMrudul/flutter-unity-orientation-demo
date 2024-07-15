using UnityEngine;

namespace XcelerateGames.UI.Animations
{
    #region Anim Data Base
    [System.Serializable]
    public class BaseAnimData
    {
        public float _Duration = 0f;
        public float _Delay = 0f;
        public AnimationCurve _Curve = null;
        [HideInInspector] public float mDelay = 0f;
        protected float mTime;

        public void UpdateTime(float dt, Mode _Mode, UiAnimBase animBase)
        {
            //if (!mLengthCalculated)
            //CalculateLength();

            switch (_Mode)
            {
                case Mode.FORWARD_INFINITE:
                    {
                        mTime += (dt * animBase._TimeMultiplier);
                        //if (mTime >= _Duration)
                        //{
                        //    mDelay = _Delay;
                        //    mTime = 0.0f;
                        //}
                        if (animBase._TimeMultiplier > 0.0f && mTime >= _Duration)
                        {
                            mDelay = _Delay;
                            mTime = 0.0f;
                            animBase.UpdateLoopCount();
                        }
                        else if (animBase._TimeMultiplier < 0.0f && mTime <= 0/*mFirstKeyTime*/)
                        {
                            animBase._TimeMultiplier = 1.0f;
                            animBase.UpdateLoopCount();
                        }
                    }
                    break;

                case Mode.FORWARD_ONESHOT:
                    {
                        mTime += (dt * animBase._TimeMultiplier);
                        if (mTime >= _Duration)
                        {
                            //No more updates required
                            animBase.Stop();
                            return;
                        }
                    }
                    break;

                case Mode.REVERSE_INFINITE:
                    {
                        mTime -= (dt * animBase._TimeMultiplier);
                        if (mTime <= 0.0f)
                        {
                            mTime = _Duration;
                            // Dispatch looped signal here
                        }
                    }
                    break;

                case Mode.REVERSE_ONESHOT:
                    {
                        mTime -= (dt * animBase._TimeMultiplier);
                        if (mTime <= 0.0f)
                        {
                            mTime = 0f;
                            //No more updates required
                            animBase.Stop();
                            return;
                        }
                    }
                    break;

                case Mode.PINGPONG:
                    {
                        mTime += (dt * animBase._TimeMultiplier);
                        if (animBase._TimeMultiplier > 0.0f && mTime >= _Duration)
                        {
                            animBase._TimeMultiplier = -1.0f;
                            animBase.UpdateLoopCount();
                        }
                        else if (animBase._TimeMultiplier < 0.0f && mTime <= 0/*mFirstKeyTime*/)
                        {
                            animBase._TimeMultiplier = 1.0f;
                            animBase.UpdateLoopCount();
                        }
                    }
                    break;
            }
        }
    }
    #endregion Anim Data Base

    #region Anim Data
    [System.Serializable]
    public class AnimData : BaseAnimData
    {
        public Vector3[] _Keys = null;

        public virtual AnimData Clone()
        {
            AnimData clonedObj = new AnimData();
            clonedObj._Duration = _Duration;
            clonedObj._Keys = new Vector3[_Keys.Length];
            _Keys.CopyTo(clonedObj._Keys, 0);
            clonedObj._Curve = new AnimationCurve();
            clonedObj._Curve.keys = _Curve.keys;
            return clonedObj;
        }

        public virtual void Reset()
        {
            mDelay = _Delay;
            mTime = 0;
        }

        public Vector3 GetValue()
        {
            float time = mTime / _Duration;
            float scaledT = time * (_Keys.Length - 1);
            int index = (int)Mathf.Clamp(scaledT, 0, _Keys.Length - 2);
            Vector3 prevC = _Keys[index];
            Vector3 nextC = _Keys[index + 1];
            float newt = scaledT - index;

            return Vector3.LerpUnclamped(prevC, nextC, _Curve.Evaluate(newt));
        }

        #region Editor Only code
#if UNITY_EDITOR

        public int NumKeys()
        {
            return (_Keys == null ? 0 : _Keys.Length);
        }

        public virtual void Init()
        {
            _Keys = new Vector3[0];
            _Curve = new AnimationCurve();
        }

        public void SetTime(float time)
        {
            mTime = time - _Delay;
        }

#endif
        #endregion
    }
    #endregion

    #region Position Data
    [System.Serializable]
    public class PositionAnimData : AnimData
    {
        //If set to true, the exposed position values are added with the current transform position.
        public bool _UsePositionAsOffset = true;
        //If set to true, animation will start from current position
        public bool _StartFromCurrentPos = false;
        //If set tot true, world position will be used instead of local position.
        public bool _UseWorldPosition = false;

        public new PositionAnimData Clone()
        {
            PositionAnimData clonedObj = new PositionAnimData();
            clonedObj._Duration = _Duration;
            clonedObj._Keys = new Vector3[_Keys.Length];
            _Keys.CopyTo(clonedObj._Keys, 0);
            clonedObj._Curve = new AnimationCurve();
            clonedObj._Curve.keys = _Curve.keys;
            clonedObj._UsePositionAsOffset = _UsePositionAsOffset;

            return clonedObj;
        }
    }
    #endregion

    #region Color Data
    [System.Serializable]
    public class ColorData : BaseAnimData
    {
        public Color[] _Keys = null;

        internal ColorData Clone()
        {
            ColorData clonedObj = new ColorData();
            clonedObj._Duration = _Duration;
            clonedObj._Keys = new Color[_Keys.Length];
            _Keys.CopyTo(clonedObj._Keys, 0);
            clonedObj._Curve = new AnimationCurve();
            clonedObj._Curve.keys = _Curve.keys;
            return clonedObj;
        }

        public virtual void Reset()
        {
            mDelay = _Delay;
            mTime = 0;
        }
        #region Editor Only Code
#if UNITY_EDITOR

        public int NumKeys()
        {
            return (_Keys == null ? 0 : _Keys.Length);
        }

        public virtual void Init()
        {
            _Keys = new Color[0];
            _Curve = new AnimationCurve();
        }

#endif
        #endregion
        #endregion
    }
}