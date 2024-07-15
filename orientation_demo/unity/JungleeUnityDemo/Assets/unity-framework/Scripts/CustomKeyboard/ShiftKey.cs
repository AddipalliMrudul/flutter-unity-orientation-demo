using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public class ShiftKey : UiItem
    {
        #region Private Variables
        // private bool shiftOn;
        private bool mIsNumOn;
        private bool mIsSpecialCharOn;
        //private static GameObject LASTPOINTERDOWNOBJECT = null;//Need to record the last click event's object
        private int mclickcount = 1;
        private float mHoldbuttonTime;
        #endregion //Private Variables

        #region Const Variables
        //const float doubleClickTime = 0.5f;
        const float longPressTime = .5f;
        #endregion //Const Variables

        #region Signals
        [InjectSignal] private SigOnShiftKeyPress mSigOnShiftKeyPress = null;
        [InjectSignal] private SigOnShowKeyboard mSigOnShowKeyboard = null;
        [InjectSignal] private SigOnSpecialKeyPress mSigOnSpecialKeyPress = null;
        [InjectSignal] private SigOnNumKeyPress mSigOnNumKeyPress = null;
        [InjectSignal] private SigOnKeyPress mSigOnKeyPress = null;
        #endregion //Signals

        #region Properties
        [SerializeField] private Sprite[] mShiftSprites = null;
        [SerializeField] private TMP_Text mText = null;
        [SerializeField] private Image mImage = null;
        #endregion //Properties

        #region Protected func
        protected override void Start()
        {
            base.Start();
            Reset();
            mSigOnShowKeyboard.AddListener(Reset);
            mSigOnNumKeyPress.AddListener(OnNumKeyPress);
            mSigOnKeyPress.AddListener(OnKeyPress);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigOnShowKeyboard.RemoveListener(Reset);
            mSigOnNumKeyPress.RemoveListener(OnNumKeyPress);
        }
        #endregion //Protected func

        #region Public func
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            mHoldbuttonTime = Time.unscaledTime;
            Debug.Log("OnPointerDown");
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            Debug.Log("OnPointerUp");
            float totalPressTime = Time.unscaledTime - mHoldbuttonTime;
            mHoldbuttonTime = 0f;
            if (totalPressTime >= longPressTime && mclickcount != 2)
            {
                mclickcount = 2;
                Debug.Log("long press detected");
            }
            else
            {
                Debug.Log("click");
                if (mclickcount >= 2)
                {
                    mclickcount = 0;
                }
                else
                {
                    mclickcount++;
                }
            }
            OnKeyClick();
        }

        #endregion //Public func

        #region Private func
        private void OnKeyClick()
        {
            if (mIsNumOn)
            {
                mIsSpecialCharOn = !mIsSpecialCharOn;
                mSigOnSpecialKeyPress.Dispatch(mIsSpecialCharOn);
            }
            else
            {
                mSigOnShiftKeyPress.Dispatch(mclickcount != 0);
            }
            UpdateUI();
        }

        private void OnKeyPress()
        {
            if (mclickcount == 1 && !mIsNumOn)
            {
                mclickcount = 0;
                OnKeyClick();
            }
        }

        private void OnNumKeyPress(bool isOn)
        {
            mIsNumOn = isOn;
            UpdateUI();
        }

        private void Reset()
        {
            mIsNumOn = false;
            mIsSpecialCharOn = false;
            mclickcount = 1;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (mIsNumOn)
            {
                mText.enabled = true;
                mImage.enabled = false;
                if (mIsSpecialCharOn)
                {
                    mText.text = "123";
                }
                else
                {
                    mText.text = "#!.";
                }

            }
            else
            {
                mText.enabled = false;
                mImage.enabled = true;
                mImage.sprite = mShiftSprites[mclickcount];
            }
        }
        #endregion //Private func
    }
}
