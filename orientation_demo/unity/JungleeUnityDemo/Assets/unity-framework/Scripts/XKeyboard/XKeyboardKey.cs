using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public class XKeyboardKey : UiItem
    {
        #region Data    
        //Public      
        public XKeyboardMap.KeyMap keyMap;
        public TMP_Text keyText;
        public GameObject keyObject;
        public bool capsLock;
        public Button keyButton;
        public int keyMapIndex;
        [SerializeField] public bool _ProcessEventOnHoldDown = false;
        private bool mIsPressed = false;

        #endregion//============================================================[ Data ]        

        #region Unity        
        protected override void OnEnable()
        {
            base.OnEnable();
            mIsPressed = false;
            if (XKeyboard.instace != null)
            {
                XKeyboard.instace.broadcastKeyPressedEvent?.AddListener(OnBroadcastKeyMap);
            }            
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (XKeyboard.instace != null)
            {
                XKeyboard.instace.broadcastKeyPressedEvent?.RemoveListener(OnBroadcastKeyMap);                
            }            
        }
        #endregion//============================================================[ Unity ]

        #region Listeners
        public void OnBroadcastKeyMap(XKeyboardMap.KeyMap keyMap)
        {            
            if (keyMap.keyCode == KeyCode.CapsLock)
            {
                capsLock = !capsLock;
            }           
            UpdateKey();
        }
        #endregion//============================================================[ Listeners ]

        #region Public
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!mIsPressed)
            {
                mIsPressed = true;
                OnKeyPressed();
                if (_ProcessEventOnHoldDown)
                {
                    StartCoroutine(ProcessEventOnHoldDown());
                }
            }
        }

        IEnumerator ProcessEventOnHoldDown()
        {
            yield return new WaitForSeconds(0.5f);
            while (mIsPressed)
            {
                yield return new WaitForSeconds(0.1f);
                OnKeyPressed();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (mIsPressed)
                mIsPressed = false;
        }

        public void OnKeyPressed()
        {
            if (XKeyboard.instace != null)
            {
                XKeyboard.instace.keyPressedEvent?.Invoke(this);
            }
        }
        public void UpdateKey()
        {
            if (keyText != null)
            {
                if (capsLock)
                {
                    keyText.text = keyMap.keyCharacter.ToUpper();
                }
                else
                {
                    keyText.text = keyMap.keyCharacter.ToLower();
                }                
            }
            else if (keyObject != null)
            {                
                if (capsLock)
                {
                    keyObject.gameObject.SetActive(true);
                }
                else
                {
                    keyObject.gameObject.SetActive(false);
                }
            }
        }
        #endregion//============================================================[ Public ]        
    }
}
