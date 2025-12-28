using Cysharp.Threading.Tasks;
using Easypapa;
using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class HomeScene : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject _homeView;

        private View _view;
        private static bool s_loggedThisLaunch = false;

        private void Start()
        {
            //LogAppLaunch();
            OpenView();

        }

        private void LogAppLaunch()
        {
            if (s_loggedThisLaunch)
                return;

            s_loggedThisLaunch = true;

            Easypapa.AdHelper.ShowAppOpen();
        }

        public async void OpenView()
        {
            EasypapaAdSdk.InitOnStartup();

            await UniTask.WaitForSeconds(3);

            _view = await ViewHelper.PushAsync(_homeView);
        }
    }
}
