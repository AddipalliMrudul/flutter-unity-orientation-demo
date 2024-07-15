using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.Audio;

namespace XcelerateGames.UI
{
    public class UiSpriteSwitcher : MonoBehaviour
    {
        #region Properties
        [SerializeField] protected Image _TargetImage = null;
        [SerializeField] protected Sprite[] _Sprites = null;
        [SerializeField] protected bool _CanShuffle = false;
        [SerializeField, Range(1, 60)] protected int _FrameRate = 1;
        [SerializeField] protected bool _PlayonAwake = false, _IgnoreTimeScale = false;

        protected float mElapsedTime = 0, mTimePerFrame = 0;
        protected int mCurrentIndex = -1;
        protected bool mIsPlaying = false;
        #endregion

        #region Protected Methods
        private void Awake()
        {
            Init();
        }

        protected virtual void Update()
        {
            mElapsedTime += _IgnoreTimeScale ? Time.unscaledTime : Time.deltaTime;
            if (mElapsedTime >= mTimePerFrame)
            {
                mElapsedTime = 0;
                mCurrentIndex = ++mCurrentIndex % _Sprites.Length;
                SetSprite();
            }
        }

        /// <summary>
        /// Context menu that works oly in Unity to help get refernces of components attached. Ex: Image, RawImage, Button & Text
        /// </summary>
        [ContextMenu("Get References")]
        protected virtual void GetReferences()
        {
            if (_TargetImage == null)
                _TargetImage = GetComponent<Image>();
            if (_TargetImage == null)
                Debug.LogError("UiSpriteSwitcher:: Image component not found, No anim will be played");
        }
        #endregion //Protected Methods

        #region Public Methods
        public virtual void Play()
        {
            if (_Sprites == null || _TargetImage == null)
            {
                XDebug.LogError($"UiSpriteSwitcher::{nameof(Play)}() _Sprites is null? {_Sprites == null}  & _TargetImage is null? {_TargetImage == null}");
                return;
            }
            enabled = true;
            mElapsedTime = 0;
        }

        public virtual void Stop()
        {
            enabled = false;
        }
        #endregion //Public Methods

        #region Private Methods
        private void Init()
        {
            mTimePerFrame = 1f / _FrameRate;
            if (_CanShuffle)
                ShuffleSprite();//TODO: Shuffle via extension methods
            mCurrentIndex = 0;
            SetSprite();
            enabled = _PlayonAwake;
        }

        public void ShuffleSprite()
        {
            Sprite temp;
            for (int i = _Sprites.Length - 1; i >= 0; i--)
            {
                int randomIndex = Random.Range(0, i);
                temp = _Sprites[randomIndex];
                _Sprites[randomIndex] = _Sprites[i];
                _Sprites[i] = temp;
            }
        }

        private void SetSprite()
        {
            _TargetImage.sprite = _Sprites[mCurrentIndex];
        }

        /// <summary>
        /// Called by Unity in IDE only to validate values
        /// </summary>
        private void OnValidate()
        {
            GetReferences();
        }
        #endregion //Private Methods
    }
}