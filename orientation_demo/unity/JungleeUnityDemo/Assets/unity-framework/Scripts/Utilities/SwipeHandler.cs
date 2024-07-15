using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public enum SwipeDirection : byte
    {
        None,
        Up,
        Down,
        Right,
        Left
    }

    public class SwipeHandler : BaseBehaviour
    {
        public enum Axis : byte
        {
            None,
            Horizontal,
            Vertical,
            Both
        }

        [SerializeField] private Axis _AllowedDirection = Axis.Both;
        [SerializeField] private float _MaxSwipeTime = 0.5f;
        [SerializeField] private float _MinSwipeDistance = 0.17f;

        [InjectSignal] private SigOnSwipe mSigOnSwipe = null;

        private float mMinDistanceSqr;
        private Vector2 mStartPos;
        private float mStartTime;

        protected override void Awake()
        {
            base.Awake();
            mMinDistanceSqr = _MinSwipeDistance * _MinSwipeDistance;
            if (_AllowedDirection == Axis.None)
            {
                enabled = false;
                XDebug.LogError($"SwipeHandler:: _AllowedDirection must not be 'None'. Please change it's value");
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mStartPos = Input.mousePosition;
                mStartPos.x /= (float)Screen.width;
                mStartPos.y /= (float)Screen.height;
                mStartTime = Time.time;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (Time.time - mStartTime > _MaxSwipeTime) // press too long
                    return;
                Vector2 endPosition = Input.mousePosition;
                endPosition.x /= (float)Screen.width;
                endPosition.y /= (float)Screen.height;
                Vector2 distance = endPosition - mStartPos;
                if (distance.sqrMagnitude < mMinDistanceSqr)
                    return;
                SwipeDirection direction = GetDirection(distance);
                if (direction != SwipeDirection.None)
                {
                    mSigOnSwipe.Dispatch(direction);
                }
            }
        }

        private SwipeDirection GetDirection(Vector3 distance)
        {
            float positiveX = Mathf.Abs(distance.x);
            float positiveY = Mathf.Abs(distance.y);
            if (_AllowedDirection == Axis.Both)
            {
                if (positiveX > positiveY)
                    return (distance.x > 0) ? SwipeDirection.Right : SwipeDirection.Left;
                else
                    return (distance.y > 0) ? SwipeDirection.Up : SwipeDirection.Down;
            }
            else if (_AllowedDirection == Axis.Horizontal)
            {
                if (positiveX > positiveY)
                    return (distance.x > 0) ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else if (_AllowedDirection == Axis.Vertical)
            {
                if (positiveX <= positiveY)
                    return (distance.y > 0) ? SwipeDirection.Up : SwipeDirection.Down;
            }
            return SwipeDirection.None;
        }
    }
}