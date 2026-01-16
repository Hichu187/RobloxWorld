using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class TowerGameplay : BaseGameplay
    {
        [SerializeField] AnimationSequence cameraSequence;
        [SerializeField] Transform cameraReviewMap;

        [SerializeField] AssetReferenceGameObject dropView;
        [SerializeField] List<TowerFloor> floors;

        public int curFloorID = 0;

        private bool _isShowingDropView;
        private int _dropViewGuardToken;

        public override void Start()
        {
            base.Start();

            player = Player.Instance;

            player.character.cCamera.SetFollowTransform(cameraReviewMap);
            player.gui.gameObject.SetActive(false);
        }

        public void InitStart()
        {
            cameraReviewMap.gameObject.SetActive(false);
            player.character.cCamera.SetFollowTransform(player.control._cameraFollowTarget);
            player.gui.gameObject.SetActive(true);
        }

        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();

            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
            StaticBus<Event_DropFloor>.Subscribe(EventDropFloor);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();

            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
            StaticBus<Event_DropFloor>.Unsubscribe(EventDropFloor);
        }

        public void EventPlayerDead(Event_Player_Dead e)
        {
            RevivePlayer();
        }

        public override void EventCheckpoint(Event_Checkpoint e)
        {
            base.EventCheckpoint(e);

            curCheckpoint = e.checkpoint;
            curCheckpoint.PlayFX();

            foreach (var f in floors)
            {
                if (f.checkpoints.Contains(curCheckpoint))
                    curFloorID = f.floorId;
            }

            Easypapa.EasypapaAdSdk.LogEvent($"slaptower_checkpoint_{checkpoints.IndexOf(curCheckpoint)}");
        }

        public void EventDropFloor(Event_DropFloor e)
        {
            _ = ShowDropViewOnceAsync();
        }

        private async Task ShowDropViewOnceAsync()
        {
            if (_isShowingDropView) return;

            _isShowingDropView = true;
            int token = ++_dropViewGuardToken;

            try
            {
                await ViewHelper.PushAsync(dropView);
            }
            finally
            {
                await Task.Delay(5000);
                if (token == _dropViewGuardToken)
                    _isShowingDropView = false;
            }
        }

        public void ReturnCheckPoint()
        {
            player.character.motor.SetPositionAndRotation(curCheckpoint.transform.position, curCheckpoint.transform.rotation);
            curCheckpoint = null;
        }

        public void ResetCurrent()
        {
            curCheckpoint = null;
            curFloorID = 0;
        }

        public async void RevivePlayer()
        {
            await Task.Delay(1000);

            player.character.Revive(startPosition.position, startPosition.rotation);
            player.character.motor.enabled = true;
        }
    }
}
