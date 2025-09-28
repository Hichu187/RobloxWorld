using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game
{
    public class UINotificationText : MonoCached
    {
        static UINotificationText[] _pool = new UINotificationText[1];
        static int _poolIndex = 0;

        [Header("Reference")]
        [SerializeField] TextMeshProUGUI _txtMain;

        [Header("Config")]
        [SerializeField] float _fadeInDuration = 0.35f;
        [SerializeField] float _scaleDuration = 0.25f;
        [SerializeField] Ease _scaleEase = Ease.Linear;
        [MinMaxSlider(0f, 1f, ShowFields = true)]
        [SerializeField] Vector2 _anchorY = new Vector2(0.7f, 0.75f);
        [SerializeField] float _moveDuration = 0.4f;
        [SerializeField] Ease _moveEase = Ease.Linear;
        [SerializeField] float _fadeOutDelay = 0.5f;
        [SerializeField] float _fadeOutDuration = 0.9f;

        Sequence _sequence;

        #region MonoBehaviour

        void OnDestroy()
        {
            _sequence?.Kill();
        }

        #endregion

        public void Show(string msg)
        {
            _txtMain.text = msg;

            InitSequence();

            _sequence.Restart();
            _sequence.Play();
        }

        void InitSequence()
        {
            if (_sequence != null)
                return;

            RectTransform target = transformCached.GetChild(0).GetComponent<RectTransform>();
            float rectHeight = transformCached.GetComponent<RectTransform>().rect.height;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            target.SetAnchoredPositionY(rectHeight * _anchorY.x);
            target.SetScale(0f);

            _sequence = DOTween.Sequence();
            _sequence.Append(target.DOScale(1f, _scaleDuration).SetEase(_scaleEase));
            _sequence.Join(FadeCanvasGroup(canvasGroup, 0f, 1f, _fadeInDuration));
            _sequence.Join(target.DOAnchorPosY(rectHeight * _anchorY.y, _moveDuration).SetEase(_moveEase));
            _sequence.AppendInterval(_fadeOutDelay);
            _sequence.Append(FadeCanvasGroup(canvasGroup, 1f, 0f, _fadeOutDuration));

            _sequence.AppendCallback(() => { gameObjectCached.SetActive(false); });

            _sequence.SetAutoKill(false);
            _sequence.SetUpdate(true);
        }

        Tween FadeCanvasGroup(CanvasGroup canvas, float from, float to, float duration)
        {
            float alpha = from;
            return DOTween.To(() => alpha, (x) => { canvas.alpha = x; }, to, duration);
        }

        public static void Push(string msg)
        {
            if (_poolIndex >= _pool.Length)
                _poolIndex = 0;

            if (_pool[_poolIndex] == null)
            {
                _pool[_poolIndex] = FactoryPrefab.uiNotificationText.Create().GetComponent<UINotificationText>();
            }

            _pool[_poolIndex].transformCached.SetParent(ViewContainer.Instance.TransformCached, false);
            _pool[_poolIndex].transformCached.SetAsLastSibling();
            _pool[_poolIndex].gameObjectCached.SetActive(true);
            _pool[_poolIndex].Show(msg);

            _poolIndex++;
        }
    }
}