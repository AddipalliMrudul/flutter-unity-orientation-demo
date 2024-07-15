using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    [RequireComponent(typeof(UiJGInputFieldItem))]
    public class JGInputField : TMP_InputField
    {
        #region Signals
        public SigShowKeyboard pSigShowKeyboard = null;
        public SigHideKeyboard pSigHideKeyboard = null;
        #endregion //Signals

        #region private variables
        private KeyboardType mkeyboardType = KeyboardType.Standard;
        private TextAnchor mkeyboardpos = TextAnchor.LowerCenter;
        private bool UseTMPInputField = false;
        //private BaseEventData mBaseEventData = null;
        #endregion //private variables

        #region Public Variables
        public UnityEvent OnJGInputFieldOverlapped = null;
        public UnityEvent OnFocusRemove = null;

        #endregion //Public Variables

        protected override void Start()
        {
            base.Start();
            mkeyboardType = transform.GetComponent<UiJGInputFieldItem>()._keyboardType;
            mkeyboardpos = transform.GetComponent<UiJGInputFieldItem>()._keyboardpos;
            UseTMPInputField = transform.GetComponent<UiJGInputFieldItem>().UseTextMeshProInputField;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (!UseTMPInputField)
            {
                this.shouldHideSoftKeyboard = true;
                if (!JGKeyboard._IsKeyboardOpen || JGKeyboard.mlastSelected != this)
                {
                    //   JGKeyboard.mlastSelected = this;
                    pSigShowKeyboard.Dispatch(this, mkeyboardType, mkeyboardpos);
                }

            }
            else
            {
                this.shouldHideSoftKeyboard = false;
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            //mBaseEventData = eventData;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (JGKeyboard.mlastSelected == this)
                pSigHideKeyboard.Dispatch();
        }

        public void CheckifOverlap()
        {
            if (JGKeyboard.mlastSelected == this)
            {
                Debug.Log("check overlapping");
                RectTransform thisrect = transform.GetComponent<RectTransform>();
                bool isOverlapping = thisrect.Overlaps(thisrect, JGKeyboard.KeyboardRect);
                if (isOverlapping && OnJGInputFieldOverlapped != null)
                    OnJGInputFieldOverlapped?.Invoke();
            }
        }

        public void OnFocusRemoveFromthisField()
        {
            if (OnFocusRemove != null)
                OnFocusRemove?.Invoke();

            //if (mBaseEventData != null)
            //    base.OnDeselect(mBaseEventData);
        }

        public void OnFocusthisField()
        {
            Invoke("CheckifOverlap", .5f);
        }
    }
}
