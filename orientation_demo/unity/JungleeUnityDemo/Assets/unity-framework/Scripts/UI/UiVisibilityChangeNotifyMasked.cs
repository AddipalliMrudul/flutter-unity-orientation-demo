using UnityEngine;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UiVisibilityChangeNotifyMasked : MonoBehaviour
    {
        public enum ScrollDirection
        {
            Vertical,
            Horizontal
        }

        public ScrollDirection _ScrollDirection = ScrollDirection.Vertical;
        public RectTransform _MaskedRectTransform = null;
        private bool? mVisibilityStatus;
        private RectTransform mRectTransform = null;
        //0 – Bottom Left, 1 – Top Left, 2 – Top Right, 3 – Bottom Right
        private Vector3[] corners = null;

        private bool mSkipped = false;

        public System.Action<UiVisibilityChangeNotifyMasked, bool> OnVisibilityChangeEvent = null;

        public bool pIsVisible
        {
            get
            {
                if (!mVisibilityStatus.HasValue)
                    Update();
                return mVisibilityStatus.Value;
            }
        }

        private void Start()
        {
            mRectTransform = GetComponent<RectTransform>();
            corners = new Vector3[4];
        }

        private void Update()
        {
            bool now = IsVisible();
            if (!mVisibilityStatus.HasValue || now != mVisibilityStatus)
            {
                mVisibilityStatus = now;

                //We skip the first event, this is to avoid all widgets sending their events on creation.
                if (mSkipped && OnVisibilityChangeEvent != null)
                {
                    //Debug.LogError(name + " : " + mVisibilityStatus + " " + Time.frameCount);
                    OnVisibilityChangeEvent(this, now);
                }
                else
                {
                    //Debug.LogError(name + "  skipped : " + mVisibilityStatus);
                    mSkipped = true;
                }
            }
        }

        /// <summary>
        /// We usually dont have scrolling in both directions,
        /// So checking for bottom left corner always
        /// </summary>
        /// <returns></returns>
        private bool IsVisible()
        {
            mRectTransform.GetWorldCorners(corners);
            bool visible = false;
            Vector3 point;
            //Check top left
            point = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[1]);
            visible = RectTransformUtility.RectangleContainsScreenPoint(_MaskedRectTransform, point, Camera.main);
            if (visible)
                return visible;
            if (_ScrollDirection == ScrollDirection.Vertical)
            {
                //Check bottom Left
                point = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]);
                visible = RectTransformUtility.RectangleContainsScreenPoint(_MaskedRectTransform, point, Camera.main);
                if (visible)
                    return visible;
            }
            else if (_ScrollDirection == ScrollDirection.Horizontal)
            {
                //Check bottom right
                point = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[3]);
                visible = RectTransformUtility.RectangleContainsScreenPoint(_MaskedRectTransform, point, Camera.main);
                if (visible)
                    return visible;
            }

            return visible;
        }
    }
}