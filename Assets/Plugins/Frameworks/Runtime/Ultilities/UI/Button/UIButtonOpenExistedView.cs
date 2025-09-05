using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Hichu
{
    public class UIButtonOpenExistedView : UIButtonBase
    {
        [Title("Config")]
        [SerializeField] protected View _view;
        [SerializeField] GameObject _notice;
        public event Action<View> eventViewOpened;

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            if(_view == null) return;
            _view.Open();

            if (_notice == null) return;
            _notice.gameObject.SetActive(false);
        }

        protected virtual void OnViewOpened(View view)
        {
        }

        public void SetView(AssetReferenceGameObject view)
        {

        }
        public void EnableNotice()
        {
            if (_notice == null) return;
            _notice.SetActive(true);
        }
    }
}
