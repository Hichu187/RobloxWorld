using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Base : MonoBehaviour
    {
        public int baseID;

        public StealBrainrot_ButtonLock buttonLock;
        public Transform playerSpawnPosition;
        public Transform lockButton;
        public GameObject gateLock;
        public List<StealBrainrot_Slot> slots;

        private void Awake()
        {
            SetSlotID();
            SetLock(false);
        }

        public void SetLock(bool isLock)
        {
            if (gateLock)
                gateLock.SetActive(isLock);
        }

        // UTILS
        public StealBrainrot_Slot GetFirstEmptySlot()
        {
            if (slots == null || slots.Count == 0)
                return null;

            return slots
                .Where(s => s != null && s.isEmpty)
                .OrderBy(s => s.slotId)
                .FirstOrDefault();
        }

        public StealBrainrot_Slot GetRandomEmptySlot()
        {
            if (slots == null || slots.Count == 0)
                return null;

            var emptySlots = slots.Where(s => s != null && s.isEmpty).ToList();
            if (emptySlots.Count == 0)
                return null;

            return emptySlots[Random.Range(0, emptySlots.Count)];
        }

        // EDITOR
        [Button]
        public void SetSlotID()
        {
            if (slots == null) return;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null) continue;
                slots[i].slotId = i;
                slots[i].baseId = baseID;
            }
        }
    }
}
