using TMPro;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public class NumKey : UiItem
    {
        #region Signals
        [InjectSignal] private SigOnNumKeyPress mSigOnNumKeyPress = null;
        [InjectSignal] private SigOnShowKeyboard mSigOnShowKeyboard = null;
        #endregion //Signals

        private bool isNumOn = false;

        protected override void Start()
        {
            base.Start();
            Reset();
            mSigOnShowKeyboard.AddListener(Reset);
            GetComponent<Button>().onClick.AddListener(() => Press());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GetComponent<Button>().onClick.RemoveListener(() => Press());
            mSigOnShowKeyboard.RemoveListener(Reset);
        }

        void UpdateText()
        {
            if (isNumOn)
            {
                transform.GetComponentInChildren<TMP_Text>().text = "ABC";
            }
            else
            {
                transform.GetComponentInChildren<TMP_Text>().text = "123";
            }
        }

        void Reset() {
            isNumOn = false;
            UpdateText();
        }

        public void Press()
        {
            isNumOn = !isNumOn;
            mSigOnNumKeyPress.Dispatch(isNumOn);
            UpdateText();
        }

    }
}
