using DG.Tweening.Core.Easing;
using Hichu;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;


namespace Game
{
    public class TowerGameplay : BaseGameplay
    {
        [SerializeField] AnimationSequence cameraSequence;
        [SerializeField] Transform cameraReviewMap;

        private void Start()
        {
            player = Player.Instance;

            //player.character.cCamera.SetFollowTransform(cameraReviewMap);
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
