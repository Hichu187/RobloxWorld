using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Base : MonoBehaviour
    {
        public int baseID;

        public List<StealBrainrot_Slot> slots;


        public void SetLock(bool isLock)
        {

        }


        //UTILS
        public StealBrainrot_Slot GetFirstEmptySlot()
        {
            if (slots == null || slots.Count == 0)
                return null;

            var slot = slots
                .Where(s => s != null && s.isEmpty)
                .OrderBy(s => s.slotId)
                .FirstOrDefault();

            return slot;
        }

        //EDITOR

        [Button]
        public void SetSlotID()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].slotId = i;
                slots[i].baseId = baseID;
            }
        }

    }
}
