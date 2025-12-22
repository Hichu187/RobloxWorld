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
        protected View view;
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
            view = await ViewHelper.PushAsync(gameView);
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

        public void GotoNextCheckPoint()
        {
            if (player == null) return;
            if (checkpoints == null || checkpoints.Count == 0) return;

            int curIndex = CurrentCheckpointIndex();

            int nextIndex = Mathf.Clamp(curIndex + 1, 0, checkpoints.Count - 1);

            PlatformCheckpoint nextCheckpoint = checkpoints[nextIndex];
            if (nextCheckpoint == null) return;

            curCheckpoint = nextCheckpoint;

            var pos = curCheckpoint.transform.position;
            var rot = curCheckpoint.transform.rotation;

            if (player.character != null && player.character.motor != null)
            {
                player.character.motor.SetPositionAndRotation(pos, rot);
            }
            else
            {
                player.transform.SetPositionAndRotation(pos, rot);
            }

            if (player.character != null && player.character.motor != null)
                player.character.motor.enabled = true;

            if (player.control != null)
                player.control.canMove = true;

            Debug.Log($"GotoNextCheckPoint -> {curCheckpoint.name} ({nextIndex}/{checkpoints.Count - 1})");
        }

    }
}
