using Cysharp.Threading.Tasks;
using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class HomeScene : MonoBehaviour
    {
        [SerializeField] AssetReferenceGameObject _homeView;

        private View _view;

        private void Start()
        {
            OpenView();
        }

        public async void OpenView()
        {
            _view = await ViewHelper.PushAsync(_homeView);
        }
    }
}
