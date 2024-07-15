using UnityEngine;
using XcelerateGames;
using XcelerateGames.Audio;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace JungleeGames
{
    public class UiSettings : UiBase
    {
        #region Properties/Signals/Models
        [SerializeField] protected UiToggleItem _BtnSound;
        [SerializeField] protected UiToggleItem _BtnVibrate;
        [SerializeField] protected UiToggleItem _BtnMusic;
        #endregion Properties

        #region Signals/Models
        [InjectSignal] private SigShowSettings mSigShowSettings = null;
        [InjectSignal] private SigSettingsHidden mSigSettingsHidden = null;
        #endregion Signals/Models

        protected override void Awake()
        {
            base.Awake();
            _BtnSound._OnValueChange.AddListener(OnSoundToggle);
            _BtnMusic._OnValueChange.AddListener(OnMusicToggle);
            _BtnVibrate._OnValueChange.AddListener(OnVibrationToggle);
            mSigShowSettings.AddListener(Show);
            gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            mSigShowSettings.RemoveListener(Show);
            _BtnSound._OnValueChange.RemoveListener(OnSoundToggle);
            _BtnMusic._OnValueChange.RemoveListener(OnMusicToggle);
            _BtnVibrate._OnValueChange.RemoveListener(OnVibrationToggle);
            base.OnDestroy();
        }

        protected virtual void OnSoundToggle(bool isOn)
        {
            AudioController.SoundMute(!isOn);
        }

        protected virtual void OnVibrationToggle(bool isOn)
        {
            Vibration.VibrationState = isOn;
        }

        protected virtual void OnMusicToggle(bool isOn)
        {
            AudioController.MusicMute(!isOn);
        }

        public override void Hide()
        {
            base.Hide();
            mSigSettingsHidden.Dispatch();
        }

        #region Public Methods
        public virtual void OnReportAProblemBtnClicked()
        {

        }

        public override void Show()
        {
            base.Show();
            _BtnSound.isOn = !AudioController.SoundMuted;
            _BtnMusic.isOn = !AudioController.MusicMuted;
            _BtnVibrate.isOn = Vibration.VibrationState;
        }
        #endregion Public Methods
    }
}
