using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hichu;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class  Event_Buff_Countdown_Start: IEvent
    {
        public int durationSeconds;

        public Event_Buff_Countdown_Start(int durationSeconds)
        {
            this.durationSeconds = durationSeconds;
        }
    }

    public class Event_Buff_Countdown_End : IEvent { }

    public class BrainrotEvoView : MonoBehaviour
    {
        [SerializeField] private Slider _transformProgress;
        [SerializeField] private Image _brainrotImage;
        [SerializeField] private TextMeshProUGUI _expText;
        [SerializeField] private TextMeshProUGUI _cashText;
        [SerializeField] private float _tweenDuration = 0.35f;
        [SerializeField] private GameObject buffNotice;

        [Header("Buff")]
        [SerializeField] private Button btn_Buff;
        [SerializeField] private TextMeshProUGUI buffCountdownText;
        [SerializeField] private int buffCountdownSeconds = 60;

        private Tween _expTween;
        private Tween _cashTween;

        private CancellationTokenSource _buffCts;

        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);
            StaticBus<Event_Cash_Update>.Subscribe(EventCashUpdate);

            InitProgress();
            CashUpdate();

            if (DataBrainrotEvo.boostFree)
            {
                buffNotice.SetActive(true);
            }

            if (buffCountdownText != null)
                buffCountdownText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
            StaticBus<Event_Cash_Update>.Unsubscribe(EventCashUpdate);

            _cashTween?.Kill();
            _expTween?.Kill();

            _buffCts?.Cancel();
            _buffCts?.Dispose();
            _buffCts = null;
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

        private async void EventCashUpdate(Event_Cash_Update e)
        {
            if (e.encreaseCash)
                await UniTask.WaitForSeconds(1f);

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

            _brainrotImage.sprite = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level + 1].texture;
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
                int.TryParse(_cashText.text, out start);

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

        public void BuffActive()
        {
            DataBrainrotEvo.BoostFree();

            _buffCts?.Cancel();
            _buffCts?.Dispose();
            _buffCts = new CancellationTokenSource();

            RunBuffCountdownAsync(buffCountdownSeconds, _buffCts.Token).Forget();
        }

        private async UniTaskVoid RunBuffCountdownAsync(int seconds, CancellationToken token)
        {
            StaticBus<Event_Buff_Countdown_Start>.Post(new Event_Buff_Countdown_Start(seconds));

            if (btn_Buff != null)
                btn_Buff.interactable = false;

            if (buffCountdownText != null)
            {
                buffCountdownText.gameObject.SetActive(true);
                buffCountdownText.text = Mathf.Max(0, seconds).ToString();
            }

            buffNotice.SetActive(false);

            int remain = Mathf.Max(0, seconds);
            while (remain > 0)
            {
                token.ThrowIfCancellationRequested();

                if (buffCountdownText != null)
                    buffCountdownText.text = remain.ToString();

                await UniTask.Delay(1000, DelayType.DeltaTime, PlayerLoopTiming.Update, token);
                remain--;
            }

            if (buffCountdownText != null)
            {
                buffCountdownText.text = "0";
                buffCountdownText.gameObject.SetActive(false);
            }

            buffNotice.SetActive(true);

            if (btn_Buff != null)
                btn_Buff.interactable = true;

            StaticBus<Event_Buff_Countdown_End>.Post(new Event_Buff_Countdown_End());
        }
    }
}
