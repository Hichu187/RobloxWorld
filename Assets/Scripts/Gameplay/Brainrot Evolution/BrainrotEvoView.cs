using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hichu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BrainrotEvoView : MonoBehaviour
    {
        [SerializeField] private Slider _transformProgress;
        [SerializeField] private TextMeshProUGUI _expText;
        [SerializeField] private TextMeshProUGUI _cashText;
        [SerializeField] private float _tweenDuration = 0.35f;   // thời gian tween slider

        private Tween _expTween;
        private Tween _cashTween;

        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);
            StaticBus<Event_Cash_Update>.Subscribe(EventCashUpdate);
            InitProgress();

            CashUpdate();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
            StaticBus<Event_Cash_Update>.Unsubscribe(EventCashUpdate);
            _cashTween?.Kill();
        }

        private async void EventAddExp(Event_Player_Add_Exp e)
        {
            await UniTask.WaitForEndOfFrame();
            UpdateExpUI(true);
        }

        private async void EventLevelUp(Event_Player_Level_Up e)
        {
            _expTween?.Kill();
            _transformProgress.value = 0;

            await UniTask.WaitForEndOfFrame();
            UpdateExpUI(false);
        }

        private void EventCashUpdate(Event_Cash_Update e)
        {
            CashUpdate();
        }

        public void InitProgress()
        {
            _expTween?.Kill();
            UpdateExpUI(false);
        }

        private void UpdateExpUI(bool animate)
        {
            int curExp = DataBrainrotEvo.exp;
            int maxExp = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level].exp;

            _expText.text = $"{curExp}/{maxExp}";

            float targetValue = maxExp > 0 ? (float)curExp / maxExp : 0f;

            if (animate)
            {
                _expTween?.Kill();
                _expTween = _transformProgress.DOValue(targetValue, _tweenDuration).SetEase(Ease.OutCubic);
            }
            else
            {
                _transformProgress.value = targetValue;
            }
        }

        public void CashUpdate()
        {
            int target = DataBrainrotEvo.cash;

            int start = 0;
            if (!string.IsNullOrEmpty(_cashText.text))
            {
                int.TryParse(_cashText.text, out start);
            }

            if (start == target)
            {
                _cashText.text = target.ToString();
                return;
            }

            _cashTween?.Kill();
            int val = start;

            _cashTween = DOTween
                .To(() => val, v =>
                {
                    val = v;
                    _cashText.text = v.ToString();
                }, target, _tweenDuration)
                .SetEase(Ease.OutCubic);
        }
    }
}
