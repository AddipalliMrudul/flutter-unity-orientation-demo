#if VIDEO_ENABLED
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using XcelerateGames.Audio;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Video
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class UiVideoPlayer : UiBase
    {
        #region Member Variables & Properties

        public float _Width = 2960;
        public float _Height = 1440;

        public UiItem _PauseBtn, _PlayBtn;
        public Slider _PlayProgress, _Speed;

        public UiToggleItem _MuteBtn = null;

        public RawImage _RawImage = null;

        public UiItem _CurrentTime, _TotalTime;

        public float _ControlsTimeout = 3f;

        public GameObject _ControlsGroup = null;
        public GameObject _LoadingGroup = null;

        public bool _AllowSeeking = true;
        public bool _AllowClose = true;
        public bool _ShowLoadingCursor = true;

        //properties of the video player
        private bool mIsDone;

        private bool mShowingControls = false;
        private bool mIsMusicMuted = false;
        private float mElapsedTime = 0f;

        protected VideoPlayer mVideoPlayer = null;
        protected AudioSource mAudioSource = null;
        protected bool mAdjustSize = false;

        protected ScreenOrientation mPrevOrientation = ScreenOrientation.Portrait;

        public bool IsPlaying { get { return mVideoPlayer.isPlaying; } }

        public bool IsLooping { get { return mVideoPlayer.isLooping; } }

        public bool IsPrepared { get { return mVideoPlayer.isPrepared; } }

        public bool IsDone { get { return mIsDone; } }

        public double Length { get { return mVideoPlayer.time; } }

        public ulong Duration { get { return (ulong)(mVideoPlayer.frameCount / mVideoPlayer.frameRate); } }

        public double NTime { get { return Length / Duration; } }
        #endregion Member Variables & Properties

        #region Signals & Models
        [InjectSignal] protected SigVideoPlayerCreated mSigVideoPlayerCreated = null;
        [InjectSignal] protected SigVideoPlayerClosed mSigVideoPlayerClosed = null;
        [InjectSignal] protected SigVideoPlayerComplete mSigVideoPlayerComplete = null;
        [InjectSignal] protected SigVideoPlaybackStarted mSigVideoPlaybackStarted = null;
        #endregion Signals & Models

        protected override void Awake()
        {
            base.Awake();
            mVideoPlayer = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();

            mVideoPlayer.errorReceived += OnErrorReceived;
            mVideoPlayer.frameReady += OnFrameReady;
            mVideoPlayer.loopPointReached += OnLoopPointReached;
            mVideoPlayer.prepareCompleted += OnPrepareCompleted;
            mVideoPlayer.seekCompleted += OnSeekCompleted;
            mVideoPlayer.started += OnStarted;

            mAudioSource = gameObject.GetComponent<AudioSource>();

            if (_AllowSeeking)
                _PlayProgress.onValueChanged.AddListener(Seek);
            _PlayProgress.interactable = _AllowSeeking;

            Orientation.OnOrientationChange += OnOrientationChange;
            mSigVideoPlayerCreated.Dispatch(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mVideoPlayer.errorReceived -= OnErrorReceived;
            mVideoPlayer.frameReady -= OnFrameReady;
            mVideoPlayer.loopPointReached -= OnLoopPointReached;
            mVideoPlayer.prepareCompleted -= OnPrepareCompleted;
            mVideoPlayer.seekCompleted -= OnSeekCompleted;
            mVideoPlayer.started -= OnStarted;

            Orientation.OnOrientationChange -= OnOrientationChange;
            mSigVideoPlayerClosed.Dispatch();
        }

        protected virtual void OnOrientationChange(ScreenOrientation orientation)
        {
            AdjustSize(orientation);
        }

        #region Unity VideoPlayer callbacks

        protected virtual void OnErrorReceived(UnityEngine.Video.VideoPlayer v, string msg)
        {
            XDebug.LogError($"video player error: {msg}", XDebug.Mask.Video);
        }

        protected virtual void OnFrameReady(UnityEngine.Video.VideoPlayer v, long frame)
        {
            //cpu tax is heavy
        }

        protected virtual void OnLoopPointReached(UnityEngine.Video.VideoPlayer v)
        {
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("video player loop point reached", XDebug.Mask.Video);
            mIsDone = true;
            if (!mVideoPlayer.isLooping)
                OnVideoEnd();
        }

        protected virtual void OnPrepareCompleted(UnityEngine.Video.VideoPlayer v)
        {
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("video player finished preparing", XDebug.Mask.Video);
            _LoadingGroup.SetActive(false);
            mIsDone = false;
            _RawImage.texture = mVideoPlayer.texture;

            AdjustSize(Screen.orientation);
            PlayVideo();
            mSigVideoPlaybackStarted.Dispatch();
        }

        protected virtual void AdjustSize(ScreenOrientation orientation)
        {
            if (!mAdjustSize)
                return;
            _RawImage.ScaleToDimension(new Vector2(mVideoPlayer.width, mVideoPlayer.height), true);
            AspectRatioFitter aspectRatioFitter = _RawImage.GetComponent<AspectRatioFitter>();

            float aspectRatio = mVideoPlayer.width / (float)mVideoPlayer.height;
            float multiplier = 0f;

            if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                multiplier = _Height / mVideoPlayer.height;
                _RawImage.rectTransform.Height(mVideoPlayer.height * multiplier);
            }
            else
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                multiplier = _Width / mVideoPlayer.width;
                _RawImage.rectTransform.Width(mVideoPlayer.width * multiplier);
            }
            aspectRatioFitter.aspectRatio = aspectRatio;

            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log($"Video res: {mVideoPlayer.width}x{mVideoPlayer.height}" +
                $", Screen res: {_Width}x{_Height}" +
                $", AspectRatio: {aspectRatio}, Orientation: {orientation}, multiplier: {multiplier}", XDebug.Mask.Video);
        }

        protected virtual void AdjustSize(float width, float height)
        {
            _Panel.sizeDelta = new Vector2(width, height);
        }

        protected virtual void OnSeekCompleted(UnityEngine.Video.VideoPlayer v)
        {
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("video player finished seeking", XDebug.Mask.Video);
            mIsDone = false;
        }

        protected virtual void OnStarted(UnityEngine.Video.VideoPlayer v)
        {
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("video player started", XDebug.Mask.Video);
            if (_ShowLoadingCursor)
                UiLoadingCursor.Show(false);
            _TotalTime.text = TimeUtilities.GetTimerStringShortNoAbbreviations((long)Duration);
        }

        #endregion Unity VideoPlayer callbacks

        protected override void Update()
        {
            base.Update();
            if (!IsPrepared) return;

            if (mVideoPlayer.isPlaying)
            {
                if (EventSystem.current.currentSelectedGameObject != _PlayProgress.gameObject)
                    _PlayProgress.value = (float)NTime;
                _CurrentTime.text = TimeUtilities.GetTimerStringShortNoAbbreviations((long)Length);
            }

            if (!mShowingControls && (Input.touchCount >= 1 || Input.anyKeyDown))
            {
                mShowingControls = true;
                mElapsedTime = 0f;
                if (mVideoPlayer.isPlaying)
                    _PauseBtn?.gameObject.SetActive(true);
                _ControlsGroup.SetActive(true);
            }
            if (mShowingControls)
            {
                //If we are already showing controls, if there is any input reset timer.
                if (Input.touchCount >= 1 || Input.anyKeyDown)
                    mElapsedTime = 0f;

                mElapsedTime += Time.deltaTime;

                if (mElapsedTime >= _ControlsTimeout)
                {
                    _ControlsGroup.SetActive(false);
                    mShowingControls = false;
                }
            }
        }

        protected virtual void OnVideoEnd()
        {
            mVideoPlayer.Stop();
            StartCoroutine(ChangeOrientation());
            mSigVideoPlayerComplete.Dispatch();
        }

        protected IEnumerator ChangeOrientation()
        {
            yield return new WaitForSeconds(.1f);
            Screen.orientation = mPrevOrientation;
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("New Orientation : " + Screen.orientation, XDebug.Mask.Video);
            yield return null;
            gameObject.SetActive(false);
            AudioController.MusicMuted = mIsMusicMuted;
            if (_ShowLoadingCursor)
                UiLoadingCursor.Show(false);
        }

        protected IEnumerator ChangeOrientation(ScreenOrientation orientation)
        {
            yield return new WaitForSeconds(.1f);
            Screen.orientation = orientation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("New Orientation set to : " + orientation, XDebug.Mask.Video);
            yield return null;
        }

        //File name must have an extension such as .mp4,.avi,.mov
        protected IEnumerator LoadVideo(string fileName)
        {
            _LoadingGroup.SetActive(true);
            if (mVideoPlayer.url == fileName)
                yield return null;

            mAudioSource.mute = false;
            _ControlsGroup.SetActive(false);
            mShowingControls = false;

            yield return new WaitForSeconds(0.5f);

            mVideoPlayer.source = VideoSource.Url;
            mVideoPlayer.url = fileName;
            mVideoPlayer.Prepare();

            while (!mVideoPlayer.isPrepared)
            {
                //Log("Waiting for preparation");
                yield return new WaitForSeconds(1);

                break;
            }

            _RawImage.texture = mVideoPlayer.texture;
        }

        protected IEnumerator LoadVideo(VideoClip videoClip)
        {
            _LoadingGroup.SetActive(true);

            mAudioSource.mute = false;
            _ControlsGroup.SetActive(false);
            mShowingControls = false;

            yield return new WaitForSeconds(0.5f);
            mVideoPlayer.source = VideoSource.VideoClip;
            mVideoPlayer.clip = videoClip;
            mVideoPlayer.Prepare();

            while (!mVideoPlayer.isPrepared)
            {
                //Log("Waiting for preparation");
                yield return new WaitForSeconds(1);

                break;
            }

            _RawImage.texture = mVideoPlayer.texture;
        }

        protected virtual void PlayVideo()
        {
            if (!IsPrepared) return;
            _PlayBtn.gameObject.SetActive(false);
            _PauseBtn?.gameObject.SetActive(true);

            _RawImage.texture = mVideoPlayer.texture;
            mVideoPlayer.Play();
        }

        protected virtual void PauseVideo()
        {
            if (!IsPlaying) return;
            _PlayBtn.gameObject.SetActive(true);
            _PauseBtn?.gameObject.SetActive(false);
            mVideoPlayer.Pause();
        }

        protected virtual void RestartVideo()
        {
            if (!IsPrepared) return;
            PauseVideo();
            Seek(0);
        }

        protected virtual void LoopVideo(bool toggle)
        {
            if (!IsPrepared) return;
            mVideoPlayer.isLooping = toggle;
        }

        protected virtual void Seek(float inTime)
        {
            if (!mVideoPlayer.canSetTime) return;
            if (!IsPrepared) return;
            if (!_AllowSeeking || EventSystem.current.currentSelectedGameObject == _PlayProgress.gameObject)
            {
                inTime = Mathf.Clamp(inTime, 0, 1);
                mVideoPlayer.time = inTime * Duration;
            }
        }

        protected virtual void IncrementPlaybackSpeed()
        {
            if (!mVideoPlayer.canSetPlaybackSpeed) return;

            mVideoPlayer.playbackSpeed += 1;
            mVideoPlayer.playbackSpeed = Mathf.Clamp(mVideoPlayer.playbackSpeed, 0, 10);
        }

        protected virtual void DecrementPlaybackSpeed()
        {
            if (!mVideoPlayer.canSetPlaybackSpeed) return;

            mVideoPlayer.playbackSpeed -= 1;
            mVideoPlayer.playbackSpeed = Mathf.Clamp(mVideoPlayer.playbackSpeed, 0, 10);
        }

        protected virtual void EnableCloseButton()
        {
            _BackBtn.SetActive(true);
        }

        #region UI callbacks

        public void OnClickPause()
        {
            PauseVideo();
        }

        public void OnClickPlay()
        {
            PlayVideo();
        }

        public void OnClickMute()
        {
            mAudioSource.mute = !mAudioSource.mute;
            _MuteBtn.isOn = mAudioSource.mute;
        }

        public virtual void OnClickClose()
        {
            Seek(1);
            OnVideoEnd();
        }

        public void OnSpeedChange(float speed)
        {
            mVideoPlayer.playbackSpeed = speed;
        }

        #endregion UI callbacks

        #region Public Methods

        public virtual void Play(string fileName, ScreenOrientation orientation, int closeButtonTimer, bool adjustSize, bool showLoadingCursor = true)
        {
            if (fileName.IsNullOrEmpty())
            {
                if (XDebug.CanLog(XDebug.Mask.Video))
                    XDebug.Log("Null or empty url passed for the video file, Returning now", XDebug.Mask.Video);
                return;
            }
            mAdjustSize = adjustSize;
            _ShowLoadingCursor = showLoadingCursor;
            if (_ShowLoadingCursor)
                UiLoadingCursor.Show(true);

            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("Play called:: FileName : " + fileName + " Orientation : " + orientation);
            gameObject.SetActive(true);
            _MuteBtn.isOn = false;
            mIsMusicMuted = AudioController.MusicMuted;
            mVideoPlayer.playbackSpeed = 1;
            AudioController.MusicMuted = true;
            StartCoroutine(LoadVideo(fileName));
            if (mPrevOrientation != orientation)
            {
                mPrevOrientation = Screen.orientation;
                StartCoroutine(ChangeOrientation(orientation));
            }
            _BackBtn.SetActive(false);
            if (closeButtonTimer > 0)
            {
                Invoke("EnableCloseButton", closeButtonTimer);
            }
        }

        public virtual void Play(VideoClip videoClip, ScreenOrientation orientation, bool adjustSize, bool showLoadingCursor = true)
        {
            if (videoClip == null)
            {
                if (XDebug.CanLog(XDebug.Mask.Video))
                    XDebug.Log("videoClip is null, Returning now", XDebug.Mask.Video);
                return;
            }
            if (XDebug.CanLog(XDebug.Mask.Video))
                XDebug.Log("Play called:: Clip Name : " + videoClip.name + " Orientation : " + orientation, XDebug.Mask.Video);
            _ShowLoadingCursor = showLoadingCursor;

            mAdjustSize = adjustSize;
            gameObject.SetActive(true);
            _MuteBtn.isOn = false;
            mIsMusicMuted = AudioController.MusicMuted;
            mVideoPlayer.playbackSpeed = 1;
            AudioController.MusicMuted = true;
            StartCoroutine(LoadVideo(videoClip));
            if (mPrevOrientation != orientation)
            {
                mPrevOrientation = Screen.orientation;
                StartCoroutine(ChangeOrientation(orientation));
            }
        }

        #endregion Public Methods

        #region Editor Only Code
#if UNITY_EDITOR
        [ContextMenu("Adjust Size")]
        private void AdjustSize()
        {
            AdjustSize(_Width, _Height);
        }
#endif //UNITY_EDITOR

        #endregion Editor Only Code
    }
}
#endif //VIDEO_ENABLED