using DG.Tweening;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class StealBrainrot_BaseGate : MonoBehaviour
    {
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
            if (!IsValidCharacter(other.transform, out var playerBaseID))
                return;

            if (_base != null && playerBaseID == _base.baseID)
                _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidCharacter(other.transform, out var playerBaseID))
                return;

            if (_base != null && playerBaseID != _base.baseID)
                _collider.isTrigger = false;
        }

        private bool IsValidCharacter(Transform target, out int baseID)
        {
            baseID = -1;

            if (target == null || target.gameObject.layer != LayerMask.NameToLayer("Character"))
                return false;

            var parent = target.parent;
            if (parent == null)
                return false;

            var player = parent.GetComponent<StealBrainrot_Player>();
            if (player != null && player.baseSlot != null)
            {
                baseID = player.baseSlot.baseID;
                return true;
            }

            var ai = parent.GetComponent<StealBrainrot_AI>();
            if (ai != null && ai.curBase != null)
            {
                baseID = ai.curBase.baseID;
                return true;
            }

            return false;
        }
    }
}
