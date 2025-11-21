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
            _base = GetComponentInParent<StealBrainrot_Base>();
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
            if (!target || target.gameObject.layer != LayerMask.NameToLayer("Character"))
                return false;

            if (target.TryGetComponent(out StealBrainrot_Player pl) && pl.baseSlot)
            {
                baseID = pl.baseSlot.baseID;
                return true;
            }

            var parent = target.parent;
            if (parent && parent.TryGetComponent(out StealBrainrot_AI ai) && ai.curBase)
            {
                baseID = ai.curBase.baseID;
                return true;
            }

            return false;
        }
    }
}
