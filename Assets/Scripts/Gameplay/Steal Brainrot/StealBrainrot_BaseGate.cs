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

            if (_collider == null)
                Debug.LogWarning($"{name}: Missing BoxCollider component!");

            if (_base == null)
                Debug.LogWarning($"{name}: Missing parent StealBrainrot_Base!");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsValidCharacter(other.transform, out var playerBaseID))
                return;

            Debug.Log("Test");

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

            var p = target;
            if (!p)
                return false;

            if (p.TryGetComponent(out StealBrainrot_Player pl) && pl.baseSlot)
            {
                baseID = pl.baseSlot.baseID;
                Debug.Log($"[Gate] Player '{p.name}' baseID = {baseID}");
                return true;
            }

            if (p.TryGetComponent(out StealBrainrot_AI ai) && ai.curBase)
            {
                baseID = ai.curBase.baseID;
                Debug.Log($"[Gate] AI '{p.name}' baseID = {baseID}");
                return true;
            }

            Debug.Log($"[Gate] '{p.name}' không có baseSlot hoặc curBase hợp lệ.");
            return false;
        }


    }
}
