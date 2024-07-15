using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;
using XcelerateGames.Audio;
using XcelerateGames.UI;

namespace XcelerateGames
{
    public class UiAudioPlayer : UiBase
    {
        #region Properties
        public UiItem _PlayBtn, _PauseBtn, _TotalTime, _CurrentTime;
        public UiProgressBar _DownloadProgressBar;
        public Slider _Slider;

        private bool mIsPlaying = false, mDragging = false;
        private AudioSource mAudioSource = null;
        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        public void OnClickPlay()
        {
            mIsPlaying = true;
            mAudioSource.Play();
            _PlayBtn.SetActive(false);
        }

        public void OnClickPause()
        {
            _PlayBtn.SetActive(true);
            mAudioSource.Pause();
        }

        public void OnSliderValueChange()
        {
            //Debug.Log(_Slider.value);
            _CurrentTime.text = TimeUtilities.GetTimerStringShortNoAbbreviations((long)(_Slider.value * mAudioSource.time));
            mDragging = true;
            mAudioSource.Stop();
        }

        public void OnSlidingComplete()
        {
            mDragging = false;
            mAudioSource.time = _Slider.value * mAudioSource.clip.length;
            //Debug.Log($"OnSlidingComplete : {mAudioSource.time}   {_Slider.value} {Time.frameCount}");
            if (_PauseBtn.gameObject.activeSelf)
                mAudioSource.Play();
        }
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();
            _TotalTime.SetActive(false);
            _Slider.value = 0;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AudioController.StopMusic(mAudioSource);
        }

        protected override void Update()
        {
            base.Update();
            if (mIsPlaying && !mDragging && _PauseBtn.gameObject.activeInHierarchy)
            {
                float p = mAudioSource.time / mAudioSource.clip.length;
                _Slider.value = p;
                _CurrentTime.text = TimeUtilities.GetTimerStringShortNoAbbreviations((long)mAudioSource.time);
                //Debug.Log($"Update :{p}  {mAudioSource.time} {Time.frameCount}");
                if (p > 1f)
                {
                    //Hide();
                    mIsPlaying = false;
                }
            }
        }

        private void OnAudioClipLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                AudioClip audioClip = (AudioClip)inObject;
                if (audioClip.length > 0f)
                {
                    mAudioSource = AudioController.PlayMusic(audioClip);
                    _DownloadProgressBar.SetActive(false);
                    _PlayBtn.SetActive(false);
                    _Slider.SetActive(true);
                    _TotalTime.text = TimeUtilities.GetTimerStringShortNoAbbreviations((long)mAudioSource.clip.length);
                    mIsPlaying = true;
                }
                else
                    OnError();
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                OnError();
            }
            else if (inEvent == ResourceEvent.PROGRESS)
            {
                (float progress, ulong downloadedBytes) data = ((float, ulong))inObject;
                //Resource resource = ResourceManager.GetLoadingResource(mUrl);
                //resource.mAudioClipHandler.streamAudio = true;
                _DownloadProgressBar.SetProgress(data.progress);
                //Debug.Log($"{inURL} : {data.progress}");
                //if (data.downloadedBytes > 10)
                //    AudioController.PlayMusic(resource.mAudioClipHandler.audioClip);
            }

            void OnError()
            {
                UiDialogBox dialogBox = UiDialogBox.ShowKey("audio_load_error", "error", DialogBoxType.OKAY);
                dialogBox.OnOkay += Hide;
            }
        }

        #endregion //Private Methods

        #region Public Methods
        public void Init(string url)
        {
            _Slider.SetActive(false);
            _DownloadProgressBar.SetProgress(0);
            ResourceManager.LoadURL(url, OnAudioClipLoaded, ResourceManager.ResourceType.AudioClip);
        }
        #endregion //Public Methods
    }
}
