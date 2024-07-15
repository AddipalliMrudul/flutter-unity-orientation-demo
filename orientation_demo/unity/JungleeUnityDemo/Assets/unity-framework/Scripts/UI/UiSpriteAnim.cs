using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    //Reference: https://www.youtube.com/watch?v=-NqNDLT-c48
    [RequireComponent(typeof(Image))]
    public class UiSpriteAnim : MonoBehaviour
    {
        public enum State
        {
            None,
            Playing,
            Paused,
            Complete
        }
        public string _SpriteSheetName;
        public int _StartFrame = 0;
        public int _FrameRate = 30;
        public bool _Loop = false;
        public bool _PlayOnAwake = false;
        public bool _IgnoreTimeScale = false;
        public float _Speed = 1f;

        public System.Action<UiSpriteAnim> OnComplete = null;

        private int mCurrentIndex = 0;
        private State mState = State.None;

        private float mElapsedTime = 0f;
        private float mTimer = 0f;
        private float mTimePerFrame = 0f;

        private Image mImage = null;

        public Image pImage { get { return mImage; } }

        private Sprite[] mSprites;

        #region Private/Protected methods
        private void Awake()
        {
            mImage = GetComponent<Image>();
            if (mImage != null)
                LoadSpriteSheet();
            else
                Debug.LogError("Image component not found, No anim will be played", this);
        }

        private void LoadSpriteSheet()
        {
            if (!string.IsNullOrEmpty(_SpriteSheetName))
            {
                mSprites = Resources.LoadAll<Sprite>(_SpriteSheetName);
                if (mSprites != null && mSprites.Length > 0)
                {
                    if (_PlayOnAwake)
                    {
                        _StartFrame = Mathf.Clamp(_StartFrame, 0, mSprites.Length);
                        mCurrentIndex = _StartFrame;
                        mTimePerFrame = 1f / _FrameRate;
                        Play();
                    }
                }
                else
                    XDebug.LogError($"Error! Failed to load {_SpriteSheetName}");
            }
            else
                XDebug.LogError("_SpriteSheetName cannot be empty");
        }

        void Update()
        {
            if (mState == State.Playing)
            {
                mElapsedTime += (_IgnoreTimeScale ? Time.unscaledTime : Time.deltaTime) * _Speed;
                if (mElapsedTime >= mTimePerFrame)
                {
                    mElapsedTime = 0f;
                    ++mCurrentIndex;
                    if (mCurrentIndex == mSprites.Length)
                    {
                        if (_Loop)
                            mCurrentIndex = _StartFrame;
                        else
                        {
                            mState = State.Complete;
                            OnComplete?.Invoke(this);
                        }
                    }
                    SetSprite();
                }
            }
        }

        private void SetSprite()
        {
            if (mCurrentIndex >= 0 && mCurrentIndex < mSprites.Length)
                mImage.sprite = mSprites[mCurrentIndex];
        }

        private IEnumerator PlayTimedAnimation(float time)
        {
            mTimer = time;
            while (mTimer > 0f)
            {
                mTimer -= (_IgnoreTimeScale ? Time.unscaledTime : Time.deltaTime);
                mElapsedTime += (_IgnoreTimeScale ? Time.unscaledTime : Time.deltaTime) * _Speed;
                if (mElapsedTime >= mTimePerFrame)
                {
                    mElapsedTime = 0f;
                    ++mCurrentIndex;
                    if (mCurrentIndex == mSprites.Length)
                    {
                        mCurrentIndex = _StartFrame;
                    }
                    SetSprite();
                }
                yield return new WaitForEndOfFrame();
            }

            mState = State.Complete;
            OnComplete?.Invoke(this);
        }


        #endregion Private/Protected methods

        #region Public methods
        public void Play()
        {
            mElapsedTime = 0f;
            mState = State.Playing;
            SetSprite();
        }

        public void Play(float time)
        {
            StartCoroutine(PlayTimedAnimation(time));
        }

        /// <summary>
        /// Stop the Sprite Animation at First Frame or at Last Frame
        /// </summary>
        public void StopAtFirstFrame()
        {
            StopAtFrame(0);
        }

        public void StopAtLastFrame()
        {
            StopAtFrame(mSprites.Length - 1);
        }

        /// <summary>
        /// Stopping the Sprite Animation at Particular Frame 
        /// </summary>
        /// <param name="frame"></param>
        public void StopAtFrame(int frame)
        {
            mState = State.Complete;
            if (frame >= 0 && frame < mSprites.Length)
                mImage.sprite = mSprites[frame];
            else
                XDebug.LogException("Invalid Argument");
        }
        #endregion Public methods
    }
}
