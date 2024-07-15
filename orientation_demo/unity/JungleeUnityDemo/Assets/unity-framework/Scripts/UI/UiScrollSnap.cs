using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    /*Add this component to ScrollView parent(object having ScrollRect component)
     * For this component to work, the content should have:
      - either have a vertical or a horizontal layout group
      - more than 1 children 
     */
    public class UiScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private enum ScrollDirection
        {
            Horizontal,
            Vertical
        }

        [SerializeField]
        private float _SnapSpeed = 2;

        [Header("Auto Scroll Setup")]
        [SerializeField]
        private bool _AutoScroll = false;

        [SerializeField]
        private int _AutoScrollInterval = 2;

        private Vector3 mCentrePoint;
        private List<GameObject> mChildren = new List<GameObject>();
        private float[] mDistancesFromCentrePoint = null;
        private float mDimentionAlongRespectiveAxis;
        private float mDestinationPos;
        private int mNumOfChildren;
        private int mIndexOfChildAtFocus;
        private bool mIsDragging;
        private float mSpacing;
        private ScrollDirection mScrollDirection = ScrollDirection.Horizontal;
        private ScrollRect mScrollRect = null;
        private RectTransform mContent = null;
        private Coroutine mAutoScrollRoutine;
        private Vector2 mNewPos = Vector2.zero;
        private bool mIsInitialized = false;

        private void OnEnable()
        {
            if (_AutoScroll && mNumOfChildren > 2 && mContent != null && mScrollRect)
            {
                if (mAutoScrollRoutine != null)
                    StopCoroutine(mAutoScrollRoutine);
                mAutoScrollRoutine = StartCoroutine(updateChildAtFocus());
            }
        }

        //Initialize this after successfully loading the data or call this is a Start(to be created) function
        public void Init()
        {
            mScrollRect = GetComponent<ScrollRect>();
            mContent = mScrollRect.content;
            mNumOfChildren = mContent.childCount;
            var layOutGroup = mContent.GetComponent<HorizontalOrVerticalLayoutGroup>();

            // no point of having a scroll snap
            if (mNumOfChildren < 2 || layOutGroup == null)
            {
                this.enabled = false;
                return;
            }

            mCentrePoint = mScrollRect.transform.position;
            if (layOutGroup is HorizontalLayoutGroup)
            {
                mScrollDirection = ScrollDirection.Horizontal;
                mDimentionAlongRespectiveAxis = mContent.GetChild(0).GetComponent<RectTransform>().rect.width;
            }
            else
            {
                mScrollDirection = ScrollDirection.Vertical;
                mDimentionAlongRespectiveAxis = mContent.GetChild(0).GetComponent<RectTransform>().rect.height;
            }

            mSpacing = mContent.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing;
            mIndexOfChildAtFocus = 0;

            foreach (Transform child in mContent)
            {
                mChildren.Add(child.gameObject);
            }

            mDistancesFromCentrePoint = new float[mNumOfChildren];
            mIsInitialized = true;
            if (mAutoScrollRoutine != null)
                StopCoroutine(mAutoScrollRoutine);
            mAutoScrollRoutine = StartCoroutine(updateChildAtFocus());
        }

        IEnumerator updateChildAtFocus()
        {
            if (mIsDragging)
                yield break;

            yield return new WaitForSeconds(_AutoScrollInterval);

            if (mIndexOfChildAtFocus == mNumOfChildren - 1)
            {
                mIndexOfChildAtFocus = 0;
                mScrollRect.velocity = Vector2.zero;
                mIsDragging = false;
            }
            else
                mIndexOfChildAtFocus++;

            float multiplier = 1;
            if (mScrollDirection == ScrollDirection.Horizontal)
                multiplier = -1;
            mDestinationPos = multiplier * mIndexOfChildAtFocus * (mDimentionAlongRespectiveAxis + mSpacing);

            if (_AutoScroll)
            {
                if (mAutoScrollRoutine != null)
                    StopCoroutine(mAutoScrollRoutine);
                mAutoScrollRoutine = StartCoroutine(updateChildAtFocus());
            }
        }

        void updateChildAtFocus(int indexOfNearestChild)
        {
            if (mIsDragging)
                return;

            mIndexOfChildAtFocus = indexOfNearestChild;

            if (mScrollDirection == ScrollDirection.Horizontal)
                mDestinationPos = -1 * mIndexOfChildAtFocus * (mDimentionAlongRespectiveAxis + mSpacing);
            else
                mDestinationPos = 1 * mIndexOfChildAtFocus * (mDimentionAlongRespectiveAxis + mSpacing);
            //Debug.Log($"pos x is: {mDestinationPos}");

            if (_AutoScroll)
            {
                if (mAutoScrollRoutine != null)
                    StopCoroutine(mAutoScrollRoutine);
                mAutoScrollRoutine = StartCoroutine(updateChildAtFocus());
            }
        }

        void SnapToTheNearestChild()
        {
            float sourcePos = 0;
            if (mScrollDirection == ScrollDirection.Horizontal)
                sourcePos = mContent.anchoredPosition.x;
            else
                sourcePos = mContent.anchoredPosition.y;

            float mRespectiveAxisPos = Mathf.Lerp(sourcePos, mDestinationPos, Time.deltaTime * _SnapSpeed);
            if (mScrollDirection == ScrollDirection.Horizontal)
            {
                mNewPos.x = mRespectiveAxisPos;
                mNewPos.y = 0;
            }
            else
            {
                mNewPos.x = 0;
                mNewPos.y = mRespectiveAxisPos;
            }

            mContent.anchoredPosition = mNewPos;
        }

        void Update()
        {
            if (mIsDragging || !mIsInitialized)
                return;

            if (mScrollRect.velocity.x != 0 || mScrollRect.velocity.y != 0) // so Scroll is moving
            {
                updateChildAtFocus(getNearestChildIndex());
            }

            if (_AutoScroll && mIndexOfChildAtFocus == 0)
            {
                if (mScrollRect.velocity.x < 0.00000001f || mScrollRect.velocity.y < 0.000000001f)
                {
                    mContent.anchoredPosition = Vector2.zero;
                    mScrollRect.velocity = Vector2.zero;
                }
            }
            SnapToTheNearestChild();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            mIsDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (mNumOfChildren < 2)
                return;
            mIsDragging = false;
            //Debug.Log($"getNearestChildIndex: {getNearestChildIndex()}");
        }

        private int getNearestChildIndex()
        {
            int? nearestChildIndex = null;
            for (int i = 0; i < mChildren.Count; i++)
            {
                if (mScrollDirection == ScrollDirection.Horizontal)
                    mDistancesFromCentrePoint[i] = Mathf.Abs(mCentrePoint.x - mChildren[i].transform.position.x);
                else
                    mDistancesFromCentrePoint[i] = Mathf.Abs(mCentrePoint.y - mChildren[i].transform.position.y);
            }
            float minDistance = mDistancesFromCentrePoint.Min();

            for (int i = 0; i < mDistancesFromCentrePoint.Length; i++)
            {
                if (minDistance == mDistancesFromCentrePoint[i])
                    nearestChildIndex = i;
            }

            if (nearestChildIndex == null)
                Debug.LogError("Something went wrong, nearest child index cannot be wrong");

            return nearestChildIndex.Value;
        }
    }
}
