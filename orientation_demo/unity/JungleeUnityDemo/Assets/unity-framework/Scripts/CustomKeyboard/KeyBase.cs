using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public class KeyBase : UiItem
    {
        [HideInInspector] public bool mCapitalized;
        [HideInInspector] public Type mKeyType;
        [HideInInspector] public bool mIsPressed=false;
        [HideInInspector] public string mChildText=null;
        [SerializeField] public bool _ProcessEventOnHoldDown = false;

        #region Signals
        [InjectSignal] private SigOnShiftKeyPress mSigOnShiftKeyPress = null;
        [InjectSignal] private SigOnShowKeyboard mSigOnShowKeyboard = null;
        [InjectSignal] private SigOnKeyPress mSigOnKeyPress = null;
        #endregion //Signals

        protected override void Start()
        {
            base.Start();
            Reset();
            mSigOnShowKeyboard.AddListener(Reset);
            mSigOnShiftKeyPress.AddListener(ToggleShiftEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigOnShowKeyboard.RemoveListener(Reset);
            mSigOnShiftKeyPress.RemoveListener(ToggleShiftEvent);
        }

        public override void OnClicked()
        {
            base.OnClicked();
            ReactivateInputField(JGKeyboard.mlastSelected);
            Invoke("AfterKeyPress", .01f);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!mIsPressed)
            {
                mIsPressed = true;
                ProcessKeypressEvent();
                if (_ProcessEventOnHoldDown)
                {
                    StartCoroutine(ProcessEventOnHoldDown());
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (mIsPressed)
                mIsPressed = false;
        }

        public virtual void ToggleShiftEvent(bool shiftOn)
        {
            mCapitalized = shiftOn;
        }

        public virtual void Reset()
        {
            mCapitalized = true;
        }

        protected virtual void ProcessKeypressEvent()
        {
            //Add or Modify the keypress events to be done in the child classes
        }

        void ReactivateInputField(JGInputField inputField)
        {
            if (inputField != null)
            {
                StartCoroutine(ActivateInputFieldWithoutSelection(inputField));
            }
        }

        void AfterKeyPress()
        {
            mSigOnKeyPress.Dispatch();
        }

        IEnumerator ActivateInputFieldWithoutSelection(JGInputField inputField)
        {
            inputField.ActivateInputField();
            // wait for the activation to occur in a lateupdate
            yield return new WaitForEndOfFrame();
            // make sure we're still the active ui
            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject)
            {
                // To remove hilight we'll just show the caret at the end of the line
                inputField.MoveTextEnd(false);

            }
        }

        IEnumerator ProcessEventOnHoldDown()
        {
            yield return new WaitForSeconds(0.5f);
            while (mIsPressed)
            {
                yield return new WaitForSeconds(0.1f);
                ProcessKeypressEvent();
            }
        }
    }
}
