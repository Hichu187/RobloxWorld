using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class PlatformFade : MonoCached, ICharacterCollidable
    {
        [Title("Reference")]
        [SerializeField] private MeshRenderer _renderer;

        [Title("Config")]
        [SerializeField, Min(0f)] private float _fadeDuration = 0.75f;
        [SerializeField, Min(0f)] private float _appearDelay = 2f;

        private Material _origin;
        private Material _runtime;
        private string _colorProp = "_Color";
        private Tween _tween;
        private MaterialPropertyBlock _mpb;
        private float _alpha = 1f;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void Start()
        {
            _origin = _renderer != null ? _renderer.sharedMaterial : null;
            if (_origin == null) return;

            _runtime = new Material(_origin);
            _renderer.material = _runtime;

            _colorProp = _runtime.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

            MakeTransparent(_runtime);

            var c = _runtime.GetColor(_colorProp);
            c.a = 1f;
            _runtime.SetColor(_colorProp, c);

            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorProp, c);
            _renderer.SetPropertyBlock(_mpb);
            _alpha = 1f;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            if (_runtime != null) Destroy(_runtime);
        }

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character) => FadeOut();
        void ICharacterCollidable.OnCollisionExit(CharacterControl character) { }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character) => FadeOut();
        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { }

        private void FadeOut()
        {
            if (_renderer == null || _runtime == null) return;
            if (_tween != null && _tween.IsActive()) return;

            _renderer.GetPropertyBlock(_mpb);
            var start = _mpb.GetColor(_colorProp);
            _alpha = start.a;

            _tween = DOTween.To(() => _alpha, v =>
            {
                _alpha = v;
                var c = start; c.a = _alpha;
                _mpb.SetColor(_colorProp, c);
                _renderer.SetPropertyBlock(_mpb);
            }, 0f, _fadeDuration).OnComplete(OnFadeComplete);
        }

        private void OnFadeComplete()
        {
            gameObjectCached.SetActive(false);

            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(_appearDelay, () =>
            {
                if (_renderer == null || _runtime == null) return;

                _renderer.GetPropertyBlock(_mpb);
                var c = _mpb.GetColor(_colorProp);
                c.a = 1f;
                _mpb.SetColor(_colorProp, c);
                _renderer.SetPropertyBlock(_mpb);
                _alpha = 1f;

                gameObjectCached.SetActive(true);
            }, false);
        }

        private static void MakeTransparent(Material m)
        {
            if (m.shader != null && m.shader.name.Contains("Universal Render Pipeline"))
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_ZWrite", 0f);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                return;
            }

            if (m.shader != null && m.shader.name.Contains("Standard"))
            {
                m.SetFloat("_Mode", 2f);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                return;
            }
        }
    }
}
