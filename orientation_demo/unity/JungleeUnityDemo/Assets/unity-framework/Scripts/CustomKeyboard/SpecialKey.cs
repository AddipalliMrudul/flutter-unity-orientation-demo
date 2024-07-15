using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XcelerateGames.Keyboard
{
    public class SpecialKey : KeyBase
    {
        protected override void ProcessKeypressEvent()
        {
            JGInputField input = JGKeyboard.mlastSelected;
            Event keyPress = Event.KeyboardEvent(transform.name);
            input.ProcessEvent(keyPress);
            input.ForceLabelUpdate();
        }
    }
}
