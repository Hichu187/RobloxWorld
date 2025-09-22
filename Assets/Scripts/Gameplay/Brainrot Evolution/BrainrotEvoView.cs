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
        [SerializeField] private float _tweenDuration = 0.35f;   // thời gian tween slider

        private Tween _expTween;

        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);
            InitProgress();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
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
    }
}
