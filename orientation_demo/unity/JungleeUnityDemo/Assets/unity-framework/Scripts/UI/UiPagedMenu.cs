using XcelerateGames.UI.Animations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public class UiPagedMenu : UiMenu
    {
        public UiItem _LeftBtn, _RightBtn;
        public int mVisibleItemCount = -1;

        public int TotalPages { get; private set; }

        public UiPageMarker _PageMarkerTemplate;
        public GridLayoutGroup _PageMarkerGrid;

        private GridLayoutGroup mGridLayout;
        //This anim will do the animation of page switching.
        private UiAnim mAnim = null;
        private List<UiPageMarker> mPageMarkers;
        private int mCurrentPage = -1;


        public int pCurrentPage { get { return mCurrentPage; } }

        private float Height
        {
            get { return _Grid.parent.GetComponent<RectTransform>().rect.height; }
        }

        #region Private & Protected Methods
        protected override void Awake()
        {
            base.Awake();
            mGridLayout = _Grid.GetComponent<GridLayoutGroup>();
            mAnim = _Grid.GetComponent<UiAnim>();
        }

        protected override void Start()
        {
            base.Start();

            _LeftBtn?._OnClick.AddListener(OnClickPageChange);
            _RightBtn?._OnClick.AddListener(OnClickPageChange);
        }

        /// <summary>
        /// Initializes the variables, Call this after adding widgets to menu or after adding/removing any game object from grid
        /// </summary>
        protected virtual void Init()
        {
            if (mGridLayout == null)
                mGridLayout = _Grid.GetComponent<GridLayoutGroup>();
            float width = _Grid.parent.GetComponent<RectTransform>().rect.width;
            mVisibleItemCount = (int)(width / (mGridLayout.cellSize.x + mGridLayout.spacing.x + mGridLayout.padding.left));
            //mVisibleItemCount++;
            TotalPages = GetTotalPages();
            CreatePageMarkers();
            GoToPage(0, true);
        }

        private void CreatePageMarkers()
        {
            _PageMarkerGrid.transform.DestroyChildren();
            mPageMarkers = new List<UiPageMarker>();
            _PageMarkerTemplate.gameObject.SetActive(true);

            if (TotalPages > 1)
            {
                for (int i = 0; i < TotalPages; ++i)
                {
                    UiPageMarker marker = Utilities.Instantiate<UiPageMarker>(_PageMarkerTemplate.gameObject, "PageMarker-" + i, _PageMarkerGrid.transform);
                    mPageMarkers.Add(marker);
                }
            }
            _PageMarkerTemplate.gameObject.SetActive(false);
        }

        private int GetTotalPages()
        {
            int rows = Mathf.FloorToInt(Height / (mGridLayout.cellSize.y + mGridLayout.spacing.y + mGridLayout.padding.top));
            int totalItems = Mathf.CeilToInt((float)_Grid.GetChildList(true).Count / rows);
            float totalWidth = totalItems * (mGridLayout.cellSize.x + mGridLayout.spacing.x + mGridLayout.padding.left);
            float widthOfEachPage = mVisibleItemCount * (mGridLayout.cellSize.x + mGridLayout.spacing.x + mGridLayout.padding.left);
            int pageCount = Mathf.CeilToInt(totalWidth / widthOfEachPage);
            return pageCount;
        }

        private void OnClickPageChange(UiItem item)
        {
            int page = (item == _RightBtn ? mCurrentPage + 1 : mCurrentPage - 1);
            page = Mathf.Clamp(page, 0, TotalPages);
            GoToPage(page, false);
        }

        private void OnPageChange()
        {
            _RightBtn?.SetActive(TotalPages > 1);
            _LeftBtn?.SetActive(TotalPages > 1);
            _RightBtn?.SetInteractive(mCurrentPage < TotalPages - 1);
            _LeftBtn?.SetInteractive(mCurrentPage > 0);
        }
        #endregion

        #region Public Methods
        public bool GoToPage(int page, bool snap)
        {
            if (page < 0 || page >= TotalPages)
            {
                XDebug.LogError("Invalid Page index : " + page);
                return false;
            }
            if (page == mCurrentPage)
                return false;
            //bool nextPage = page > mCurrentPage;
            int multiplier = -1;
            //if (nextPage)
            //{
            //    //CurrentPage++;
            //    multiplier = -1;
            //}
            //else
            //{
            //    //CurrentPage--;
            //    multiplier = -1;

            //}

            if (mCurrentPage >= 0)
                mPageMarkers[mCurrentPage].PlayAnim("Hide");
            mCurrentPage = page;
            if (mCurrentPage < mPageMarkers.Count)
                mPageMarkers[mCurrentPage].PlayAnim("Show");

            Vector3 pos = _Grid.localPosition;
            pos.x = multiplier * mCurrentPage * (mGridLayout.cellSize.x + mGridLayout.spacing.x + mGridLayout.padding.left) * mVisibleItemCount;
            if (snap)
            {
                _Grid.localPosition = pos;
            }
            else
            {
                mAnim.GetAnim("Slide").SetPositionKeys(new Vector3[] { _Grid.localPosition, pos }, false);
                mAnim.Play("Slide");
            }
            OnPageChange();

            return true;
        }

        public void RefreshPageView()
        {
            mCurrentPage = -1;
            Init();
        }
        #endregion

        #region Editor Only Code
        //#if UNITY_EDITOR
        [ContextMenu("ReInit")]
        public void ReInit()
        {
            mCurrentPage = -1;
            Init();
        }

        //#endif
        #endregion
    }
}
