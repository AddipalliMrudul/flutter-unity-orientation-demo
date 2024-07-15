using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public abstract class UiScrollListBase : UiMenu
    {
        #region Properties
        /// <summary>
        /// Used to shifting the element from top to bottom or from bottom to top
        /// </summary>
        public enum ReorientMethod
        {
            TopToBottom,
            BottomToTop
        }
        /// <summary>
        /// Space layout element - used for creating the blank space inside scroll rect
        /// </summary>
        [SerializeField] protected LayoutElement _SpaceElement = null;
        /// <summary>
        /// Scroll Rect
        /// </summary>
        [SerializeField] protected ScrollRect _ScrollRect = null;

        protected int mLastElementCulledAbove = -1;
        protected int mTotalElementSize = 0;
        protected RectOffset mPadding = null;
        protected float mLayoutSpacing;
        protected float mTemplateSize;
        protected float mScrollAreaSize;
        protected int mElementsVisibleInScrollArea = 0;
        protected int mRequiredElementsInList = 0;

        /// <summary>
        /// Return no of ui elements size
        /// </summary>
        protected int mActiveElementSize => NoOfActiveElements();
        #endregion Properties

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();
            _ScrollRect.onValueChanged.AddListener(OnScrolling);
            mScrollAreaSize = _ScrollRect.vertical ? _ScrollRect.viewport.rect.height : _ScrollRect.viewport.rect.width;
            RectTransform templateRt = _Template.GetComponent<RectTransform>();
            mTemplateSize = _ScrollRect.vertical ? templateRt.rect.height : templateRt.rect.width;
            var layoutGroup = _ScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                mLayoutSpacing = layoutGroup.spacing;
                mPadding = layoutGroup.padding;
                mTemplateSize += mLayoutSpacing;
            }
            else
                XDebug.LogError($"HorizontalOrVerticalLayoutGroup must not be null");
            _SpaceElement = CreateSpaceElement(_ScrollRect, 0f);
            _SpaceElement.transform.SetParent(_ScrollRect.content.transform, false);
            mElementsVisibleInScrollArea = Mathf.CeilToInt(mScrollAreaSize / mTemplateSize);
        }

        protected override void OnDestroy()
        {
            _ScrollRect.onValueChanged.RemoveListener(OnScrolling);
            Destroy(_SpaceElement.gameObject);
            base.OnDestroy();
        }

        /// <summary>
        /// Callback of ScrollRect's OnValueChange 
        /// </summary>
        /// <param name="delta"></param>
        private void OnScrolling(Vector2 delta)
        {
            UpdateContent();
        }
        #endregion Unity Methods

        #region Abstract/Virtual Methods
        protected abstract int NoOfActiveElements();
        protected abstract void AddElements(int requiredElementsInList, int elementsCulledAbove);
        protected abstract void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove);

        /// <summary>
        /// Initialize the required data 
        /// </summary>
        /// <param name="totalDataSize">total no of elements in full list</param>
        public virtual void Initialize(int totalDataSize)
        {
            mLastElementCulledAbove = -1;
            mTotalElementSize = totalDataSize;
            mRequiredElementsInList = Mathf.Min(mElementsVisibleInScrollArea + 1, mTotalElementSize);
            ResetPosition();
            AdjustContentSize(mTemplateSize * mTotalElementSize);
            UpdateContent();
        }

        /// <summary>
        /// Reset Position
        /// </summary>
        public virtual void ResetPosition()
        {
            if (_ScrollRect.vertical)
                _ScrollRect.verticalNormalizedPosition = 1;
            else
                _ScrollRect.horizontalNormalizedPosition = 0;
        }

        /// <summary>
        /// Update the content's size of scroll rect
        /// </summary>
        /// <param name="size"></param>
        public virtual void AdjustContentSize(float size)
        {
            Vector2 currentSize = _ScrollRect.content.sizeDelta;
            size -= mLayoutSpacing;
            if (_ScrollRect.vertical)
            {
                if (mPadding != null)
                    size += mPadding.top + mPadding.bottom;
                currentSize.y = size;
            }
            else
            {
                if (mPadding != null)
                    size += mPadding.left + mPadding.right;
                currentSize.x = size;
            }
            _ScrollRect.content.sizeDelta = currentSize;
        }

        /// <summary>
        /// Updating ui elements data & adjusting blank space while scrolling
        /// </summary>
        protected void UpdateContent()
        {
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (mTotalElementSize - mElementsVisibleInScrollArea)), 0,
                Mathf.Clamp(mTotalElementSize - (mElementsVisibleInScrollArea + 1), 0, int.MaxValue));
            if (mLastElementCulledAbove == elementsCulledAbove)
                return;
            AdjustSpaceElement(mTemplateSize * elementsCulledAbove);
            if (mActiveElementSize == 0 || mActiveElementSize != mRequiredElementsInList)
                AddElements(mRequiredElementsInList, elementsCulledAbove);
            else if (mLastElementCulledAbove != elementsCulledAbove)
                ReorientElement(elementsCulledAbove > mLastElementCulledAbove ? ReorientMethod.TopToBottom : ReorientMethod.BottomToTop, elementsCulledAbove);
            mLastElementCulledAbove = elementsCulledAbove;
            UpdateActiveElementsOnUi();
        }

        /// <summary>
        ///  Must to override this method for updating the ui elements while initialization or scrolling
        /// </summary>
        protected virtual void UpdateActiveElementsOnUi() { }

        /// <summary>
        /// Used for adjusting the blank space element's size
        /// </summary>
        /// <param name="size">used either height or width based on selected layout group</param>
        public virtual void AdjustSpaceElement(float size)
        {
            if (size <= 0)
                _SpaceElement.ignoreLayout = true;
            else
            {
                _SpaceElement.ignoreLayout = false;
                size -= mLayoutSpacing;
            }
            if (_ScrollRect.vertical)
                _SpaceElement.minHeight = size;
            else
                _SpaceElement.minWidth = size;
            _SpaceElement.transform.SetAsFirstSibling();
        }

        /// <summary>
        /// Returns the Scroll rect normalized position
        /// </summary>
        /// <returns></returns>
        protected virtual float GetScrollRectNormalizedPosition()
        {
            return Mathf.Clamp01(_ScrollRect.vertical ? 1 - _ScrollRect.verticalNormalizedPosition : _ScrollRect.horizontalNormalizedPosition);
        }

        #endregion Abstract/Virtual Methods

        #region Static Methods
        /// <summary>
        /// Create space element
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="elementSize"></param>
        /// <returns></returns>
        protected static LayoutElement CreateSpaceElement(ScrollRect scrollRect, float elementSize)
        {
            var spaceElement = new GameObject("SpaceElement").AddComponent<LayoutElement>();
            if (scrollRect.vertical)
                spaceElement.minHeight = elementSize;
            else
                spaceElement.minWidth = elementSize;
            return spaceElement;
        }
        #endregion Static Methods
    }
}