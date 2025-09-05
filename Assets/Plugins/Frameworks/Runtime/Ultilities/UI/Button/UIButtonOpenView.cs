using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Hichu
{
    public class UIButtonOpenView : UIButtonBase
    {
        [Title("Config")]
        [SerializeField] protected AssetReferenceGameObject _view;

        public event Action<View> eventViewOpened;
        public float timeDelayOpen = 0;
        public override async void Button_OnClick()
        {
            base.Button_OnClick();

            await UniTask.WaitForSeconds(timeDelayOpen);

            View view = await ViewHelper.PushAsync(_view);

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            eventViewOpened?.Invoke(view);

            OnViewOpened(view);
        }

        protected virtual void OnViewOpened(View view)
        {

        }

        public void SetView(AssetReferenceGameObject view)
        {
            _view = view;
        }
    }
}
