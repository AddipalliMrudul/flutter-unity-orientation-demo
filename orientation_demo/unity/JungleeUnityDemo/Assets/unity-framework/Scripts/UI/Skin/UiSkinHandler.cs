using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [ExecuteInEditMode]
    public class UiSkinHandler : MonoBehaviour
    {
        public string _SkinName = "Default";

        [Header("Override Skin settings")]
        public bool _OverrideFont = false;
        public bool _OverrideFontColor = false;
        public bool _OverrideFontSize = false;
        public bool _OverrideFontStyle = false;

        private void Awake()
        {
            SetSkin(_SkinName);
        }

        private void SetSkin(string skinName)
        {
            if (string.IsNullOrEmpty(_SkinName))
                return;
            UiSkinData.Skin skin = UiSkinData.GetSkin(_SkinName);
            if (skin == null)
                return;
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = skin._ButtonSprite;
                image.color = skin._ImageColor;
                if (image.enabled)
                {
                    //HACK:For reason, after changing color, it does not reflect unless we disable & enable object again. 
                    image.enabled = false;
                    image.enabled = true;
                }
            }

            Button button = GetComponent<Button>();
            if (button != null)
            {
                //If it is a button, set Image color to white, else highlist color will look odd.
                image.color = Color.white;
                if (button.transition == Selectable.Transition.ColorTint)
                    button.colors = skin._ColorBlock;
                else if (button.transition == Selectable.Transition.SpriteSwap)
                {
                    button.targetGraphic = image;
                    button.spriteState = skin._SpriteState;
                }
            }

            TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                if (!_OverrideFontColor)
                    text.color = skin._TextColor;
                if (!_OverrideFont && skin._Font != null)
                    text.font = skin._Font;
                if (!_OverrideFontStyle)
                    text.fontStyle = skin._FontStyle;
                if (!_OverrideFontSize)
                    text.fontSize = skin._FontSize;
            }
        }

        #region Editor Only methods

        public void SetSkin()
        {
            SetSkin(_SkinName);
        }

        [ContextMenu("Apply")]
        private void ApplySkin()
        {
            SetSkin(_SkinName);
        }

        #endregion Editor Only methods
    }
}