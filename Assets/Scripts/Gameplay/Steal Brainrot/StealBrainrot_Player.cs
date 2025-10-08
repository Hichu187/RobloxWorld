using Sirenix.OdinInspector;
using UnityEngine;

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
        public void StealingBrainrot(StealBrainrot_Brainrot brainrot)
        {
            isStealing = true;
            takedBrainrot = brainrot;

            _preTrans = brainrot.transform.parent;

            brainrot.transform.parent = holdingPos;
            brainrot.transform.position = holdingPos.position;
            brainrot.transform.localRotation = Quaternion.Euler(0, 0, 0);
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
            takedBrainrot = null;

        }
    }
}
