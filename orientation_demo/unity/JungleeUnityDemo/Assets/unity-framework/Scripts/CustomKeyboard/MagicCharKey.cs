using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace XcelerateGames.Keyboard
{
    public class MagicCharKey : KeyBase
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
            Event keyPress = null;
            keyPress = Event.KeyboardEvent("a");
            switch (mChildText)
            {
                case "&":
                    keyPress.keyCode = KeyCode.Ampersand;
                    keyPress.character = mChildText[0];
                    break;
                case "^":
                    keyPress.keyCode = KeyCode.Caret;
                    keyPress.character = mChildText[0];
                    break;
                case "%":
                    keyPress.keyCode = KeyCode.Percent;
                    keyPress.character = mChildText[0];
                    break;
                case "#":
                    keyPress.keyCode = KeyCode.Hash;
                    keyPress.character = mChildText[0];
                    break;
                default:
                    if (mChildText.Length != 1)
                    {
                        Debug.LogError("Ignoring spurious multi-character key value: " + mChildText);
                        return;
                    }
                    keyPress = Event.KeyboardEvent(mChildText);
                    if (mCapitalized) keyPress.character = char.ToUpper(keyPress.character);
                    break;
            }
            input.ProcessEvent(keyPress);
            input.ForceLabelUpdate();
        }
    }
}

