using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class PlatformFade : MonoCached, ICharacterCollidable
    {
        private static int s_triggerIndex = 0;

        [Title("Reference")]
        [SerializeField] private MeshRenderer _renderer;

        [Title("Config")]
        [SerializeField] private float _fadeDuration = 0.75f;
        [SerializeField] private float _appearDelay = 2f;
        [SerializeField] private AudioConfig[] _sfxTrigger;

        private Tween _tween;

        private Material _materialOrigin;

        private void Start()
        {
            _materialOrigin = _renderer.sharedMaterial;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            FadeOut();
        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            FadeOut();
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {
        }

        private void FadeOut()
        {
            if (_tween.IsActive())
                return;

            Color colorStart = _materialOrigin.color;
            Color colorEnd = _materialOrigin.color;
            colorEnd.a = 0f;

            Material materialReplace = new Material(_materialOrigin);

            _renderer.material = materialReplace;

            _tween = materialReplace.DOColor(colorEnd, _fadeDuration)
                                    .OnComplete(OnFadeComplete)
                                    .ChangeStartValue(colorStart);

            // Play sfx
            //AudioManager.Play(_sfxTrigger.GetLoop(s_triggerIndex)).transformCached.position = transformCached.position;
            //s_triggerIndex++;
        }

        private void OnFadeComplete()
        {
            gameObjectCached.SetActive(false);

            _renderer.material = _materialOrigin;

            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(_appearDelay, () => { gameObjectCached.SetActive(true); }, false);
        }
    }
}
