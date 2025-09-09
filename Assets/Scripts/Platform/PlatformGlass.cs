using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class PlatformGlass : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] private bool _isBreakable;
        [SerializeField] private GameObject _prefab;

        [Space]

        [SerializeField] private float _respawnDelay = 2f;

        [Space]

        [SerializeField] private AudioConfig[] _sfxBroken;

        private PlatformBreakable _current;

        private Tween _tween;

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        private void Start()
        {
            _current = GetComponentInChildren<PlatformBreakable>();
            _current.onBreak += OnGlassBreak;

            GenerateBreakable();
        }

        private void GenerateBreakable()
        {
            if (_current == null)
            {
                _current = _prefab.Create(transform).GetComponent<PlatformBreakable>();

                _current.onBreak += OnGlassBreak;
            }
            else if (_current.isBroken)
            {
                Destroy(_current.gameObject);

                _current = null;

                GenerateBreakable();

                return;
            }

            _current.transform.localScale = Vector3.one;
            _current.transform.localPosition = Vector3.zero;
            _current.GetComponent<BoxCollider>().isTrigger = _isBreakable;
        }

        private void OnGlassBreak()
        {
            AudioManager.Play(_sfxBroken.GetRandom());

            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(_respawnDelay, GenerateBreakable);
        }
    }
}
