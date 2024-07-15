using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiShinyEffect : MonoBehaviour
    {
        public enum ScrollDirection
        {
            BottomRightToTopLeft,
            TopLeftToBottomRight,
            LeftToRight,
        }

        #region Properties
        [SerializeField] RectTransform _Container = null;
        [SerializeField] float _Speed = 1f;
        [SerializeField] ScrollDirection _ScrollDirection = ScrollDirection.BottomRightToTopLeft;
        Vector3 mDirection = Vector3.zero;
        float mLastDiff = float.MaxValue;
        /// <summary>
        /// Set the object position to a proper place by default
        /// </summary>
        Vector3 mStartPosition = Vector3.zero;

        /// <summary>
        /// used for storing corners values of a rect
        /// 0 – Bottom Left, 1 – Top Left, 2 – Top Right, 3 – Bottom Right
        /// </summary>
        Vector3[] corners = null;
        /// <summary>
        /// Caching the transform reference
        /// </summary>
        Transform mTransform = null;

        #endregion //Properties

        /// <summary>
        /// Update the corners & direction based on scroll direction 
        /// </summary>
        private void Awake()
        {
            mTransform = transform;
            mStartPosition = mTransform.localPosition;
            corners = new Vector3[4];

            //If the canvas render mode is screen space camera, corners are not set properly
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransform rectTransform = (RectTransform)transform;
                corners[1] = rectTransform.anchoredPosition;
                corners[3] = new Vector3(-rectTransform.anchoredPosition.x, -rectTransform.anchoredPosition.y);
            }
            else
                _Container.GetWorldCorners(corners);

            if (_ScrollDirection == ScrollDirection.BottomRightToTopLeft)
            {
                mDirection.x = -1f;
                mDirection.y = 1f;
            }
            else if (_ScrollDirection == ScrollDirection.TopLeftToBottomRight)
            {
                mDirection.x = 1f;
                mDirection.y = -1f;
            }
            else if (_ScrollDirection == ScrollDirection.LeftToRight)
            {
                mDirection.x = 1f;
                mDirection.y = 0f;
            }
        }

        /// <summary>
        /// Moving position based on scroll direction
        /// </summary>
        private void Update()
        {
            mTransform.localPosition += Time.deltaTime * _Speed * mDirection;
            if (_ScrollDirection == ScrollDirection.BottomRightToTopLeft)
            {
                float diff = Vector3.Distance(corners[1], mTransform.localPosition);
                if (diff > mLastDiff)
                {
                    Loop();
                }
                mLastDiff = diff;
            }
            else if (_ScrollDirection == ScrollDirection.TopLeftToBottomRight || _ScrollDirection == ScrollDirection.LeftToRight)
            {
                float diff = corners[3].x - mTransform.localPosition.x;
                if (diff < 0)
                {
                    Loop();
                }
            }
        }

        /// <summary>
        /// Reset to initial position 
        /// </summary>
        private void Loop()
        {
            mTransform.localPosition = mStartPosition;
        }
    }
}