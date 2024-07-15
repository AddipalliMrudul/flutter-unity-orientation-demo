using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XcelerateGames.UI
{
    [System.Serializable]
    public class OnEndEditEvent : UnityEvent<UiInputItem> { }

    [RequireComponent(typeof(TMP_InputField))]
    public class UiInputItem : MonoBehaviour
    {
        public bool _IsRequiredField = true;
        //If set, this button will be set to interactive if the text is non empty else it will be set to non-interactive
        public UiItem _LinkedButton;
        [Tooltip("Minimum required text length, -1 is no restriction")]
        public int _MinLength = -1;
        public UiItem _ErrorText;

        public OnEndEditEvent OnEndEdit = null;

        private TMP_InputField mInputField;
        public TMP_InputField InputField
        {
            get
            {
                if (mInputField == null)
                    mInputField = GetComponent<TMP_InputField>();
                return mInputField;
            }
        }

        public bool IsNullOrEmpty => string.IsNullOrEmpty(text);

        public string text { get { return InputField.text; } set { InputField.text = value; OnTextChanged(value); } }
        public string placeholderText { get { return InputField.placeholder.GetComponent<TextMeshProUGUI>().text; } set { InputField.placeholder.GetComponent<TextMeshProUGUI>().text = value; OnTextChanged(value); } }

        private void Awake()
        {
            GetReferences();
            InputField.onValueChanged.AddListener(OnTextChanged);
            InputField.onEndEdit.AddListener(EndEditCallback);
            InputField.onSelect.AddListener(OnSelect);
            OnTextChanged(InputField.text);
            if (_ErrorText != null)
                _ErrorText.SetActive(false);
        }

        private void OnSelect(string arg0)
        {
            if (_ErrorText != null)
            {
                if (_ErrorText.isActiveAndEnabled)
                    _ErrorText.PlayAnim("Out");
            }
        }

        private void EndEditCallback(string arg0)
        {
            //Debug.Log(">" + EventSystem.current.currentSelectedGameObject.name);
            if (!text.IsNullOrEmpty())
                OnEndEdit?.Invoke(this);
        }

        private void OnDestroy()
        {
            InputField.onValueChanged.RemoveListener(OnTextChanged);
            InputField.onEndEdit.RemoveListener(EndEditCallback);
            InputField.onSelect.RemoveListener(OnSelect);
        }

        private void OnTextChanged(string inText)
        {
            _LinkedButton?.SetInteractive(!inText.IsNullOrEmpty());
        }

        [ContextMenu("Get References")]
        private void GetReferences()
        {
        }

        private void OnValidate()
        {
            GetReferences();
        }

        public bool IsValid()
        {
            if (_IsRequiredField)
            {
                bool hasValidData = !string.IsNullOrEmpty(text);
                if (hasValidData)
                {
                    if (InputField.contentType == TMP_InputField.ContentType.EmailAddress)
                        hasValidData = text.IsEmail();
                    else if (text.Length < _MinLength)
                        hasValidData = false;
                }
                OnValidation(hasValidData);
                return hasValidData;
            }
            return true;
        }

        public void OnValidation(bool hasValidData)
        {
            if (_ErrorText != null)
            {
                _ErrorText.SetActive(!hasValidData);
                _ErrorText.PlayAnim(hasValidData ? "Out" : "In");
            }
        }

        public void SetSelected()
        {
            StartCoroutine(WaitAndSet());
        }

        public void SetSelectedIfEmpty()
        {
            //Debug.Log($"SetSelectedIfEmpty {name}");
            if (text.IsNullOrEmpty())
            {
                StartCoroutine(WaitAndSet());
            }
        }

        IEnumerator WaitAndSet()
        {
            yield return new WaitForEndOfFrame();
            mInputField.ActivateInputField();
            mInputField.Select();
        }
    }
}
