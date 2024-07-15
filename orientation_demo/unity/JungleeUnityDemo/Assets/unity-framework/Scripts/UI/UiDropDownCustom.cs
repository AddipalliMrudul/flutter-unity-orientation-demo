using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [Serializable]
    public class DropDownOption
    {
        public string text = null;
        public Sprite sprite = null;
    }

    public class UiDropDownCustom : UiContainer
    {
        [SerializeField] protected List<DropDownOption> _DropDownOptions = null;
        //If set to true, drop down height will adjusted to fit children size
        [SerializeField] protected bool _FitDropDownToContents = true;
        [SerializeField] protected float _AnimationSpeed = 0.25f;

        protected UiItem mDropDownButton = null;
        protected RectTransform mScrollRect = null;
        protected List<UiItem> mItems = null;
        protected int mSelectedIndex = -1;
        protected float mHeight = 128;

        #region callbacks
        public UnityEvent<int> OnSelectionChange = null;
        public UnityEvent OnOpened = null;
        public UnityEvent OnClosed = null;
        #endregion callbacks

        #region UI Callbacks
        public void OnClickDropDown(UiItem item)
        {
            Expand();
            OnOpened?.Invoke();
        }
        #endregion UI Callbacks

        #region Private/Protected Methods
        private void Awake()
        {
            if (_DropDownOptions == null)
                _DropDownOptions = new List<DropDownOption>();
            mDropDownButton._OnClick.AddListener(OnClickDropDown);
            mScrollRect.SetActive(false);
        }

        protected override void Start()
        {
            base.Start();
            Populate();
        }

        private void Populate()
        {
            if (mItems == null)
                mItems = new List<UiItem>();
            else
                mItems.Clear();
            foreach (DropDownOption dropDownOption in _DropDownOptions)
            {
                UiItem item = AddWidget<UiItem>(dropDownOption.text);
                item._InvokeDelay = 0f;
                mItems.Add(item);
            }
            mHeight = mScrollRect.GetSize().y;
            if (_FitDropDownToContents)
            {
                SetHeight();
            }
        }

        protected override void OnSelect(UiItem inItem)
        {
            base.OnSelect(inItem);
            mSelectedIndex = mItems.FindIndex(e => e == inItem);
            mDropDownButton.text = _DropDownOptions[mSelectedIndex].text;
            mDropDownButton.SetSprite(_DropDownOptions[mSelectedIndex].sprite);
            OnSelectionChange?.Invoke(mSelectedIndex);
            Collapse();
        }

        private void Expand()
        {
            mScrollRect.SetActive(true);
            Vector2 sizeDelta = mScrollRect.sizeDelta;
            mScrollRect.sizeDelta = new Vector2(sizeDelta.x, 0);
            mScrollRect.DOSizeDelta(new Vector2(sizeDelta.x, sizeDelta.y), _AnimationSpeed);
            OnOpened?.Invoke();
            mDropDownButton.SetInteractive(false);
        }

        private void Collapse()
        {
            Vector2 sizeDelta = mScrollRect.sizeDelta;
            mScrollRect.DOSizeDelta(new Vector2(sizeDelta.x, 0), _AnimationSpeed).OnComplete(() =>
            {
                mScrollRect.SetActive(false);
                SetHeight();
                mDropDownButton.SetInteractive(true);
            });
            OnClosed?.Invoke();
        }

        protected void SetHeight()
        {
            float h = mHeight;
            if (_FitDropDownToContents)
            {
                h = _Template.GetComponent<RectTransform>().GetSize().y * _DropDownOptions.Count;
            }
            mScrollRect.Height(h);
        }
        #endregion Private/Protected Methods

        #region Public Methods
        public void AddOptions(List<DropDownOption> dropDownOptions)
        {
            _DropDownOptions.Clear();
            _DropDownOptions.AddRange(dropDownOptions);
        }

        public void SetSelected(int index)
        {
            OnSelect(mItems[index]);
        }
        #endregion Public Methods

        #region Editor Only Code
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity in IDE only to validate values
        /// </summary>
        protected virtual void OnValidate()
        {
            mDropDownButton = GetComponent<UiItem>();
            mDropDownButton._TextItem.raycastTarget = false;
            mScrollRect = GetComponentInChildren<ScrollRect>().GetComponent<RectTransform>();
        }
#endif //UNITY_EDITOR
        #endregion Editor Only Code
    }
}
