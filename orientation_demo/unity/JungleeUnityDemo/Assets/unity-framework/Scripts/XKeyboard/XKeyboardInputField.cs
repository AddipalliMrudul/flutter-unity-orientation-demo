using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using XcelerateGames.IOC;

namespace XcelerateGames.Keyboard
{
    [RequireComponent(typeof(XInputField))]
    public class XKeyboardInputField : BaseBehaviour, IPointerDownHandler
    {
        #region Data    
        //Public
        public string assetBundlePath = "xkeyboard/XKeybaord";
        public XInputField currentInputField;
        [InjectSignal] private SigLoadAssetFromBundle mSigLoadAssetFromBundle = null;
        //Private
        private bool keyboardLoadInitiated = false;
        #endregion//============================================================[ Data ]

        #region Unity
        protected override void Awake()
        {
            base.Awake();
            SetupInputField();
        }

        private void OnDisable()
        {
            if (XKeyboard.instace != null)
            {
                XKeyboard.instace.HideKeyboard();
            }
        }

        private void Reset()
        {
            SetupInputField();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (XKeyboard.instace != null)
            {
                XKeyboard.instace.ShowKeyboard(this);
            }
            else
            {
                if (!keyboardLoadInitiated)
                {
                    keyboardLoadInitiated = true;
                    mSigLoadAssetFromBundle.Dispatch(assetBundlePath, null, false, 0, OnResultScreenLoaded);
                }
            }
        }
        #endregion//============================================================[ Unity ]

        #region Private
        private void SetupInputField()
        {
            if (currentInputField == null)
            {
                currentInputField = GetComponent<XInputField>();
            }
            currentInputField.isRichTextEditingAllowed = true;
            currentInputField.richText = true;
            currentInputField.shouldHideSoftKeyboard = true;
            currentInputField.shouldHideMobileInput = true;
            currentInputField.onFocusSelectAll = false;
            currentInputField.resetOnDeActivation = false;
            currentInputField.restoreOriginalTextOnEscape = false;
            currentInputField.readOnly = false;
        }
        private void OnResultScreenLoaded(GameObject keyboard)
        {
            keyboard.GetComponent<XKeyboard>().ShowKeyboard(this);
        }
        #endregion//============================================================[ Private ]
    }
}
