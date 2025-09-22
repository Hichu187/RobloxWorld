using Hichu;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class BrainrotEvoGameplay : BaseGameplay
    {
        [SerializeField] AssetReference gameView;

        private async void Start()
        {
            View view = await ViewHelper.PushAsync(gameView);
        }
    }
}
