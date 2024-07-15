using XcelerateGames.IOC;
using XcelerateGames.UI;
using UnityEngine;

namespace XcelerateGames.Keyboard
{
    
    public class NumSpecialCharHolder : UiItem
    {
        #region Signals
        [InjectSignal] private SigOnNumKeyPress mSigOnNumKeyPress = null;
        [InjectSignal] private SigOnShowKeyboard mSigOnShowKeyboard = null;
        [InjectSignal] private SigOnSpecialKeyPress mSigOnSpecialKeyPress = null;
        #endregion //Signals

        [SerializeField] private GameObject AlphabetContainer = null;
        [SerializeField] private GameObject NumContainer = null;
        [SerializeField] private GameObject SpecialCharContainer = null;

        protected override void Start()
        {
            base.Start();
            Reset();
            mSigOnShowKeyboard.AddListener(Reset);
            mSigOnNumKeyPress.AddListener(OnNumKeyPress);
            mSigOnSpecialKeyPress.AddListener(OnSpecialCharKeyPress);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigOnShowKeyboard.RemoveListener(Reset);
            mSigOnNumKeyPress.RemoveListener(OnNumKeyPress);
            mSigOnSpecialKeyPress.RemoveListener(OnSpecialCharKeyPress);
        }

        void OnNumKeyPress(bool isOn)
        {
            AlphabetContainer.SetActive(!isOn);
            NumContainer.SetActive(isOn);
            if (SpecialCharContainer != null)
            {
                SpecialCharContainer.SetActive(false);
            }
        }

        private void Reset()
        {
            OnNumKeyPress(false);
        }

        void OnSpecialCharKeyPress(bool isOn)
        {
            if (SpecialCharContainer != null)
            {
                AlphabetContainer.SetActive(false);
                NumContainer.SetActive(!isOn);
                SpecialCharContainer.SetActive(isOn);
            }
        }
    }
}
