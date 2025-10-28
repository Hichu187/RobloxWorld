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

        public override async void EventPlayerDead(Event_Player_Dead e)
        {
            await Task.Delay(1000);
            RespawnCheckpoint();
        }
    }
}
