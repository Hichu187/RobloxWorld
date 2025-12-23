using DG.Tweening;
using Hichu;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class StealBrainrot_BaseGate : MonoBehaviour
    {
        private BoxCollider _collider;
        private StealBrainrot_Base _base;

        private View _view;
        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _base = GetComponentInParent<StealBrainrot_Base>();
        }

        private async void OnCollisionEnter(Collision other)
        {
            if (other.transform.TryGetComponent(out StealBrainrot_Player player))
            {
                if (_base.baseID != 0)
                {
                    _view = await ViewHelper.PushAsync(FactoryPrefab.popupBreakLock);
                    _view.GetComponent<Popup_Steal_BreakLock>().baseM = _base;

                    return;
                }
            }


            if (!IsValidCharacter(other.transform, out var playerBaseID))
            {
                TryRandomAIState(other.transform);
                return;
            }

            if (_base && playerBaseID == _base.baseID)
                _collider.isTrigger = true;
            else
                TryRandomAIState(other.transform);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidCharacter(other.transform, out var playerBaseID))
            {
                TryRandomAIState(other.transform);
                return;
            }

            if (_base && playerBaseID != _base.baseID)
            {
                _collider.isTrigger = false;
                TryRandomAIState(other.transform);
            }
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

        private void TryRandomAIState(Transform target)
        {

            if (!target) return;

            StealBrainrot_AI ai = null;

            if (!target.TryGetComponent(out ai))
            {
                if (target.parent)
                    target.parent.TryGetComponent(out ai);
            }

            if (ai == null) return;

            AIState[] randomStates = new AIState[]
            {
                AIState.BuyPet,
                AIState.FollowWaypoint,
                AIState.ReturnHome
            };

            int r = Random.Range(0, randomStates.Length);
            ai.SetState(randomStates[r]);

            ai.GetComponent<AI>().Chase(_base.playerSpawnPosition);

            Debug.Log(randomStates[r].ToString());

        }
    }
}
