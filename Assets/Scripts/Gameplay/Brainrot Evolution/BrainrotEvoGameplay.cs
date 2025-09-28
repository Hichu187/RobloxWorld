using Cysharp.Threading.Tasks;
using Hichu;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class BrainrotEvoGameplay : BaseGameplay
    {
        [SerializeField] AssetReference gameView;
        [SerializeField] Transform _mapParent;

        private GameObject _currentMap;
        private void Awake()
        {
            _currentMap = FactoryBrainrotEvo.maps[DataBrainrotEvo.currentMap].Create(_mapParent);

        }

        public override void Start()
        {
            base.Start();

            Init();
        }
        private async void Init()
        {

            View view = await ViewHelper.PushAsync(gameView);
        }


        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Subscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Unsubscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
        }

        public void EventChangeMapSpace(Event_BrainrotEvo_Change_Space e)
        {
            LDebug.Log<BrainrotEvoGameplay>($"CHANGE MAP");
            SceneLoaderHelper.Reload();
        }

        public override async void EventPlayerDead(Event_Player_Dead e)
        {
            base.EventPlayerDead(e);

            await UniTask.WaitForSeconds(2);
            player.character.cCombat.ReSpawn();
            await UniTask.WaitForSeconds(1);
            player.character.cCombat.hasDied = false;

        }
    }
}
