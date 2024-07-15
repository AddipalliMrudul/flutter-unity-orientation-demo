using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [CreateAssetMenu(fileName = "UiSkinData", menuName = Utilities.MenuName + "Create Skin Data")]
    public class UiSkinData : ScriptableObject
    {
        [Serializable]
        public class Skin
        {
            //Name of skin, must be unique.
            public string _Name = "Default";

            [Space(10)]
            [Header("Sprites")]
            public Sprite _ButtonSprite;
            public SpriteState _SpriteState;

            [Space(10)]
            [Header("Image Color")]
            public Color _ImageColor;

            [Space(10)]
            [Header("Color Tint")]
            public ColorBlock _ColorBlock;

            [Space(10)]
            [Header("Text Properties")]
            public Color _TextColor;
            public TMP_FontAsset _Font;
            public FontStyles _FontStyle;
            public int _FontSize = 14;
        }

        public Skin[] _Skins;

        private static UiSkinData mInstance;

        public static UiSkinData pInstance
        {
            get
            {
                if (mInstance == null)
                    mInstance = Resources.Load("UiSkinData") as UiSkinData;

                return mInstance;
            }
        }

        #region Static Methods

        public static Skin GetSkin(string skinName)
        {
            Skin skin = Array.Find(pInstance._Skins, e => e._Name.Equals(skinName));
            if (skin == null)
                Debug.LogError("Could not find skin " + skinName);
            return skin;
        }

        #endregion Static Methods
    }
}