using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class GrowObbyPlayer : MonoBehaviour
    {
        [Title("Character")]
        [SerializeField] private Transform target;

        [Title("Scale Control")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private float scaleFactor = 1f;
        private Tween _scaleTween;

        private void Awake()
        {
            if (target == null) target = transform;
        }

        [Button]
        public void SetScale(float scale)
        {
            scaleFactor = scale;

            if (_scaleTween != null && _scaleTween.IsActive())
                _scaleTween.Kill();

            _scaleTween = target.DOScale(Vector3.one * scale, duration)
                                .SetEase(ease);
        }
    }
}
