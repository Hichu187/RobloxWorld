using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hichu
{
    public class UIPointerEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action eventEnter;
        public event Action eventExit;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (eventEnter != null)
            {
                eventEnter?.Invoke();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if(eventExit != null)
            {
                eventExit?.Invoke();
            }
        }
    }
}
