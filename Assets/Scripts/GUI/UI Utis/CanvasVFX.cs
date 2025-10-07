using Hichu;
using UnityEngine;

namespace Game
{
    public class CanvasVFX : MonoBehaviour
    {
        [SerializeField] ParticleSystem cashVfx;
        private void Start()
        {
            StaticBus<Event_Cash_Update>.Subscribe(EventCashUpdate);
        }

        private void OnDestroy()
        {
            StaticBus<Event_Cash_Update>.Unsubscribe(EventCashUpdate);
        }

        private void EventCashUpdate(Event_Cash_Update e)
        {
            if (e.encreaseCash)
            {
                cashVfx.gameObject.SetActive(true);
                cashVfx.Play();
            }
        }
    }
}
