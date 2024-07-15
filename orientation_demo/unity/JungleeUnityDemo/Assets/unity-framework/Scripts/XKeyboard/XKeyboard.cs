using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using System.Collections;

namespace XcelerateGames.Keyboard
{
    [DefaultExecutionOrder(-100)]
    public class XKeyboard : MonoBehaviour
    {
        #region Constants        
        public enum KeyboardOrientation
        {
            Auto = 0,
            Portrait = 1,
            Landscape = 2,
            LandscapeLeft = 3,
            LandscapeRight = 4,
        }
        #endregion//============================================================[ Constants ]

        #region Data    
        //Public
        public static XKeyboard instace;
        public bool isKeyboardVisible;
        public float animationTime;
        public RectTransform panel;
        public CanvasGroup canvasGroup;
        public XKeyboardInputField xKeyboardInputField;
        public XKeyboardMap keyboardMap;
        public KeyboardOrientation keyboardOrientation = KeyboardOrientation.Auto;
        //Private        
        #endregion//============================================================[ Data ]

        #region Events
        public class KeyPressed : UnityEvent<XKeyboardKey> { }
        public class BroadcastKeyPressed : UnityEvent<XKeyboardMap.KeyMap> { }
        public BroadcastKeyPressed broadcastKeyPressedEvent = new BroadcastKeyPressed();
        public KeyPressed keyPressedEvent = new KeyPressed();
        #endregion//============================================================[ Events ]

        #region Unity        
        private void Awake()
        {
            instace = this;
        }
        private void OnEnable()
        {
            keyPressedEvent?.AddListener(OnKeyPressed);
        }
        private void OnDisable()
        {
            keyPressedEvent?.RemoveAllListeners();
        }
        #endregion//============================================================[ Unity ]

        #region Listeners
        public void OnKeyPressed(XKeyboardKey xKeyboardKey)
        {
            broadcastKeyPressedEvent?.Invoke(xKeyboardKey.keyMap);
            if (xKeyboardInputField != null)
            {
                ConvertKeyboardEventToText(xKeyboardKey);
            }
            if (xKeyboardKey.keyMap.keyCode == KeyCode.Numlock)
            {
                if (panel.transform.GetChild(1).gameObject.activeSelf)
                {
                    ToggleKeyboardSections(0);
                }
                else
                {
                    ToggleKeyboardSections(1);
                }
            }
            if (xKeyboardKey.capsLock && xKeyboardKey.keyMap.keyCode != KeyCode.CapsLock)
            {
                broadcastKeyPressedEvent?.Invoke(new XKeyboardMap.KeyMap { keyCode = KeyCode.CapsLock });
            }
        }
        #endregion//============================================================[ Listeners ]

        #region Public
        public void ShowKeyboard(XKeyboardInputField xKeyboardInputField)
        {
            if (this.xKeyboardInputField != null && this.xKeyboardInputField.GetInstanceID() != xKeyboardInputField.GetInstanceID())
            {
                this.xKeyboardInputField.currentInputField.ReleaseSelection();
                this.xKeyboardInputField.currentInputField.HideCaret();
            }
            this.xKeyboardInputField = xKeyboardInputField;
            gameObject.SetActive(true);
            StartCoroutine(SetupKeyboardOrientation());
            isKeyboardVisible = true;
        }
        public void HideKeyboard()
        {
            if (xKeyboardInputField != null)
            {
                xKeyboardInputField.currentInputField.ReleaseSelection();
                xKeyboardInputField.currentInputField.HideCaret();
            }
            isKeyboardVisible = false;
            canvasGroup.DOFade(0, animationTime);
            xKeyboardInputField = null;
            panel.DOAnchorPosY(-panel.rect.height, animationTime).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                panel.offsetMax = new Vector2(0, panel.offsetMax.y);
                panel.offsetMin = new Vector2(0, panel.offsetMin.y);
                gameObject.SetActive(false);
            });
        }
        public void ToggleKeyboardSections(int sectionIndex)
        {
            for (int i = 0; i < panel.transform.childCount; i++)
            {
                if (i == sectionIndex)
                {
                    panel.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    panel.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        public IEnumerator SetupKeyboardOrientation()
        {
            if (!isKeyboardVisible)
            {
                if (Screen.orientation != ScreenOrientation.Portrait || Application.isEditor)
                {
                    yield return new WaitForEndOfFrame();
                    if (keyboardOrientation == KeyboardOrientation.LandscapeLeft)
                    {
                        panel.offsetMax = new Vector2(-panel.rect.width / 2, panel.offsetMax.y);
                    }
                    else if (keyboardOrientation == KeyboardOrientation.LandscapeRight)
                    {
                        panel.offsetMin = new Vector2(panel.rect.width / 2, panel.offsetMin.y);
                    }
                }
            }
            canvasGroup.DOFade(1, animationTime);
            panel.DOAnchorPosY(0, animationTime).SetEase(Ease.InOutSine);
        }
        #endregion//============================================================[ Public ]

        #region Private
        private void ConvertKeyboardEventToText(XKeyboardKey xKeyboardKey)
        {
            Event keyboardInputEvent = new Event();
            if (xKeyboardKey.keyMap.keyCode == KeyCode.CapsLock
                || xKeyboardKey.keyMap.keyCode == KeyCode.Numlock
            ) { }
            else if (xKeyboardKey.keyMap.keyCode == KeyCode.Backspace)
            {
                keyboardInputEvent = new Event
                {
                    modifiers = EventModifiers.FunctionKey,
                    keyCode = xKeyboardKey.keyMap.keyCode,
                    type = EventType.KeyDown,
                };

            }
            else if (xKeyboardKey.keyMap.keyCode == KeyCode.Return)
            {
                keyboardInputEvent = new Event
                {
                    modifiers = EventModifiers.FunctionKey,
                    character = '\n',
                    keyCode = xKeyboardKey.keyMap.keyCode,
                    type = EventType.KeyDown,
                };
            }
            else
            {
                char textChar = char.Parse(xKeyboardKey.capsLock ? xKeyboardKey.keyMap.keyCharacter.ToUpper() : xKeyboardKey.keyMap.keyCharacter.ToLower());
                keyboardInputEvent = new Event
                {
                    modifiers = EventModifiers.None,
                    character = textChar,
                    keyCode = xKeyboardKey.keyMap.keyCode,
                    type = EventType.KeyDown,
                };
            }
            xKeyboardInputField.currentInputField.ProcessEvent(keyboardInputEvent);
            xKeyboardInputField.currentInputField.ForceLabelUpdate();
            xKeyboardInputField.currentInputField.ActivateInputField();
            if ((xKeyboardInputField.currentInputField.lineType == XInputField.LineType.SingleLine ||
                xKeyboardInputField.currentInputField.lineType == XInputField.LineType.MultiLineSubmit)
                && xKeyboardKey.keyMap.keyCode == KeyCode.Return)
            {
                HideKeyboard();
            }
        }
        #endregion//============================================================[ Private ]
    }
}