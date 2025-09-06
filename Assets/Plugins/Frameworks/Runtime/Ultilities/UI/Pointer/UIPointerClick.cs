using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hichu
{
    public class UIPointerClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action eventDown;
        public event Action eventUp;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            eventDown?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            eventUp?.Invoke();
        }
    }
}

