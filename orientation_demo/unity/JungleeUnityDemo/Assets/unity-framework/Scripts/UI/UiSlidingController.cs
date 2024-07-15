using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace XcelerateGames.UI
{
    public class UiSlidingController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler
    {
        #region Serialize Field
        [SerializeField] float mTimeToLerp = 1f;
        [SerializeField] SlideDirection mSlideDirection = SlideDirection.LeftToRight;
        [SerializeField] Transform mSlidingUi = null;
        [SerializeField] float mMovePanelDelta = 400f;
        [SerializeField] bool mCanUseDrag = true;
        [SerializeField] Transform mButtonToRotate = null;
        #endregion

        #region private variables
        bool mIsOpen = false;
        bool mIsMoving = false;
        bool mEnteredInDragState = false;
        float mMinX, mMinY, mMaxX, mMaxY = 0f;
        Vector2 mStartPos = Vector2.zero;
        Vector2 mEndPos = Vector2.zero;
        RectTransform mSlidingUiRectTrans = null;
        Vector2 mCurrentPos;
        Vector2 mPreviousPos;
        RectTransform mSlidingUiParentRectTrans = null;
        #endregion

        #region public variables
        public UnityEvent OnOpenEvent = null;
        public UnityEvent OnOpenedEvent = null;
        public UnityEvent OnCloseEvent = null;
        public UnityEvent OnClosedEvent = null;
        #endregion

        #region interface implemented
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (mIsMoving || !mCanUseDrag) return;
            mEnteredInDragState = true;
            mPreviousPos = GetAnchoredPositionFromWorldPoint(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mIsMoving || !mCanUseDrag) return;
            SetDragPositions(eventData.position);
         }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (mIsMoving || !mCanUseDrag) return;
            SetDragPositions(eventData.position);
            MoveContainer();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (mIsMoving || !mCanUseDrag) return;
        }
        #endregion

        #region Public Functions
        public void OnArrowButtonClicked()
        {
            if (mIsMoving|| mEnteredInDragState) return;
            MoveContainer();
        }
        #endregion

        #region UI Functions
        private void Start()
        {
            SetPositions();
            SetMinMax();
            SetRotation();
        }
        #endregion

        #region Private functions
        private void MoveContainer()
        {
            if (mIsMoving) return;
            mIsMoving = true;
            StartCoroutine(MoveContainerCO());
        }

        IEnumerator MoveContainerCO()
        {
            float timeElapsed = 0f;
            Vector2 endPos = mIsOpen ? mStartPos : mEndPos;
            Vector2 startPos = mIsOpen ? mEndPos : mStartPos;
            mIsOpen = !mIsOpen;
            
            if (mIsOpen) OnOpenEvent?.Invoke();
            else OnCloseEvent?.Invoke();

            if (mEnteredInDragState)
            { 
                float actualDist = Mathf.Abs(Vector2.Distance(mStartPos,mEndPos));
                float distCovered = Mathf.Abs(Vector2.Distance(mStartPos, mSlidingUi.GetComponent<RectTransform>().anchoredPosition));
                timeElapsed = Mathf.Clamp(distCovered / actualDist,0, mTimeToLerp );
            }

            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            while (timeElapsed < mTimeToLerp)
            {
                mSlidingUiRectTrans.anchoredPosition = Vector2.Lerp(startPos,endPos,timeElapsed/mTimeToLerp);
                timeElapsed += Time.deltaTime;
                yield return wait;
            }

            mSlidingUiRectTrans.anchoredPosition = endPos;
            SetRotation();
            mIsMoving = false;
            mEnteredInDragState = false;

            if (mIsOpen) OnOpenedEvent?.Invoke();
            else OnClosedEvent?.Invoke();
        }

        Vector2 ClampGridPos(Vector2 pos)
        {
            if (mSlideDirection == SlideDirection.RightToLeft|| mSlideDirection == SlideDirection.LeftToRight)
                return new Vector2(Mathf.Clamp(pos.x, mMinX, mMaxX), mStartPos.y);
            else return new Vector2(mStartPos.x, Mathf.Clamp(pos.y, mMinY, mMaxY));
        }

        void SetMinMax()
        {
            mMinX = mStartPos.x > mEndPos.x ? mEndPos.x : mStartPos.x;
            mMinY = mStartPos.y > mEndPos.y ? mEndPos.y : mStartPos.y;
            mMaxX = mStartPos.x > mEndPos.x ? mStartPos.x : mEndPos.x;
            mMaxY = mStartPos.y > mEndPos.y ? mStartPos.y : mEndPos.y;
        }

        void SetDragPositions(Vector2 worldPosition)
        {
            mCurrentPos = GetAnchoredPositionFromWorldPoint(worldPosition);
            Vector2 delta = mCurrentPos - mPreviousPos;
            mPreviousPos = mCurrentPos;
            mSlidingUiRectTrans.anchoredPosition = ClampGridPos(mSlidingUiRectTrans.anchoredPosition + delta);
        }

        void SetRotation()
        {
            if (mButtonToRotate == null) return;
            switch (mSlideDirection)
            {
                case SlideDirection.LeftToRight:
                    mButtonToRotate.eulerAngles = mIsOpen? Vector3.zero : new Vector3(0,0,180);
                    break;
                case SlideDirection.RightToLeft:
                    mButtonToRotate.eulerAngles = mIsOpen ? new Vector3(0, 0, 180) : Vector3.zero;
                    break;
                case SlideDirection.TopToBottom:
                    mButtonToRotate.eulerAngles = mIsOpen ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
                    break;
                case SlideDirection.BottomToTop:
                    mButtonToRotate.eulerAngles = mIsOpen ? new Vector3(0, 0, 270) : new Vector3(0, 0, 90);
                    break;
            }
        }

        void SetPositions()
        {
            mSlidingUiRectTrans = mSlidingUi.GetComponent<RectTransform>();
            mSlidingUiParentRectTrans = mSlidingUi.parent.GetComponent<RectTransform>();
            mStartPos = mSlidingUiRectTrans.anchoredPosition;
            switch (mSlideDirection)
            {
                case SlideDirection.LeftToRight:
                    mEndPos = new Vector2(mStartPos.x+ mMovePanelDelta, mStartPos.y);
                    break;
                case SlideDirection.RightToLeft:
                    mEndPos = new Vector2(mStartPos.x - mMovePanelDelta, mStartPos.y);
                    break;
                case SlideDirection.TopToBottom:
                    mEndPos = new Vector2(mStartPos.x, mStartPos.y- mMovePanelDelta);
                    break;
                case SlideDirection.BottomToTop:
                    mEndPos = new Vector2(mStartPos.x, mStartPos.y + mMovePanelDelta);
                    break;
            }
        }

        Vector2 GetAnchoredPositionFromWorldPoint(Vector2 worldPoint) 
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mSlidingUiParentRectTrans, screenPos, null, out localPos);
            return localPos;
        }
        #endregion
    }

    public enum SlideDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }
}