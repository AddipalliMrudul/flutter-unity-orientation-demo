using XcelerateGames.UI;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Keyboard
{
    public class UiJGInputFieldItem : UiItem
    {
        [Header("Custom JG Field")]
        public KeyboardType _keyboardType;
        public TextAnchor _keyboardpos;
        public bool UseTextMeshProInputField = false;

        #region Signals
        [InjectSignal] private SigHideKeyboard mSigHideKeyboard = null;
        [InjectSignal] private SigShowKeyboard mSigShowKeyboard = null;
        #endregion //Signals

        private JGInputField mJGInputField = null;

        protected override void Start()
        {
            base.Start();
            mJGInputField = transform.GetComponent<JGInputField>();
            mJGInputField.pSigShowKeyboard = mSigShowKeyboard;
            mJGInputField.pSigHideKeyboard = mSigHideKeyboard;
        }
    }
}
