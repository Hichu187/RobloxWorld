using DG.Tweening;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(RectTransform))]
    public class ButtonRank : MonoBehaviour
    {
        private RectTransform _rect;
        private Tweener _tween;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void Picked()
        {
            _tween?.Kill(); 
            _tween = _rect.DOAnchorPosX(75f, 0.25f)
                .SetEase(Ease.OutQuad);
        }

        public void UnPicked()
        {
            _tween?.Kill();
            _tween = _rect.DOAnchorPosX(115, 0.25f)
                .SetEase(Ease.OutQuad);
        }
    }
}
