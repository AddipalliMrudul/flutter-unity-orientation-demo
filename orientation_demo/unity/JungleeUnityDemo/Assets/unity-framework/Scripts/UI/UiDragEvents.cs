using UnityEngine;
using UnityEngine.EventSystems;

namespace XcelerateGames.UI
{
    public interface IDragEvents
    {
        void OnPointerEntered(PointerEventData eventData, bool isDragging);
        void OnBeginDrag(PointerEventData eventData);
        void OnDrag(PointerEventData eventData);
        void OnEndDrag(PointerEventData eventData);
    }

    public class UiDragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler
    {
        public IDragEvents mListener = null;

        public void Init(IDragEvents dragEventListener)
        {
            mListener = dragEventListener;
        }

        private static bool IsDragging = false;
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            mListener.OnPointerEntered(eventData, IsDragging);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
            mListener.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            mListener.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
            mListener.OnEndDrag(eventData);
        }
    }
}