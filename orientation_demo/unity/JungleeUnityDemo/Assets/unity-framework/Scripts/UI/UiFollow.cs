using UnityEngine;

namespace XcelerateGames.UI
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class UiFollow : MonoBehaviour
    {
        public enum Side
        {
            Right,
        }
        public RectTransform _Target;
        public Vector3 _Offset;
        public Side _Side;

        private RectTransform mTransform;

        void Start()
        {
            mTransform = GetComponent<RectTransform>();
        }

        void LateUpdate()
        {
            if (_Target == null)
                return;
            Vector3 rect = _Target.position;
            if(_Side == Side.Right)
            {
                rect.x += _Target.rect.max.x + _Offset.x + mTransform.rect.width;
                rect.y += _Target.rect.max.y + _Offset.y + mTransform.rect.height;
            }
            mTransform.position = rect;
        }
    }
}
