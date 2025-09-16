using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PlatformConveyor : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] Transform _model;
        [SerializeField] GameObject _cubePrefab;
        [SerializeField] Transform _arrowParent;
        [SerializeField] GameObject _arrow;
        [SerializeField] int size = 5;
        [SerializeField] int sizeX = 3;

        [SerializeField] private float _velocity = 2f;

        [SerializeField] private Collider _collider;

        Tween _tw;

        [HideInInspector]
        public List<Character> _controllers = new List<Character>();

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            _controllers.Add(character.GetComponent<Character>());
        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {
            _controllers.Add(character.GetComponent<Character>());
        }

        private void Start()
        {
            _collider = GetComponent<Collider>();

            if (_collider == null)
                _collider = GetComponentInChildren<Collider>();

            SpawnArrow();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (!_controllers[i].motor.GroundingStatus.IsStableOnGround)
                    continue;

                if (_controllers[i].motor.GroundingStatus.GroundCollider != _collider)
                    continue;

                _controllers[i].cControl.AddVelocity(Quaternion.Euler(Vector3.zero) * -transform.forward * _velocity * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            _tw?.Kill();
        }

        private void SpawnArrow()
        {
            float zStart = size % 2 != 0 ? size / 2 : size / 2 - 0.5f;

            var go = _arrow.Create(_arrowParent);
            go.transform.SetLocalZ(zStart);
            go.transform.DOLocalMoveZ(-zStart, size / 1.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(go);
            });

            _tw = DOVirtual.DelayedCall(1.5f, () =>
            {
                SpawnArrow();
            });
        }

        [Button("Generate")]
        private void Generate()
        {
            var collider = GetComponent<BoxCollider>();

            float zStart = size % 2 != 0 ? size / 2 : size / 2 - 0.5f;

            collider.size = new Vector3(sizeX, collider.size.y, size);

            for (int i = 0; i < _model.childCount; i++)
            {
                DestroyImmediate(_model.GetChild(i).gameObject);
            }

            for (int i = 0; i < size; i++)
            {
                var go = _cubePrefab.Create(_model);
                go.transform.localPosition = new Vector3(0, 0, -zStart + i);

                Collider col = go.GetComponentInChildren<Collider>();
                DestroyImmediate(col);
            }

        }
    }
}
