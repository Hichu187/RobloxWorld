using Cysharp.Threading.Tasks;
using Hichu;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;

namespace Game
{
    public abstract class BaseGameplay : MonoBehaviour
    {
        public Player player;
        public Transform startPosition;
        public PlatformCheckpoint curCheckpoint;
        public AssetReference gameView;

        public List<PlatformCheckpoint> checkpoints;

        public virtual void Start()
        {
            if(startPosition != null) 
                player.character.motor.SetPositionAndRotation(startPosition.position, startPosition.rotation);

            SubscribeEvent();

            Init();
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

        private async void Init()
        {
            if (gameView == null) return;
            View view = await ViewHelper.PushAsync(gameView);
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

            player.character.Revive(curCheckpoint.transform.position, curCheckpoint.transform.rotation);
            player.character.motor.enabled = true;
            player.control.canMove = true;
        }

        public virtual async void EventPlayerDead(Event_Player_Dead e)
        {
            await UniTask.WaitForSeconds(2);

        }

        public virtual void EventCheckpoint(Event_Checkpoint e)
        {

        }

        public int CurrentCheckpointIndex()
        {
            if (curCheckpoint == null)
                return 0;

            return checkpoints.IndexOf(curCheckpoint);
        }
    }
}
