using System;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Keyboard {
    [CreateAssetMenu(fileName = "KeyboardMap", menuName = "XcelerateGames/XKeyboard/KeyMap", order = 0)]
    public class XKeyboardMap : ScriptableObject
    {
        #region KeyMapData        
        public string languageCode;
        public List<KeyMap> keyMap = new List<KeyMap>();
        [Serializable]
        public class KeyMap
        {
            public KeyCode keyCode;
            public string keyCharacter;
        }
        #endregion//============================================================[ KeyMapData ]
    }
}
