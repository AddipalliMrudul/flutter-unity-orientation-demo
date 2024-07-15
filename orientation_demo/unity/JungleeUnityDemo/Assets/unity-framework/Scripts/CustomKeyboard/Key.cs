using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace XcelerateGames.Keyboard
{
    public class Key : KeyBase
    {
        protected override void Start()
        {
            base.Start();
            TMP_Text mText = transform.GetComponentInChildren<TMP_Text>();
            mChildText = mText.text;
        }

        protected override void ProcessKeypressEvent()
        {
            JGInputField input = JGKeyboard.mlastSelected;
            Event keyPress = Event.KeyboardEvent(mChildText);
            if (mCapitalized)
                keyPress.character = char.ToUpper(keyPress.character);
            input.ProcessEvent(keyPress);
            input.ForceLabelUpdate();
        }

        public override void ToggleShiftEvent(bool shiftOn)
        {
            base.ToggleShiftEvent(shiftOn);
            SetTextValue();
        }

        public override void Reset()
        {
            base.Reset();
            SetTextValue();
        }

        void SetTextValue()
        {
            TMP_Text mText = transform.GetComponentInChildren<TMP_Text>();
            mChildText = mText.text;
            if (mCapitalized)
            {
                mText.text = mChildText.ToUpper();
            }
            else
            {
                mText.text = mChildText.ToLower();
            }
        }
    }
}
