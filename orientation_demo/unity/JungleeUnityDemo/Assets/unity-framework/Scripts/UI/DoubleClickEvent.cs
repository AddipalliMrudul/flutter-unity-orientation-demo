using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace XcelerateGames.UI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Event/DoubleClickEvent")]
    public class DoubleClickEvent : MonoBehaviour, IPointerClickHandler
    {
        [System.Serializable]
        public class DoubleClick : UnityEvent { }
        public DoubleClick OnDoubleClick;
        private float mLastClickTime = 0;

        void Start()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if ((Time.timeSinceLevelLoad - mLastClickTime) < 0.25f)
                OnDoubleClick.Invoke();
            else
                mLastClickTime = Time.timeSinceLevelLoad;
        }
    }
}