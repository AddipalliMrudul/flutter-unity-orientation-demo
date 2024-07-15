using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.UI
{
    /// <summary>
    /// Used for scroll view with static items on top followed by pooled items of same length
    /// Currently only vertical scroll is supported
    /// </summary>
    public class UiVariableSizeScrollList : UiScrollListBase
    {
        #region Properties
        [SerializeField] List<RectTransform> _FixedRectTransforms;
        private float mFixedItemsNormalizedBoundary = 0;
        #endregion Properties

        #region Methods
        protected override int NoOfActiveElements() { return 0; }
        protected override void AddElements(int requiredElementsInList, int elementsCulledAbove) { }
        protected override void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove) { }
        protected void OnEnable()
        {
            float fixedItemsLength = 0;
            int activeFixedItems = 0;
            for (int i = 0; i < _FixedRectTransforms.Count; i++)
            {
                if (_FixedRectTransforms[i].gameObject.activeInHierarchy)
                {
                    fixedItemsLength += _FixedRectTransforms[i].rect.height;
                    activeFixedItems++;
                }
            }

            fixedItemsLength += activeFixedItems * mLayoutSpacing;
            AdjustContentSize((mTemplateSize * mTotalElementSize) + fixedItemsLength);
            mFixedItemsNormalizedBoundary = fixedItemsLength / _ScrollRect.content.rect.height;
        }

        protected override float GetScrollRectNormalizedPosition()
        {
            float originalNormalized = base.GetScrollRectNormalizedPosition();
            float min = mFixedItemsNormalizedBoundary;

            float newNormalized = Mathf.Clamp01((originalNormalized - min) / (1 - min));
            return newNormalized;
        }
        #endregion Methods
    }
}