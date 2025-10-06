using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Base : MonoBehaviour
    {
        public int baseID;

        [SerializeField] private List<StealBrainrot_Slot> _slots;


        public void SetLock(bool isLock)
        {

        }


        //UTILS
        public StealBrainrot_Slot GetFirstEmptySlot()
        {
            if (_slots == null || _slots.Count == 0)
                return null;

            var slot = _slots
                .Where(s => s != null && s.isEmpty)
                .OrderBy(s => s.slotId)
                .FirstOrDefault();

            return slot;
        }

        //EDITOR

        [Button]
        public void SetSlotID()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].slotId = i;
                _slots[i].baseId = baseID;
            }
        }

    }
}
