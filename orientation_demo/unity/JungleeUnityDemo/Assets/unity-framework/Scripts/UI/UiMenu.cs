using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using XcelerateGames.Locale;

namespace XcelerateGames.UI
{
    public class UiMenu : UiBase
    {
        public GameObject _Template = null;
        public Transform _Grid = null;
        public List<BindingData> _BindingData = null;
        //Enable/Disable horizontal/vertical scrolling based on child count under content
        public bool _AutoControlSrcolling = true;

        public Action<UiItem> OnItemSelected = null;

        private UiItem mSelectedItem = null;
        protected bool? mIsVerticalScrollEnabled, mIsHorizontalScrollEnabled;

        public UiItem pSelectedItem { get { return mSelectedItem; } }

        public Func<UiMenu, UiItem[]> GetChildItemsFun;

        protected override void Start()
        {
            base.Start();

            if (_Template != null)
                _Template.SetActive(false);
            if (_Grid != null)
            {
                UiItem[] childItems = _Grid.GetComponentsInChildren<UiItem>(true);
                foreach (UiItem item in childItems)
                    item._OnClick.AddListener(OnSelect);
            }

            UpdateScrolling();
        }

        private void RegisterID()
        {
#if AUTOMATION_ENABLED
            if (ID != null)
            {
                IDMapper.RegisterID(ID, gameObject.GetObjectPath());
            }
#endif
        }

        public UiItem AddWidget()
        {
            UiItem item = Utilities.Instantiate<UiItem>(_Template, "MenuItem");
            item.SetActive(true);
            UpdateScrolling();
            return AddWidget(item);
        }

        public UiItem AddWidget(UiItem inItem)
        {
            inItem.transform.SetParent(_Grid, false);
            inItem._OnClick.AddListener(OnSelect);
            return inItem;
        }

        public UiItem AddWidget(string widgetText)
        {
            UiItem inItem = AddWidget();
            if (!string.IsNullOrEmpty(widgetText))
            {
                inItem.SetText(widgetText);
                inItem.name = widgetText;
#if AUTOMATION_ENABLED
inItem.UpdateID(name + "_" + widgetText);
#endif
            }
            return inItem;
        }

        public T AddWidget<T>(string widgetText) where T : UiItem
        {
            return AddWidget(widgetText) as T;
        }

        //Adds widgets & binds their properties
        public void AddWidgets<T>(List<T> items)
        {
            foreach (object obj in items)
            {
                List<FieldInfo> fieldInfo = ReflectionUtilities.GetFields(obj.GetType());
                foreach (FieldInfo fInfo in fieldInfo)
                {
                    //Debug.Log($"{fInfo.Name} : {fInfo.GetValue(obj)}");

                    BindingData bindingData = GetBinding(fInfo.Name);
                    if (bindingData != null)
                    {
                        if (bindingData._Type == BindingType.String)
                            bindingData._Item.SetText((string)fInfo.GetValue(obj));
                        else if (bindingData._Type == BindingType.LocalizedString)
                            bindingData._Item.SetText(Localization.Get((string)fInfo.GetValue(obj)));
                        else if (bindingData._Type == BindingType.Texture)
                            bindingData._Item.SetTexture((string)fInfo.GetValue(obj), null);
                        else if (bindingData._Type == BindingType.Sprite)
                            bindingData._Item.SetSprite((string)fInfo.GetValue(obj), null);
                        else if (bindingData._Type == BindingType.Int)
                            bindingData._Item.SetText((int)fInfo.GetValue(obj));
                        else if (bindingData._Type == BindingType.Float)
                            bindingData._Item.text = ((float)fInfo.GetValue(obj)).ToString();
                        else if (bindingData._Type == BindingType.None)
                            XDebug.LogError($"Invalid type {bindingData._Type} for property {bindingData._Name}");
                        else
                            XDebug.LogError($"Add support for type {bindingData._Type}");
                    }
                    else
                        XDebug.LogError($"Could not find Binding for property: {fInfo.Name}");
                }
            }

            BindingData GetBinding(string propertyName)
            {
                if (_BindingData.IsNullOrEmpty())
                    return null;

                return _BindingData.Find(binding => binding._Name.Equals(propertyName));
            }
        }

        protected virtual void OnSelect(UiItem inItem)
        {
            if (inItem != mSelectedItem)
            {
                mSelectedItem = inItem;
                OnItemSelected?.Invoke(mSelectedItem);
            }
        }

        public virtual void SetSelected(UiItem inItem)
        {
            if (inItem != mSelectedItem)
            {
                OnSelect(inItem);
                ScrollRect scrollRect = _Grid.GetComponentInParent<ScrollRect>();
                if (scrollRect != null)
                    scrollRect.SnapTo(inItem.GetComponent<RectTransform>());
            }
        }

        public virtual void ClearWidgets()
        {
            UiItem[] widgets = _Grid.GetComponentsInChildren<UiItem>(true);
            for (int i = 0; i < widgets.Length; ++i)
            {
                if (widgets[i] != null)
                {
                    //We usually keep template item as child of grid, Ignore template item while destroying children
                    if (widgets[i].gameObject.GetInstanceID() != _Template.GetInstanceID() && widgets[i].transform.parent == _Grid)
                    {
                        widgets[i].IsDestroying = true;
                        GameObject.Destroy(widgets[i].gameObject);
                    }
                }
            }
        }

        public void Sort(Func<UiItem, UiItem, int> SortFunc)
        {
            if (SortFunc != null)
            {
                //Get all child objects.
                //NOTE: Get children returns all child objects that have UiItem component attached irrespective of the depth. 
                //To have more control, set GetChildItemsFun before calling sort 
                UiItem[] widgets = GetChildren();
                //Now sort the items based on criteria controlled by SortFunc
                Array.Sort(widgets, (UiItem item1, UiItem item2) => SortFunc(item1, item2));
                //Now re-arrenge the trasforms based on sorting order.
                int index = 0;
                foreach (UiItem item in widgets)
                    item.transform.SetSiblingIndex(index++);
            }
        }

        public void Sort<T>(Func<T, T, int> SortFunc) where T : UiItem
        {
            if (SortFunc != null)
            {
                //Get all child objects.
                List<T> widgets = GetChildren<T>(false);
                //Now sort the items based on criteria controlled by SortFunc
                widgets.Sort((T item1, T item2) => SortFunc(item1, item2));
                //Now re-arrenge the trasforms based on sorting order.
                int index = 0;
                foreach (UiItem item in widgets)
                    item.transform.SetSiblingIndex(index++);
            }
        }

        protected override UiItem[] GetChildren()
        {
            if (GetChildItemsFun == null)
                return _Grid.GetComponentsInChildren<UiItem>(true);
            else
                return GetChildItemsFun(this);
        }

        public List<UiItem> GetChildren(bool includeInactive)
        {
            List<UiItem> widgets = new List<UiItem>(_Grid.GetComponentsInChildren<UiItem>(includeInactive));
            //Now remove all objects that are marked for deletion
            widgets.RemoveAll((obj) => { return obj.IsDestroying; });

            return widgets;
        }

        public List<T> GetChildren<T>(bool includeInactive) where T : UiItem
        {
            List<T> widgets = new List<T>(_Grid.GetComponentsInChildren<T>(includeInactive));
            //Now remove all objects that are marked for deletion
            widgets.RemoveAll((obj) => { return obj.IsDestroying; });

            return widgets;
        }

        //Delaying calculation as it takes time for the UI to resize after adding/removing an item
        public void UpdateScrolling()
        {
            Invoke("DoUpdateScrolling", Time.deltaTime);
        }

        //Disables scrolling if menu has less items than the size of viewport
        //For this to work, ViewPort must be set to Stretch in both directions & Contents object must have ContentSixeFitter with PreferredSize
        [ContextMenu("UpdateScrolling")]
        public void DoUpdateScrolling()
        {
            if (_Grid == null || !_AutoControlSrcolling)
                return;
            ScrollRect scrollRect = _Grid.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                if (!mIsHorizontalScrollEnabled.HasValue)
                    mIsHorizontalScrollEnabled = scrollRect.horizontal;
                if (!mIsVerticalScrollEnabled.HasValue)
                    mIsVerticalScrollEnabled = scrollRect.vertical;
                if (mIsVerticalScrollEnabled.Value)
                {
                    //Debug.Log($"ViewPort: {scrollRect.viewport.GetComponent<RectTransform>().rect.height}, Content: {scrollRect.content.GetSize().y}");
                    scrollRect.vertical = scrollRect.content.GetSize().y > scrollRect.viewport.GetComponent<RectTransform>().rect.height;
                    if (!scrollRect.vertical && scrollRect.verticalScrollbar != null)
                        scrollRect.verticalScrollbar.value = 0.5f;
                }
                if (mIsHorizontalScrollEnabled.Value)
                {
                    //Debug.Log($"ViewPort: {scrollRect.viewport.GetComponent<RectTransform>().rect.width}, Content: {scrollRect.content.GetSize().x}");
                    scrollRect.horizontal = scrollRect.content.GetSize().x > scrollRect.viewport.GetComponent<RectTransform>().rect.width;
                    if (!scrollRect.horizontal && scrollRect.horizontalScrollbar != null)
                        scrollRect.horizontalScrollbar.value = 0.5f;
                }

                OnScrollingStateUpdated(scrollRect.horizontal, scrollRect.vertical);
            }
        }

        protected virtual void OnScrollingStateUpdated(bool horizontal, bool vertical)
        {
        }
    }
}
