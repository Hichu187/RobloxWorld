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
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Unsubscribe(EventChangeMapSpace);
        }

        public void EventChangeMapSpace(Event_BrainrotEvo_Change_Space e)
        {
            LDebug.Log<BrainrotEvoGameplay>($"CHANGE MAP");
            SceneLoaderHelper.Reload();
        }
    }
}
