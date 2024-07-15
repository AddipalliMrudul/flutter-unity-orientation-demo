using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public class UiExpandItem : UiItem
    {
        [SerializeField] private UiItem _ExpandBtn = null;
        public ScrollRect scrollRect { get; set; }

        public bool IsExpanded { get { return scrollRect.gameObject.activeSelf; } }
        public int Index { get; set; } 

        public virtual void OnExpand()
        {
            _ExpandBtn.PlayAnim("OnExpand");
            scrollRect.SetActive(true);
            scrollRect.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
            scrollRect.verticalScrollbar.value = (scrollRect.verticalScrollbar.direction == Scrollbar.Direction.BottomToTop ? 1f : 0f);
        }

        public virtual void OnShrink()
        {
            _ExpandBtn.PlayAnim("OnShrink");
            scrollRect.SetActive(false);
        }
    }
}
