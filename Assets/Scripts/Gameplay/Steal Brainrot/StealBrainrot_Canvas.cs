using DG.Tweening;
using Hichu;
using TMPro;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Canvas : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cashText;
        [SerializeField] private float _tweenDuration = 0.35f;
        [SerializeField] private float _tweenDelay = 0.35f;

        private Tweener _cashTween;
        private int _displayedCash;
        private int _lastTarget = int.MinValue;

        private void Start()
        {
            StaticBus<Event_Cash_Update>.Subscribe(EventCashUpdate);

            _displayedCash = DataStealBrainrot.cash;
            _cashText.text = _displayedCash.ToString();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Cash_Update>.Unsubscribe(EventCashUpdate);
            _cashTween?.Kill();
        }

        private void EventCashUpdate(Event_Cash_Update e)
        {
            CashUpdate(e.encreaseCash);
        }

        public void CashUpdate(bool isIncrease)
        {
            int target = DataStealBrainrot.cash;

            if (target == _displayedCash || target == _lastTarget)
            {
                if (_cashText) _cashText.text = target.ToString();
                return;
            }

            _lastTarget = target;

            if (_cashTween == null)
            {
                _cashTween = DOTween.To(() => _displayedCash, v =>
                {
                    _displayedCash = v;
                    if (_cashText) _cashText.text = v.ToString();
                }, target, _tweenDuration)
                .SetEase(Ease.OutCubic)
                .SetAutoKill(false)
                .Pause();
            }
            else
            {
                _cashTween.ChangeEndValue(target, true);
            }

            _cashTween.SetDelay(isIncrease ? _tweenDelay : 0f);
            _cashTween.Restart();
        }
    }
}
