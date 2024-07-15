using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public class UiExpand : UiMenu
    {
        #region Properties
        [SerializeField] protected ScrollRect _ScrollRect;
        [SerializeField] protected bool _AllowMultiple = false;
        //Transform of the VerticalLayoutGroup/HorizontalLayoutGroup/GridLayoutGroup
        [SerializeField] protected Transform _ParentLayout;

        protected UiExpandItem mCurrentItem = null;
        protected List<UiExpandItem> mExpandableItems = new List<UiExpandItem>();
        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        public virtual void OnSelect(UiExpandItem item)
        {
            ScrollRect scrollRect = GetScrollRect(item.Index);

            if (_AllowMultiple)
            {
                mCurrentItem = item;
                if (item.IsExpanded)
                {
                    OnReselect();
                }
                else
                {
                    OnSelect();
                }
            }
            else
            {
                if (mCurrentItem != null)
                {
                    OnReselect();
                    if (mCurrentItem == item)
                    {
                        mCurrentItem = null;
                        return;
                    }
                }
                if (mCurrentItem != item || !scrollRect.gameObject.activeSelf)
                {
                    mCurrentItem = item;
                    OnSelect();
                }
            }
        }
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();
        }

        protected virtual void CacheItems()
        {
            for (int i = 0; i < _ParentLayout.childCount; ++i)
            {
                UiExpandItem item = _ParentLayout.GetChild(i).GetComponent<UiExpandItem>();
                if (item != null)
                {
                    item.Index = i;
                    mExpandableItems.Add(item);
                }
            }
            _ScrollRect.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual void OnExpand(UiExpandItem item, int index)
        {
            item.OnExpand();
        }

        protected virtual void OnShrink(UiExpandItem item, int index)
        {
            item.OnShrink();
        }

        protected virtual void OnReselect()
        {
            OnShrink(mCurrentItem, mCurrentItem.Index);
        }

        protected virtual void OnSelect()
        {
            OnExpand(mCurrentItem, mCurrentItem.Index);
        }

        protected virtual ScrollRect GetScrollRect(int index)
        {
            if (mExpandableItems[index].scrollRect == null)
            {
                if (_AllowMultiple)
                    mExpandableItems[index].scrollRect = Utilities.Instantiate<ScrollRect>(_ScrollRect.gameObject, "ScrollItem-" + index, _ParentLayout);
                else
                    mExpandableItems[index].scrollRect = _ScrollRect;
            }
            return mExpandableItems[index].scrollRect;
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
    }
}
