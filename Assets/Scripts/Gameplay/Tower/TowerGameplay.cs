using DG.Tweening.Core.Easing;
using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;


namespace Game
{
    public class TowerGameplay : BaseGameplay
    {
        [SerializeField] AnimationSequence cameraSequence;
        [SerializeField] Transform cameraReviewMap;

        public override void Start()
        {
            base.Start();

            player = Player.Instance;

            player.control.canMove = false;

            player.character.cCamera.SetFollowTransform(cameraReviewMap);
            player.gui.gameObject.SetActive(false);

        }

        public void InitStart()
        {
            player.control.canMove = true;

            player.gui.gameObject.SetActive(true);
            player.character.cCamera.SetFollowTransform(player.control._cameraFollowTarget);
        }

        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();

            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();

            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
        }

        public void EventPlayerDead(Event_Player_Dead e)
        {
            RevivePlayer();
        }

        public async void RevivePlayer()
        {
            await Task.Delay(1000);

            player.character.Revive(startPosition.position, startPosition.rotation);
            player.character.motor.enabled = true;
        }
    }
}
