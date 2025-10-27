using Hichu;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class EasyObbyGameplay : BaseGameplay
    {
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

            player.character.Revive(curCheckpoint.transform.position, curCheckpoint.transform.rotation);
            player.character.motor.enabled = true;
        }
    }
}
