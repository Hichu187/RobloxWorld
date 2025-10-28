using Cysharp.Threading.Tasks;
using Hichu;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game
{
    public abstract class BaseGameplay : MonoBehaviour
    {
        public Player player;
        public Transform startPosition;
        public PlatformCheckpoint curCheckpoint;

        public virtual void Start()
        {
            player.character.motor.SetPositionAndRotation(startPosition.position, startPosition.rotation);
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

            Debug.Log($"{curCheckpoint.name} - {curCheckpoint.transform.localPosition}");

            player.character.Revive(curCheckpoint.transform.localPosition, curCheckpoint.transform.localRotation);
            player.character.motor.enabled = true;
            player.control.canMove = true;
        }

        public virtual async void EventPlayerDead(Event_Player_Dead e)
        {
            await UniTask.WaitForSeconds(2);

        }

        private void EventCheckpoint(Event_Checkpoint e)
        {
            curCheckpoint = e.checkpoint;
        }
    }
}
