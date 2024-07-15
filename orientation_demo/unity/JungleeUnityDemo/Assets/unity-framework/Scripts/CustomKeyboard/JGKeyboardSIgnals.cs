using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.Keyboard
{

    //keyboard related signals
    public class SigShowKeyboard : Signal<JGInputField, KeyboardType, TextAnchor> { };
    public class SigHideKeyboard : Signal { };
    public class SigOnShiftKeyPress : Signal<bool> { };
    public class SigOnNumKeyPress : Signal<bool> { };
    public class SigOnSpecialKeyPress : Signal<bool> { };
    public class SigOnShowKeyboard : Signal { };
    public class SigOnKeyPress : Signal { };
}
