using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames
{
    [System.Serializable]
    public class DropDownMenuEvent : UnityEvent<string> { }

    public class UiDropDownMenu : UiBase
    {
        public string _DefaultText = null;
        public Sprite _DefaultSprite = null;

        public Dropdown _DropDown = null;

        public DropDownMenuEvent OnSelectionChange = null;

        private int mDefaultValue = 0;

        protected override void Awake()
        {
            base.Awake();
            if (_DropDown == null)
                _DropDown = GetComponentInChildren<Dropdown>();

            mDefaultValue = _DropDown.value;
            ResetToDefault();
            _DropDown.onValueChanged.AddListener(OnValueChange);
        }

        private void OnValueChange(int selectedIndex)
        {
            if (OnSelectionChange != null)
                OnSelectionChange.Invoke(GetSelectedText());
        }

        protected override void Start()
        {
            base.Start();
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
            SetSelected(_DropDown.options.FindIndex(e => e.text == text));
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
                Dropdown.OptionData dd = new Dropdown.OptionData();
                dd.text = _DefaultText;
                dd.image = _DefaultSprite;
                _DropDown.options.Add(dd);

                _DropDown.captionImage.sprite = _DefaultSprite;
                _DropDown.captionText.text = _DefaultText;
            }
            else
                SetSelected(mDefaultValue);
        }
    }
}