using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.UI
{
    public class UiSpriteSheetAnim : MonoBehaviour
    {
        public float _FrameWidth = 1f;
        public float _FrameHeight = 1f;
        public int _FrameCount = 1;
        public float _OffsetX = 0f;
        public float _OffsetY = 0f;
        public int _StartFrame = 0;
        public int _FrameRate = 30;
        public bool _Loop = true;
        public bool _PlayOnAwake = false;
        public bool _IgnoreTimeScale = false;
        public float _Speed = 1f;
        public RawImage _RawImage = null;

        public System.Action<UiSpriteSheetAnim> OnComplete = null;

        protected int mCurrentIndex = 0;
        protected float mElapsedTime = 0f;
        protected float mTimer = 0f;
        protected float mTimePerFrame = 0f;
        protected string mTextureURL = null;
        Rect mRect;

        public RawImage pRawImage { get { return _RawImage; } }
        protected List<Vector2> mUVCoOrdinates = null;

        #region Private/Protected methods
        private void Awake()
        {
            enabled = false;
            GetReferences();
            if (_PlayOnAwake && _RawImage.texture != null)
            {
                SetupFrames();
                Play();
            }
        }

        /// <summary>
        /// Context menu that works oly in Unity to help get refernces of components attached.
        /// </summary>
        [ContextMenu("Get References")]
        private void GetReferences()
        {
            if (_RawImage == null)
                _RawImage = GetComponentInChildren<RawImage>();
        }

        private void OnTextureLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;

            Action<bool> callback = (Action<bool>)inUserData;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                _RawImage.texture = (Texture)inObject;
                callback?.Invoke(true);
                SetupFrames();
                Play();
            }
            else if (inEvent == ResourceEvent.ERROR)
                callback?.Invoke(false);
        }

        private void SetupFrames()
        {
            if (_RawImage.texture != null)
            {
                _RawImage.rectTransform.sizeDelta = new Vector2(_FrameWidth, _FrameHeight);
                mUVCoOrdinates = new List<Vector2>(_FrameCount);

                mRect.width = _FrameWidth / _RawImage.texture.width;
                mRect.height = _FrameHeight / _RawImage.texture.height;
                int numColumns = (int)(_RawImage.texture.width / _FrameWidth);
                int numRows = (int)(_RawImage.texture.height / _FrameHeight);
                float x = 0, y = 1 - mRect.height;
                for (int r = 0; r < numRows; ++r)
                {
                    for (int c = 0; c < numColumns; ++c)
                    {
                        mUVCoOrdinates.Add(new Vector2(x, y));
                        x += mRect.width;
                    }
                    x = 0f;
                    y -= mRect.height;
                }
                mTimePerFrame = 1f / _FrameRate;
                _StartFrame = Mathf.Clamp(_StartFrame, 0, _FrameCount);
                mCurrentIndex = _StartFrame;

            }
            else
                XDebug.LogError("_Texture cannot be empty");
        }

        void Update()
        {
            mElapsedTime += (_IgnoreTimeScale ? Time.unscaledTime : Time.deltaTime) * _Speed;
            if (mElapsedTime >= mTimePerFrame)
            {
                mElapsedTime = 0f;
                ++mCurrentIndex;
                if (mCurrentIndex == _FrameCount)
                {
                    if (_Loop)
                        mCurrentIndex = _StartFrame;
                    else
                    {
                        enabled = false;
                        OnComplete?.Invoke(this);
                    }
                }
                SetFrame();
            }
        }

        private void SetFrame()
        {
            try
            {
                if (mCurrentIndex >= 0 && mCurrentIndex < _FrameCount)
                {
                    mRect.position = mUVCoOrdinates[mCurrentIndex];
                    _RawImage.uvRect = mRect;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                XDebug.LogException($"index:{mCurrentIndex}, count:{mUVCoOrdinates.Count}, Image:{mTextureURL}, Message:\n {ex.Message}");
                Pause();
            }
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
                    if (mCurrentIndex == _FrameCount)
                    {
                        mCurrentIndex = _StartFrame;
                    }
                    SetFrame();
                }
                yield return new WaitForEndOfFrame();
            }

            OnComplete?.Invoke(this);
        }
        #endregion Private/Protected methods

        #region Public methods
        [ContextMenu("Play")]
        public void Play()
        {
            mElapsedTime = 0f;
            enabled = true;
            SetFrame();
        }

        public void Play(float time)
        {
            StartCoroutine(PlayTimedAnimation(time));
        }

        public void Pause()
        {
            enabled = false;
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
            StopAtFrame(_FrameCount - 1);
        }

        /// <summary>
        /// Stopping the Sprite Animation at Particular Frame 
        /// </summary>
        /// <param name="frame"></param>
        public void StopAtFrame(int frame)
        {
            if (frame >= 0 && frame < _FrameCount)
            {
                mCurrentIndex = frame;
                SetFrame();
            }
            else
                XDebug.LogException("Invalid Argument");
        }

        public void SetTexture(Texture texture)
        {
            _RawImage.texture = texture;
        }

        public void SetTexture(string textureURL, Action<bool> callback)
        {
            mTextureURL = textureURL;
            ResourceManager.LoadURL(textureURL, OnTextureLoaded, ResourceManager.ResourceType.Texture, callback);
        }
        #endregion Public methods

        #region Editor only code
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] protected string _textureURL = null;
        [ContextMenu("SetTextureAndPlay")]
        public void SetTextureAndPlay()
        {
            SetTexture(_textureURL, null);
        }
#endif //UNITY_EDITOR
        #endregion Editor only code
    }
}
