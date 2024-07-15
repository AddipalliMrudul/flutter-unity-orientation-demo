using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames.UIDemo
{
    public class UiExpandDemo : UiExpand
    {
        #region Properties
        [SerializeField] protected UiItem _Text;

        public string[] _Texts;

        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods

        protected override void OnExpand(UiExpandItem item, int index)
        {
            base.OnExpand(item, index);
            (item as UiExpandItemDemo)._Info.text = _Texts[index];
            //_Text.text = _Texts[index];
        }

        protected override void OnShrink(UiExpandItem item, int index)
        {
            base.OnShrink(item, index);
        }

        protected override ScrollRect GetScrollRect(int index)
        {
            ScrollRect scrollRect = base.GetScrollRect(index);
            UiExpandItemDemo itemDemo = mExpandableItems[index] as UiExpandItemDemo;
            itemDemo._Info = scrollRect.GetComponentInChildren<UiItem>();
            return scrollRect;
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
    }
}
