using Hichu;
using UnityEngine;

namespace Game
{
    public abstract class BaseGameplay : MonoBehaviour
    {
        public PlatformCheckpoint _curCheckpoint;
        private void Start()
        {
            SubscribeEvent();
        }

        private void OnDestroy()
        {
            UnsubscribeEvent();
        }

        protected virtual void SubscribeEvent()
        {
            StaticBus<Event_Checkpoint>.Subscribe(EventCheckpoint);
        }

        protected virtual void UnsubscribeEvent()
        {
            StaticBus<Event_Checkpoint>.Unsubscribe(EventCheckpoint);
        }

        public void EventCheckpoint(Event_Checkpoint e)
        {
            _curCheckpoint = e.checkpoint;
        }
    }
}
