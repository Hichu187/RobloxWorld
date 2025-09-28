using Cysharp.Threading.Tasks;
using Hichu;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public abstract class BaseGameplay : MonoBehaviour
    {
        public Player player;
        public Transform startPosition;
        public PlatformCheckpoint curCheckpoint;

        public virtual void Start()
        {
            SubscribeEvent();
        }

        public virtual void OnDestroy()
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

        public void RespawnStartPosition()
        {
            if (startPosition == null) return;
            if (player == null) return;
            player.character.Revive(startPosition.transform.position, startPosition.transform.rotation);
        }

        public void RespawnCheckpoint()
        {
            if (curCheckpoint == null) return;
            if (player == null) return;

            player.character.Revive(curCheckpoint.transform.position, curCheckpoint.transform.rotation);
        }

        public virtual async void EventPlayerDead(Event_Player_Dead e)
        {
            await UniTask.WaitForSeconds(2);

            player.character.Revive(transform.position + Vector3.up, transform.rotation);
            player.control.canMove = true;
        }

        private void EventCheckpoint(Event_Checkpoint e)
        {
            curCheckpoint = e.checkpoint;
        }
    }
}
