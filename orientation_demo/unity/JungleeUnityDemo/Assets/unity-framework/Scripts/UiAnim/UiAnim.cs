using System;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.UI.Animations
{
    [DisallowMultipleComponent]
    public class UiAnim : MonoBehaviour
    {
        public string _DefaultAnim = "";
        public bool _PlayOnAwake = false;
        public List<UiAnimBase> _Anims = null;
        private object _AnimParams;
        public CGUiAnimEvent _OnAnimationDone = null;

        //Following Animation names are used to trigger animation on button click
        public string ClickAnim { get { return "OnClick"; } }
        public string InAnim { get { return "In"; } }
        public string OutAnim { get { return "Out"; } }
        public bool IsPaused { get; set; }
        public bool IsAnyAnimPlaying { get { return mCurrentAnim != null; } }

        private UiAnimBase mCurrentAnim = null;
        private bool mInitialized = false;

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            if (_PlayOnAwake)
                Play(_DefaultAnim);
        }

        private void Update()
        {
            if (mCurrentAnim != null && !IsPaused)
                mCurrentAnim.Update();
        }

        public bool IsPlaying(string animName)
        {
            if (mCurrentAnim != null && Utilities.Equals(mCurrentAnim._Name, animName) && !mCurrentAnim.pDone)
                return true;
            return false;
        }

        public void Play(string animName)
        {
            Init();
            if (_Anims != null)
            {
                for (int i = 0; i < _Anims.Count; ++i)
                {
                    if (_Anims[i] != null && !string.IsNullOrEmpty(_Anims[i]._Name) && Utilities.Equals(_Anims[i]._Name, animName) /*&& this.gameObject.activeInHierarchy*/)
                    {
                        if (IsAnyAnimPlaying)
                        {
                            mCurrentAnim.Reset(false, true);
                            mCurrentAnim.Stop();
                        }
                        mCurrentAnim = _Anims[i];
                        mCurrentAnim.Play(true);
                        return;
                    }
                }
            }
            XDebug.LogWarning("Could not find anim : " + animName + " under ");
        }

        public void Play(string animName, float delay)
        {
            Init();

            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                mCurrentAnim = animBase;
                animBase._Delay = delay;
                animBase.Play(true);
                return;
            }
            XDebug.LogWarning("Could not find anim : " + animName + " under ");
        }


        public void PlayInAnimation()
        {
            Init();
            if (mCurrentAnim != null)
                return;
            mCurrentAnim = GetAnim(UiAnimBase.Category.In);
            if (mCurrentAnim != null)
                mCurrentAnim.Play(true);
        }

        public void PlayOutAnimation()
        {
            Init();
            mCurrentAnim = GetAnim(UiAnimBase.Category.Out);
            if (mCurrentAnim != null)
                mCurrentAnim.Play(true);
        }

        public UiAnimBase GetAnim(UiAnimBase.Category category)
        {
            if (_Anims != null)
            {
                for (int i = 0; i < _Anims.Count; ++i)
                {
                    if (_Anims[i] != null && _Anims[i]._Category == category && this.gameObject.activeInHierarchy)
                    {
                        return _Anims[i];
                    }
                }
            }
            if(Application.isEditor)
                XDebug.LogWarning("Could not find any anim for Category : " + category);
            return null;
        }

        public bool SilentPlay(string animName)
        {
            Init();
            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                mCurrentAnim = animBase;
                animBase.Play(false);
                return true;
            }
            XDebug.LogWarning("Could not find anim : " + animName + " under ");
            return false;
        }

        public void PlayReverse(string animName)
        {
            Init();
            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                mCurrentAnim = animBase;
                animBase.pMode = Mode.REVERSE_ONESHOT;
                animBase.Play(true);
                return;
            }
            XDebug.LogWarning("Could not find anim : " + animName + " under " + this.GetObjectPath());
        }

        public void PlayAnim()
        {
            Play(_DefaultAnim);
        }

        public void PlayAnimReverse()
        {
            PlayReverse(_DefaultAnim);
        }

        public void PlayAnimReverse(string animName)
        {
            PlayReverse(animName);
        }

        public UiAnimBase GetAnim(string animName)
        {
            return _Anims.Find(anim => anim._Name.Equals(animName));
        }

        [ContextMenu("Play Default Anim")]
        public void PlayDefaultAnim()
        {
            Play(_DefaultAnim);
        }

        public void OnAnimationDone(string animName)
        {
            if (mCurrentAnim != null && mCurrentAnim._DestroyOnComplete &&
               (mCurrentAnim._Mode == Mode.FORWARD_ONESHOT || mCurrentAnim._Mode == Mode.REVERSE_ONESHOT))
            {
                XDebug.LogWarning("Object Destroyed :" + gameObject.name);
                GameObject.Destroy(gameObject);
            }

            mCurrentAnim = null;
            if (_OnAnimationDone != null)
            {
                _OnAnimationDone.Invoke(this, animName);
            }
        }

        private void Init()
        {
            if (!mInitialized)
            {
                IsPaused = false;
                mInitialized = true;
                for (int i = 0; i < _Anims.Count; ++i)
                {
                    if (_Anims[i]._Reference)
                    {
                        if (XDebug.CanLog(XDebug.Mask.UI))
                            Debug.LogFormat("Loading anim {0} of {1} from UI Library", _Anims[i]._Name, gameObject.GetObjectPath());
                        _Anims[i] = UiAnimLibrary.GetAnim(_Anims[i]._Name, _Anims[i]);
                    }
                    _Anims[i].Init(transform, this);
                }
            }
        }

        public void Stop()
        {
            if (mCurrentAnim != null)
                mCurrentAnim.Stop();
            mCurrentAnim = null;
        }

        private void StopAndReset(string animName, bool toBeginning)
        {
            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                animBase.Reset(toBeginning, true);
                animBase.Stop();
                if (mCurrentAnim != null && mCurrentAnim._Name == animBase._Name)
                    mCurrentAnim = null;
                return;
            }
            XDebug.LogError("StopAndReset::Could not find anim : " + animName + " under " + this.GetObjectPath());
        }

        //Resets the animation without triggering any event
        public void SilentReset(string animName, bool toBeginning)
        {
            if (!mInitialized)
                Init();

            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                animBase.Reset(toBeginning, false);
                animBase.Stop();
                return;
            }
        }

        public void StopAndResetToBeginning(string animName)
        {
            StopAndReset(animName, true);
        }

        //editor displays at max of 1 param for anim3d or could've used SilentReset
        public void StopAndResetToBeginningSilent(string animName)
        {
            SilentReset(animName, true);
        }

        public void StopAndResetToEnd(string animName)
        {
            StopAndReset(animName, false);
        }

        /// <summary>
        /// Reinits default position.
        /// </summary>
        /// <param name="inAnimName"></param>
        /// <param name="usePositionAsOffset">Wheather animation should start from the re-inited position</param>
        public void ReInitDefaultPosition(string inAnimName, bool usePositionAsOffset)
        {
            if (!mInitialized)
                Init();

            UiAnimBase animBase = GetAnim(inAnimName);
            if (animBase != null)
            {
                animBase.ReInitDefaultPosition(usePositionAsOffset);
                return;
            }
        }

        private void OnDestroy()
        {
            //Inform our parent UI class that the object is being destroyed.
            gameObject.SendMessageUpwards("OnAnimDestroy", this, SendMessageOptions.DontRequireReceiver);
        }

        public void SetPositionKeys(string animName, Vector3[] keys, bool useWorldPosition, bool playAnim, float delay = 0)
        {
            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                animBase.SetPositionKeys(keys, useWorldPosition);
                animBase._Delay = delay;
                if(playAnim)
                {
                    mCurrentAnim = animBase;
                    animBase.Play(true);
                }
            }
        }

        public void SetRotationKeys(string animName, Vector3[] keys, bool playAnim)
        {
            UiAnimBase animBase = GetAnim(animName);
            if (animBase != null)
            {
                animBase.SetRotationKeys(keys);
                if (playAnim)
                {
                    if (mCurrentAnim != null)
                        mCurrentAnim.Stop();
                    mCurrentAnim = animBase;
                    animBase.Play(true);
                }
            }
        }

        public void SetPositionTarget(string animName, Transform target, bool useWorldPosition, bool playAnim)
        {
            Vector3[] keys = new Vector3[2];
            keys[0] = useWorldPosition ? transform.position : transform.localPosition;
            keys[1] = useWorldPosition ? target.position : target.localPosition;
            SetPositionKeys(animName, keys, useWorldPosition, playAnim);
        }

        public void SetAnimParams(object animParams)
        {
            _AnimParams = animParams;
        }

        public object GetAnimParams()
        {
            return this._AnimParams;
        }

#region Anim Helpers : These are just helper functions to set & reset scale, As of now, we cannot pass vector as argument to a function
        public void SetScaleToZero()
        {
            transform.localScale = Vector3.zero;
        }

        public void SetScaleToOne()
        {
            transform.localScale = Vector3.one;
        }
#endregion
#if UNITY_EDITOR
        public void OnValidate()
        {
            //if (_Anims != null)
            //{
            //    //Init();
            //    foreach (UiAnimBase anim in _Anims)
            //        anim.CalculateLength();
            //}
        }
#endif
    }
}
