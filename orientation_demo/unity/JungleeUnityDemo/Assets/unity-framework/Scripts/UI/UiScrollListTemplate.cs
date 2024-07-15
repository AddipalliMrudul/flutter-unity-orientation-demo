using System.Collections.Generic;

namespace XcelerateGames.UI
{
    public class UiScrollListTemplate : UiScrollListBase
    {
        /// <summary>
        /// Data Container
        /// </summary>
        private object mData;
        /// <summary>
        /// Ui Item/Element's List
        /// </summary>
        private List<UiItem> mItems = new List<UiItem>();

        /// <summary>
        /// Return number of ui element's size
        /// </summary>
        /// <returns></returns>
        protected override int NoOfActiveElements()
        {
            return mItems.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiredElementsInList"></param>
        /// <param name="elementsCulledAbove"></param>
        protected override void AddElements(int requiredElementsInList, int elementsCulledAbove)
        {
           
        }

        /// <summary>
        /// Re-orient the element while scrolling
        /// </summary>
        /// <param name="reorientMethod"> Possible methods-
        /// 1.TopToBottom-Shift element from top to bottom
        /// 2.BottomToTop- Shift element from bottom to top
        /// </param>
        /// <param name="elementsCulledAbove">The number of ui elements culled above the scroll list</param>
        protected override void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove)
        {
            if (mActiveElementSize <= 1)
                return;
            if(reorientMethod == ReorientMethod.TopToBottom)
            {
                var topItem = mItems[0];
                mItems.RemoveAt(0);
                mItems.Add(topItem);
                topItem.transform.SetSiblingIndex(mItems[mActiveElementSize - 2].transform.GetSiblingIndex() + 1);
                //TODO: Update the 'topItem' data
            }
            else
            {
                var bottomItem = mItems[mActiveElementSize - 1];
                mItems.RemoveAt(mActiveElementSize - 1);
                mItems.Insert(0, bottomItem);
                bottomItem.transform.SetSiblingIndex(mItems[1].transform.GetSiblingIndex());
                //TODO: Update the 'bottomItem' data
            }
        }
    }
}