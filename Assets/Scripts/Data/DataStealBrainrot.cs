using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DataStealBrainrot : LDataBlock<DataStealBrainrot>
    {
        [SerializeField] private int _cash = 1000;

        public static int cash
        {
            get => instance._cash;
            set => instance._cash = value;
        }

        [SerializeField] private Dictionary<int, int> _baseSlots = new();

        public static IReadOnlyDictionary<int, int> BaseSlots => instance._baseSlots;

        public void CashUpdate(int cashDelta)
        {
            _cash += cashDelta;
            bool increase = cashDelta > 0;
            StaticBus<Event_Cash_Update>.Post(new Event_Cash_Update(cashDelta, increase));
            Save();
        }

        public static void AddOrUpdateBaseSlot(int slotIndex, int brainrotId)
        {
            if (slotIndex < 0 || slotIndex >= 10) return;
            var dict = instance._baseSlots;
            dict[slotIndex] = brainrotId;
            Save();
        }

        public static void RemoveBaseSlot(int slotIndex)
        {
            if (instance._baseSlots.ContainsKey(slotIndex))
            {
                instance._baseSlots.Remove(slotIndex);
                Save();
            }
        }

        public static int GetBrainrotIdAtSlot(int slotIndex)
        {
            return instance._baseSlots.TryGetValue(slotIndex, out int id) ? id : -1;
        }

        public static void ClearAllBaseSlots()
        {
            instance._baseSlots.Clear();
            Save();
        }

        public static bool IsSlotEmpty(int slotIndex)
        {
            return !instance._baseSlots.ContainsKey(slotIndex);
        }
    }
}
