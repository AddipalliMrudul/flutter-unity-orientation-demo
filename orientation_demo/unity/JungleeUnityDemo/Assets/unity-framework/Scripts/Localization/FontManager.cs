using UnityEngine;
using System;
using TMPro;

namespace XcelerateGames.UI
{
    [Serializable]
    public class FontAsset
    {
        public string _Name = null;
        public Language _Language = Language.None;
        public TMP_FontAsset _Font = null;
    }

    [CreateAssetMenu(fileName = "FontManager", menuName = Utilities.MenuName + "FontManager")]
    public class FontManager : ScriptableObject
    {
        public FontAsset[] _Fonts = null;

        private static FontManager mInstance = null;

        private static FontManager Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = Resources.Load<FontManager>("FontManager");
                if (mInstance == null)
                    XDebug.LogException("Could not find FontManager asset under Resources folder");
                return mInstance;
            }
        }

        public static TMP_FontAsset GetFont(Language language)
        {
            if (Instance == null || Instance._Fonts == null)
                return null;
            FontAsset fontAsset = Array.Find(Instance._Fonts, e => e._Language.Equals(language));
            if (fontAsset != null)
                return fontAsset._Font;
            XDebug.LogException($"Could not find {language} in FontManager asset");
            return null;
        }
    }
}
