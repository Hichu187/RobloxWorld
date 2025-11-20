using Sirenix.OdinInspector;
using UnityEngine;
using Hichu;

namespace Game
{
    public class StealBrainrot_Player : MonoBehaviour
    {
        public StealBrainrot_Base baseSlot;
        public bool isStealing = false;
        public Transform holdingPos;

        private StealBrainrot_Brainrot takedBrainrot;
        private Transform _preTrans;
        private StealBrainrot_Slot _preSlot;

        private void Start()
        {
            if (baseSlot != null) Player.Instance.character.motor.SetPositionAndRotation(baseSlot.playerSpawnPosition.position, baseSlot.playerSpawnPosition.rotation);
        }
        public void StealingBrainrot(StealBrainrot_Brainrot brainrot)
        {
            isStealing = true;
            takedBrainrot = brainrot;

            _preTrans = brainrot.transform.parent;

            brainrot.StealBrainrot(holdingPos);

            Player.Instance.character.cAnim.SetSteal(isStealing);

            int victimID = brainrot.targetSlot.baseId;
            StealBrainrot_AiManager aiManager = FindAnyObjectByType<StealBrainrot_AiManager>();
            StealBrainrot_AI victim = aiManager.ais[victimID];
            if (!victim.isStealing) victim.SetState(AIState.ChasePlayer);
        }

        public void StealingDone(StealBrainrot_Slot slot)
        {
            takedBrainrot.targetSlot = slot;
            takedBrainrot.target = slot.transform;
            takedBrainrot.indBase = slot.baseId;

            slot.SetBrainrot(takedBrainrot);
            slot.StartGenerating();

            int slotIndex = baseSlot.slots.IndexOf(slot);
            if (slotIndex >= 0)
                DataStealBrainrot.AddOrUpdateBaseSlot(slotIndex, takedBrainrot.bConfig.ID);

            ResetSteal();
        }

        [Button]
        public void ResetSteal()
        {
            takedBrainrot.transform.parent = _preTrans;
            takedBrainrot.isMovingHome = true;
            takedBrainrot.canMove = true;

            isStealing = false;
            Player.Instance.character.cAnim.SetSteal(isStealing);
            takedBrainrot = null;

        }
    }
}
