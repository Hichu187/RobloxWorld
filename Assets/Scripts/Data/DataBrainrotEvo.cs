using Hichu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class DataBrainrotEvo : LDataBlock<DataBrainrotEvo>
    {
        [SerializeField] private int _level = 0;
        [SerializeField] private int _exp = 0;
        [SerializeField] private int _currentMap = 0;

        // Multi-instance: cho phép trùng
        [SerializeField] private List<int> _ownedPet = new List<int>();
        [SerializeField] private List<int> _equippedPet = new List<int>(); // ≤ 5 tổng phần tử

        // ===== Properties =====
        public static int level { get { return instance._level; } set { instance._level = value; } }
        public static int exp { get { return instance._exp; } set { instance._exp = value; } }
        public static int currentMap { get { return instance._currentMap; } set { instance._currentMap = value; } }

        // Cho phép set trực tiếp nhưng sẽ sanitize + sort
        public static List<int> ownedPet { get { return instance._ownedPet; } set { instance.SetOwnedPets(value); } }
        public static List<int> equippedPet => instance._equippedPet;

        // ===== EXP / Level =====
        public void AddExp(int amount)
        {
            _exp += amount;
            while (CanLevelUp()) LevelUp();
            Save();
        }

        private bool CanLevelUp()
        {
            return _level < FactoryBrainrotEvo.brainrotConfigs.Count &&
                   _exp >= FactoryBrainrotEvo.brainrotConfigs[_level].exp;
        }

        private void LevelUp()
        {
            int requiredExp = Mathf.RoundToInt(FactoryBrainrotEvo.brainrotConfigs[_level].exp);
            _exp -= requiredExp;
            _level++;
            StaticBus<Event_Player_Level_Up>.Post(new Event_Player_Level_Up(_level));
        }

        public static void MoveNextMap()
        {
            currentMap++;
            Save();
        }

        // ===== Owned Pet (multi-instance) =====

        /// <summary>Thêm 1 bản sao pet sở hữu, sort giảm dần, sanitize equipped, save.</summary>
        public static void AddOwnedPet(int petId)
        {
            instance._ownedPet.Add(petId);
            instance.SortOwnedDesc();
            instance.SanitizeEquippedAgainstOwnedAndLimit();
            Save();
        }

        /// <summary>Gỡ 1 bản sao pet sở hữu. Nếu đang equip nhiều hơn số bản sao còn lại thì tự cắt bớt.</summary>
        public static bool RemoveOwnedPet(int petId)
        {
            bool removed = instance.RemoveOneOwned(petId);
            if (removed)
            {
                instance.SanitizeEquippedAgainstOwnedAndLimit();
                Save();
            }
            return removed;
        }

        private bool RemoveOneOwned(int petId)
        {
            int idx = _ownedPet.IndexOf(petId);
            if (idx >= 0)
            {
                _ownedPet.RemoveAt(idx);
                return true;
            }
            return false;
        }

        private void SetOwnedPets(List<int> pets)
        {
            _ownedPet = pets ?? new List<int>();
            SortOwnedDesc();
            SanitizeEquippedAgainstOwnedAndLimit();
            Save();
        }

        public static void SortOwnedDescending()
        {
            instance.SortOwnedDesc();
            Save();
        }

        private void SortOwnedDesc()
        {
            _ownedPet.Sort((a, b) => b.CompareTo(a)); // giữ trùng, sort giảm dần
        }

        // ===== Equipped (multi-instance) =====

        /// <summary>Equip thêm 1 bản sao pet. Fail nếu vượt 5 slot hoặc không đủ bản sao sở hữu.</summary>
        public static bool EquipPet(int petId)
        {
            // Giới hạn 5
            if (instance._equippedPet.Count >= 5)
                return false;

            // Phải còn quota so với owned
            if (instance.GetEquippedCount(petId) >= instance.GetOwnedCount(petId))
                return false;

            instance._equippedPet.Add(petId);

            StaticBus<Event_BrainrotEvo_EquipPet>.Post(new Event_BrainrotEvo_EquipPet(FactoryBrainrotEvo.pets[petId]));

            Save();
            return true;
        }

        /// <summary>Unequip 1 bản sao pet.</summary>
        public static bool UnequipPet(int petId)
        {
            int idx = instance._equippedPet.IndexOf(petId);
            if (idx >= 0)
            {
                instance._equippedPet.RemoveAt(idx);

                StaticBus<Event_BrainrotEvo_UnequipPet>.Post(new Event_BrainrotEvo_UnequipPet(FactoryBrainrotEvo.pets[petId]));

                Save();
                return true;
            }
            return false;
        }

        /// <summary>Set danh sách trang bị (có thể trùng). Tự lọc để không vượt quota (owned) và không quá 5 slot.</summary>
        public static void SetEquippedPets(IEnumerable<int> petIds)
        {
            var input = (petIds ?? System.Array.Empty<int>()).ToList();

            // Cắt về tối đa 5 theo thứ tự input
            if (input.Count > 5) input = input.Take(5).ToList();

            // Clamp theo quota owned cho từng id, bảo toàn thứ tự input
            var ownedCounts = instance.CountMap(instance._ownedPet);
            var used = new Dictionary<int, int>();
            var result = new List<int>(input.Count);

            foreach (var id in input)
            {
                ownedCounts.TryGetValue(id, out int own);
                used.TryGetValue(id, out int usedCount);

                if (usedCount < own) // còn quota
                {
                    result.Add(id);
                    used[id] = usedCount + 1;
                }
                // nếu hết quota -> bỏ qua
            }

            instance._equippedPet = result;
            Save();
        }

        /// <summary>Đảm bảo equipped ⊆ owned theo multiplicity và tổng ≤ 5; gọi sau mọi thay đổi owned.</summary>
        private void SanitizeEquippedAgainstOwnedAndLimit()
        {
            if (_equippedPet == null) _equippedPet = new List<int>();

            // 1) Clamp multiplicity theo owned
            var ownedCounts = CountMap(_ownedPet);
            var used = new Dictionary<int, int>();
            var sanitized = new List<int>(_equippedPet.Count);

            foreach (var id in _equippedPet)
            {
                ownedCounts.TryGetValue(id, out int own);
                used.TryGetValue(id, out int usedCount);

                if (usedCount < own)
                {
                    sanitized.Add(id);
                    used[id] = usedCount + 1;
                }
                // nếu hết quota -> bỏ qua
            }

            _equippedPet = sanitized;

            // 2) Giới hạn tổng 5 (ưu tiên giữ đầu danh sách hiện tại)
            if (_equippedPet.Count > 5)
            {
                _equippedPet = _equippedPet.Take(5).ToList();
            }
        }

        // ===== Helpers =====
        private int GetOwnedCount(int petId)
        {
            // Với quy mô nhỏ, Count() O(n) là đủ rẻ
            int cnt = 0;
            for (int i = 0; i < _ownedPet.Count; i++) if (_ownedPet[i] == petId) cnt++;
            return cnt;
        }

        private int GetEquippedCount(int petId)
        {
            int cnt = 0;
            for (int i = 0; i < _equippedPet.Count; i++) if (_equippedPet[i] == petId) cnt++;
            return cnt;
        }

        private Dictionary<int, int> CountMap(List<int> list)
        {
            var dict = new Dictionary<int, int>();
            for (int i = 0; i < list.Count; i++)
            {
                int id = list[i];
                dict.TryGetValue(id, out int c);
                dict[id] = c + 1;
            }
            return dict;
        }
    }
}
