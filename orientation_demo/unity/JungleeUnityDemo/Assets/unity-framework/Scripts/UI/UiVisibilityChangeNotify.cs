using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UiVisibilityChangeNotify : MonoBehaviour
    {
        public ScrollRect _ScrollRect = null;
        private bool? mVisibilityStatus;
        private RectTransform mRectTransform = null;
        private RectTransform mScrollRectTransform = null;
        //0 – Bottom Left, 1 – Top Left, 2 – Top Right, 3 – Bottom Right
        private Vector3[] corners = null;

        private bool mSkipped = false;

        public System.Action<UiVisibilityChangeNotify, bool> OnVisibilityChangeEvent = null;

        public bool pIsVisible
        {
            get
            {
                if (!mVisibilityStatus.HasValue)
                    OnScroll(Vector2.zero);
                return mVisibilityStatus.Value;
            }
        }

        private void Start()
        {
            mRectTransform = GetComponent<RectTransform>();
            mScrollRectTransform = _ScrollRect.GetComponent<RectTransform>();
            corners = new Vector3[4];
            _ScrollRect.onValueChanged.AddListener(OnScroll);

            //Compute visibility on start once, later it will be calculated only on scroll event
            //OnScroll(Vector2.zero);
            //Temp
            //GetComponentInChildren<Text>().text = gameObject.name;
        }

        private void OnScroll(Vector2 scrollDelta)
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
            visible = RectTransformUtility.RectangleContainsScreenPoint(mScrollRectTransform, point, Camera.main);
            if (visible)
                return visible;
            if (_ScrollRect.vertical)
            {
                //Check bottom Left
                point = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]);
                visible = RectTransformUtility.RectangleContainsScreenPoint(mScrollRectTransform, point, Camera.main);
                if (visible)
                    return visible;
            }
            if (_ScrollRect.horizontal)
            {
                //Check bottom right
                point = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[3]);
                visible = RectTransformUtility.RectangleContainsScreenPoint(mScrollRectTransform, point, Camera.main);
                if (visible)
                    return visible;
            }

            return visible;
        }
    }
}