using Sirenix.OdinInspector;
using System.Collections;
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

        private Coroutine _lockRoutine;
        private bool _isLocked;

        private const float SPECIAL_LOCK_DURATION = 300f; // 5 phút = 300 giây

        private void Awake()
        {
            SetSlotID();
            SetLock(false);
        }

        // ===================== LOCK CONTROL =====================

        public void SetLock(bool isLock)
        {
            _isLocked = isLock;

            if (gateLock)
                gateLock.SetActive(isLock);
        }

        /// <summary>
        /// Lock base ngay lập tức trong 5 phút
        /// </summary>
        public void LockSpecial5Minutes()
        {
            SetLock(true);
            if (buttonLock != null)
                buttonLock.StartLockCountdown(300f);
        }
        /// <summary>
        /// Mở khóa base ngay lập tức, hủy mọi lock timer
        /// </summary>
        public void BreakLock()
        {
            if (buttonLock != null)
                buttonLock.ForceUnlockImmediate();
            else
                SetLock(false);
        }
        private IEnumerator LockCountdown(float duration)
        {
            yield return new WaitForSeconds(duration);
            _lockRoutine = null;
            SetLock(false);
        }

        // ===================== UTILS =====================

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

        // ===================== EDITOR =====================

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
