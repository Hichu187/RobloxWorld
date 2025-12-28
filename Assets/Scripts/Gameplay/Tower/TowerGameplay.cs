using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Game
{
    public class TowerGameplay : BaseGameplay
    {
        [SerializeField] AnimationSequence cameraSequence;
        [SerializeField] Transform cameraReviewMap;

        [SerializeField] AssetReferenceGameObject dropView;

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

            Easypapa.AdHelper.ShowInterstitial("checkpoint");

            Easypapa.EasypapaAdSdk.LogEvent("slaptower_checkpoint", checkpoints.IndexOf(curCheckpoint));
        }

        public async void EventDropFloor(Event_DropFloor e)
        {
            View drop = await ViewHelper.PushAsync(dropView);
        }

        public void ReturnCheckPoint()
        {
            player.character.motor.SetPositionAndRotation(curCheckpoint.transform.position, curCheckpoint.transform.rotation);
        }

        public void ResetCurrent()
        {
            curCheckpoint = null;
        }

        public async void RevivePlayer()
        {
            await Task.Delay(1000);

            player.character.Revive(startPosition.position, startPosition.rotation);
            player.character.motor.enabled = true;
        }
    }
}
