using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hichu
{
    public class UIPointerDrag : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<PointerEventData> eventTouch;
        public event Action<PointerEventData> eventDragBegin;
        public event Action<PointerEventData> eventDrag;
        public event Action<PointerEventData> eventDragEnd;
        public event Action<PointerEventData> eventTouchEnd;



        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventTouch != null)
                eventTouch.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventTouchEnd != null)
                eventTouchEnd.Invoke(eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            eventDragBegin?.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            eventDrag?.Invoke(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            eventDragEnd?.Invoke(eventData);
        }
    }
}
