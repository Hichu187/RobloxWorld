using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Announcement : MonoCached
    {
        [Title("Reference")]
        [SerializeField] private TextMeshProUGUI _txtMain;
        [SerializeField] private Image _tips;

        [Title("Config")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private Ease _fadeInEase = Ease.OutSine;

        [Space]

        [SerializeField] private float _fadeOutDelay = 2f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private Ease _fadeOutEase = Ease.OutSine;

        [Space]

        [SerializeField] private float _scaleDuration = 0.3f;
        [SerializeField] private Ease _scaleEase = Ease.OutSine;
        [SerializeField] private float _scaleValue = 1.1f;

        private CancellationTokenSource _cts;

        private CanvasGroup _canvasGroup;

        private Tween _tweenFadeIn;
        private Tween _tweenFadeOut;
        private Tween _tweenScale;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            InitTween();
        }

        private void OnDestroy()
        {
            _tweenFadeOut?.Kill();
            _tweenFadeIn?.Kill();
            _tweenScale?.Kill();

            _cts?.Cancel();
        }

        private void Start()
        {
            gameObjectCached.SetActive(false);
        }

        private void InitTween()
        {
            _tweenFadeOut = _canvasGroup.DOFade(0f, _fadeOutDuration)
                                        .ChangeStartValue(1.0f)
                                        .OnComplete(() => { gameObjectCached.SetActive(false); })
                                        .SetEase(_fadeOutEase)
                                        .SetAutoKill(false);

            _tweenFadeIn = _canvasGroup.DOFade(1.0f, _fadeInDuration)
                                       .ChangeStartValue(0f)
                                       .SetAutoKill(false)
                                       .SetEase(_fadeInEase);

            _tweenScale = _txtMain.transform.DOScale(_scaleValue, _scaleDuration * 0.5f)
                                            .SetAutoKill(false)
                                            .ChangeStartValue(Vector3.one)
                                            .SetLoops(2, LoopType.Yoyo)
                                            .SetEase(_scaleEase);

            if(_tips != null)
            _tweenScale = _tips.transform.DOScale(_scaleValue, _scaleDuration * 0.5f)
                                .SetAutoKill(false)
                                .ChangeStartValue(Vector3.one)
                                .SetLoops(2, LoopType.Yoyo)
                                .SetEase(_scaleEase);

        }

        public async UniTaskVoid PushMessageFadeIn(string msg)
        {
            gameObjectCached.SetActive(true);
            _txtMain.text = msg;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _tweenScale.Restart();
            _tweenScale.Pause();

            _tweenFadeIn.Restart();

            await _tweenFadeIn.Play().AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_cts.Token);

            await UniTask.WaitForSeconds(_fadeOutDelay, cancellationToken: _cts.Token);

            _tweenFadeOut.Restart();

            await _tweenFadeOut.Play().AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_cts.Token);
        }

        public async UniTaskVoid PushMessageScale(string msg)
        {
            gameObjectCached.SetActive(true);
            _txtMain.text = msg;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _tweenFadeIn.Restart();
            _tweenFadeIn.Complete();
            _tweenScale.Restart();

            await _tweenScale.Play().AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_cts.Token);

            await UniTask.WaitForSeconds(_fadeOutDelay, cancellationToken: _cts.Token);

            _tweenFadeOut.Restart();

            await _tweenFadeOut.Play().AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_cts.Token);
        }

        public async UniTaskVoid PushMesseage(string msg)
        {
            gameObjectCached.SetActive(true);
            _txtMain.text = msg;

            _tweenScale.Restart();
            _tweenScale.Pause();

            _tweenFadeIn.Restart();
            _tweenFadeIn.Complete();
            _tweenFadeIn.Pause();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            await UniTask.WaitForSeconds(_fadeOutDelay * 2, cancellationToken: _cts.Token);

            _tweenFadeOut.Restart();

            await _tweenFadeOut.Play().AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_cts.Token);
        }

        #region Test

#if UNITY_EDITOR

        [Button]
        private void TestFadeIn()
        {
            PushMessageFadeIn("Test fade in").Forget();
        }

        [Button]
        private void TestScale()
        {
            PushMessageScale("Test scale").Forget();
        }

        [Button]
        private void Test()
        {
            PushMesseage("Test normal").Forget();
        }

#endif

        #endregion
    }
}
