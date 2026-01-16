using UnityEngine;
using Hichu;
using DG.Tweening;
using Kcc;

namespace Game
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ButtonAction : MonoBehaviour, ICharacterCollidable
    {
        [Header("Refs")]
        [SerializeField] private Animator anim;
        [SerializeField] private GameObject target;

        [Header("Settings")]
        [SerializeField] private bool isMoveBack = false;

        [Header("Hide Target On Down")]
        [SerializeField, Min(0f)] private float hideDelay = 0.15f;
        [SerializeField, Min(0f)] private float showAfterHidden = 3.0f;

        private bool _hideInProgressOrHidden;
        private Tween _hideTween;

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character) { }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character) { MovingDown(); }
        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { MovingUp(); }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character) { }

        private void Awake()
        {
            EnsureAnimator();
        }

        private void Reset()
        {
            EnsureAnimator();
        }

        private void OnDisable()
        {
            _hideTween?.Kill();
            _hideTween = null;
            _hideInProgressOrHidden = false;
        }

        private void MovingDown()
        {
            if (anim != null) anim.SetBool("Up", false);

            if (target == null) return;
            if (_hideInProgressOrHidden) return;

            _hideInProgressOrHidden = true;

            _hideTween?.Kill();
            _hideTween = DOVirtual.DelayedCall(hideDelay, () =>
            {
                if (target != null) target.SetActive(false);

                _hideTween = DOVirtual.DelayedCall(showAfterHidden, () =>
                {
                    if (target != null) target.SetActive(true);
                    _hideInProgressOrHidden = false;
                }, false);
            }, false);
        }

        private void MovingUp()
        {
            if (anim != null) anim.SetBool("Up", true);
        }

        private void EnsureAnimator()
        {
            if (anim == null)
                anim = GetComponentInChildren<Animator>();
        }
    }
}
