using DG.Tweening;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class StealBrainrot_BaseGate : MonoBehaviour
    {
        [SerializeField] private GameObject barriePrefab;

        private BoxCollider _collider;
        private StealBrainrot_Base _base;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _base = transform.parent ? transform.parent.GetComponent<StealBrainrot_Base>() : null;

            if (_collider == null)
                Debug.LogWarning($"{name}: Missing BoxCollider component!");

            if (_base == null)
                Debug.LogWarning($"{name}: Missing parent StealBrainrot_Base!");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsValidCharacter(other.transform, out var player))
                return;

            if (player.baseSlot != null && _base != null && player.baseSlot.baseID == _base.baseID)
                _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidCharacter(other.transform, out var player))
                return;

            if (player.baseSlot != null && _base != null && player.baseSlot.baseID != _base.baseID)
                _collider.isTrigger = false;
        }

        private bool IsValidCharacter(Transform target, out StealBrainrot_Player player)
        {
            player = null;

            if (target == null || target.gameObject.layer != LayerMask.NameToLayer("Character"))
                return false;

            var parent = target.parent;
            if (parent == null)
                return false;

            player = parent.GetComponent<StealBrainrot_Player>();
            return player != null;
        }
    }
}
