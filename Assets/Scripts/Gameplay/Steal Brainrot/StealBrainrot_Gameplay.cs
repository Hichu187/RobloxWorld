using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class StealBrainrot_Gameplay : BaseGameplay
    {
        [SerializeField] AssetReference gameView;
        public override void Start()
        {
            base.Start();

            Init();
        }
        private async void Init()
        {

            View view = await ViewHelper.PushAsync(gameView);
        }
    }
}
