using System;
using TMPro;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.UI
{
    public class UiInputBox : UiBase
    {
        #region Properties
        [SerializeField] protected TextMeshProUGUI _Message = null;
        [SerializeField] protected TextMeshProUGUI _Header = null;
        [SerializeField] protected UiInputItem _InputField = null;
        public UiInputItem pInputField => _InputField;
        #endregion Properties

        #region Public Properties
        public System.Action<string> OnCTA = null;
        public System.Action OnClose = null;
        #endregion Public Properties


        protected Func<string, bool> ValidateFunction = null;

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();
            GetReferences();
            // _InputField.OnEndEdit.AddListener(OnEndEdit);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // _InputField.OnEndEdit.RemoveListener(OnEndEdit);
        }

        /// <summary>
        /// Context menu that works oly in Unity to help get refernces of components attached. Ex: Image, RawImage, Button & Text
        /// </summary>
        [ContextMenu("Get References")]
        private void GetReferences()
        {
            if (_InputField == null)
                _InputField = GetComponentInChildren<UiInputItem>();
        }

        // private void OnEndEdit(UiInputItem inputItem)
        // {
        //     if (ValidateFunction != null)
        //         _InputField.OnValidation(ValidateFunction(inputItem.text));
        // }

        protected static UiInputBox Create()
        {
            string prefabName = "PfUiInputBox";
            GameObject go = ResourceManager.LoadFromResources(prefabName) as GameObject;
            if (go != null)
                return GameObject.Instantiate(go).GetComponent<UiInputBox>();
            else
                Debug.LogError($"Could not find : {prefabName} under resources.");
            return null;
        }
        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// Show an input field
        /// </summary>
        /// <param name="message">message to be shown to the user</param>
        /// <param name="title">Title of the dialog box</param>
        /// <param name="ctaText">Text of CTA on dialog box</param>
        /// <param name="errorText">Error to be shown in case of invalid input</param>
        /// <param name="defaultText">default text to be shown in input field. If this text is provided, placeHoderText will be ignored</param>
        /// <param name="placeHolderText">Place holder text to be shown. if defaultText is provided this text will be ignored</param>
        /// <returns></returns>
        public static UiInputBox Show(string message, string title, string ctaText, string errorText, string defaultText = null, string placeHolderText = null)
        {
            UiInputBox inputBox = Create();
            if (inputBox._Message != null)
                inputBox._Message.text = message;

            if (inputBox._Header != null)
                inputBox._Header.text = title;

            inputBox._InputField._LinkedButton.text = ctaText;
            if (!defaultText.IsNullOrEmpty())
                inputBox._InputField.text = defaultText;
            else if (!placeHolderText.IsNullOrEmpty())
                inputBox._InputField.placeholderText = placeHolderText;
            if (!errorText.IsNullOrEmpty())
                inputBox._InputField._ErrorText.text = errorText;

            return inputBox;
        }

        public void SetValidateFunction(Func<string, bool> func)
        {
            ValidateFunction = func;
        }

        #endregion Public Methods

        #region UI Callbacks
        public virtual void OnClickCTAButton()
        {
            if (ValidateFunction != null)
            {
                bool isValid = ValidateFunction(_InputField.text);
                _InputField.OnValidation(isValid);
                if (isValid)
                    SendEvent();
            }
            else
            {
                SendEvent();
            }

            void SendEvent()
            {
                OnCTA?.Invoke(_InputField.text);
                Hide();
            }
        }

        public virtual void OnClickClose()
        {
            OnClose?.Invoke();
            base.Hide();
        }
        #endregion UI Callbacks

        #region Editor Only Code
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity in IDE only to validate values
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            GetReferences();
        }
#endif //UNITY_EDITOR
        #endregion Editor Only Code
    }
}
