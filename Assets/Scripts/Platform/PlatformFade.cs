using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class PlatformFade : MonoCached, ICharacterCollidable
    {
        private static int s_triggerIndex = 0;

        [Title("Reference")]
        [SerializeField] private MeshRenderer _renderer;

        [Title("Config")]
        [SerializeField, Min(0f)] private float _fadeDuration = 0.75f;
        [SerializeField, Min(0f)] private float _appearDelay = 2f;

        [SerializeField] private AudioConfig[] _sfxTrigger;


        private Material _materialOrigin;
        private Tween _tween;

        private void Awake()
        {

        }

        [Button]
        void GetMesh()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();

            if (_renderer == null)
                _renderer = GetComponentInChildren<MeshRenderer>();

            if (_renderer != null)
                _materialOrigin = _renderer.sharedMaterial;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            FadeOut(character.GetComponent<Character>().isPlayer);
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character) { }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            FadeOut(character.GetComponent<Character>().isPlayer);
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { }

        private void FadeOut(bool isPlayer)
        {
            if (_tween.IsActive())
                return;

            Color colorStart = Color.white;
            Color colorEnd = Color.white;

            if (_materialOrigin != null)
            {
                colorStart = _materialOrigin.color;
                colorEnd = _materialOrigin.color;
            }

            colorEnd.a = 0f;

            Material materialReplace = new Material(_materialOrigin);

            _renderer.material = materialReplace;

            _tween = materialReplace.DOColor(colorEnd, _fadeDuration)
                                    .OnComplete(OnFadeComplete)
                                    .ChangeStartValue(colorStart);

            // Play sfx
            AudioManager.Play(_sfxTrigger.GetLoop(s_triggerIndex)).transformCached.position = transformCached.position;
            s_triggerIndex++;
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
