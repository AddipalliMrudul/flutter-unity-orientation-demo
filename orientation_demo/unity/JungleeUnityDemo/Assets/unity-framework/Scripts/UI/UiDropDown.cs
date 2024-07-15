using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*
* Wrapper class for TMP_Dropdown
*/

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class UiDropDown : MonoBehaviour
    {
        public string _DefaultText = null;
        public Sprite _DefaultSprite = null;
        //If set to true, drop down height will adjusted to fit children size
        public bool _FitDropdownToContents = false;

        public TMP_Dropdown _DropDown = null;

        public DropDownMenuEvent OnSelectionChange = null;
        public GameObject _ErrorMessage;

        public int value => _DropDown.value;

        public bool HasSelection => _DropDown.value > 0;

        public List<TMP_Dropdown.OptionData> options => _DropDown.options;

        private int mDefaultValue = 0;

        protected void Awake()
        {
            if (_DropDown == null)
                _DropDown = GetComponentInChildren<TMP_Dropdown>();

            mDefaultValue = _DropDown.value;
            ResetToDefault();
            _DropDown.onValueChanged.AddListener(OnValueChange);
        }

        private void OnValueChange(int selectedIndex)
        {
            if (OnSelectionChange != null)
                OnSelectionChange.Invoke(GetSelectedText());
        }

        public string GetSelectedText()
        {
            return _DropDown.options[_DropDown.value].text;
        }

        public int GetSelectedIndex()
        {
            return _DropDown.value;
        }

        public void SetSelected(int index)
        {
            _DropDown.value = index;
            if (OnSelectionChange != null)
                OnSelectionChange.Invoke(GetSelectedText());
        }

        public void SetSelected(string text)
        {
            int index = _DropDown.options.FindIndex(e => e.text == text);
            _DropDown.value = index;
            if (OnSelectionChange != null)
                OnSelectionChange.Invoke(GetSelectedText());
        }

        public virtual void SetInteractable(bool interactive)
        {
            _DropDown.interactable = interactive;
        }

        public virtual bool IsInteractable()
        {
            return _DropDown.IsInteractable();
        }

        public virtual void ResetToDefault()
        {
            if (mDefaultValue == -1)
            {
                //We are doing all of this circus because unity does not have an option to clear seleted option.
                TMP_Dropdown.OptionData dd = new TMP_Dropdown.OptionData();
                dd.text = _DefaultText;
                dd.image = _DefaultSprite;
                _DropDown.options.Insert(0, dd);
                _DropDown.value = 0;
                if (_DropDown.captionImage != null)
                    _DropDown.captionImage.sprite = _DefaultSprite;
                if (_DropDown.captionText != null)
                    _DropDown.captionText.text = _DefaultText;
            }
            else
                SetSelected(mDefaultValue);
        }

        public virtual void AddOption(string optionText, Sprite sprite, bool keepSelected = false, int insertAt = -1)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData()
            {
                text = optionText,
                image = sprite
            };

            if (insertAt >= 0)
                _DropDown.options.Insert(insertAt, option);
            else
                _DropDown.options.Add(option);

            if (keepSelected)
                SetSelected(optionText);

            if (_FitDropdownToContents)
            {
                float h = _DropDown.template.GetComponent<ScrollRect>().content.GetSize().y * _DropDown.options.Count;
                _DropDown.template.Height(h);
            }
        }

        public virtual void Clear()
        {
            _DropDown.ClearOptions();
        }

        public bool IsValid()
        {
            if (_ErrorMessage != null)
            {
                _ErrorMessage.SetActive(GetSelectedIndex() == 0);
                return !_ErrorMessage.activeInHierarchy;
            }
            return true;
        }
    }
}