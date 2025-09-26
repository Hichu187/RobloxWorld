using Hichu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Game.BrainrotEvoGachaRates;

namespace Game
{
    public static class BrainrotEvoGachaRates
    {
        public const float RATE_COMMON = 0.55f;
        public const float RATE_UNCOMMON = 0.25f;
        public const float RATE_RARE = 0.12f;
        public const float RATE_EPIC = 0.06f;
        public const float RATE_LEGENDARY = 0.02f;
    }

    public class OpenEgg : MonoBehaviour
    {
        [SerializeField] private OpenEggOption _optionPrefab;
        [SerializeField] private Transform _optionParent;
        [SerializeField] private Button _btnHatch;

        [SerializeField] private List<OpenEggOption> _options = new List<OpenEggOption>();


        private void Start()
        {
            SpawnOptions();
            _btnHatch.onClick.AddListener(Hatch);
        }

        private void SpawnOptions()
        {
            if (_optionPrefab == null || _optionParent == null) return;

            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap; // List<BrainrotEvoPetConfig>

            for (int i = 0; i < list.Count; i++)
            {
                var opt = _optionPrefab.Create(_optionParent);
                opt.InitData(i); // nếu OpenEggOption đang hiểu i là index trong map, giữ nguyên
                _options.Add(opt);
            }
        }

        private void ReinitOptions()
        {
            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap; // List<BrainrotEvoPetConfig>

            if (_options.Count != list.Count)
            {
                for (int i = 0; i < _options.Count; i++)
                {
                    if (_options[i] != null) DestroyImmediate(_options[i].gameObject);
                }
                _options.Clear();

                SpawnOptions();
                return;
            }

            for (int i = 0; i < _options.Count; i++)
            {
                _options[i].InitData(i);
            }
        }

        private void Hatch()
        {
            // 1) Roll rank
            PetRank rolledRank = RollRank();

            // 2) Chọn petData theo rank (có fallback)
            BrainrotEvoPetConfig petData = PickPetConfigFromCurrentMapByRankWithFallback(rolledRank);
            if (petData == null)
            {
                Debug.LogWarning("[OpenEgg] Không tìm thấy pet nào trong map hiện tại.");
                return;
            }

            // 3) Đổi sang id toàn cục (index trong FactoryBrainrotEvo.pets) để lưu vào ownedPet
            int petId = FactoryBrainrotEvo.pets.IndexOf(petData);
            if (petId < 0)
            {
                Debug.LogWarning("[OpenEgg] pet không nằm trong FactoryBrainrotEvo.pets.");
                return;
            }

            DataBrainrotEvo.AddOwnedPet(petId);

            Debug.Log($"[OpenEgg] Hatch -> {petData.petName} ({petData.petRank}) (ID={petId})");
            ReinitOptions();
        }

        private PetRank RollRank()
        {
            float total = RATE_COMMON + RATE_UNCOMMON + RATE_RARE + RATE_EPIC + RATE_LEGENDARY;
            float r = Random.value * total;

            float acc = 0f;
            acc += RATE_COMMON; if (r < acc) return PetRank.Common;
            acc += RATE_UNCOMMON; if (r < acc) return PetRank.Uncommon;
            acc += RATE_RARE; if (r < acc) return PetRank.Rare;
            acc += RATE_EPIC; if (r < acc) return PetRank.Epic;
            return PetRank.Legendary;
        }

        private BrainrotEvoPetConfig PickPetConfigFromCurrentMapByRankWithFallback(PetRank target)
        {
            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap;

            var byRank = new Dictionary<PetRank, List<BrainrotEvoPetConfig>>(5)
    {
        { PetRank.Common,    new List<BrainrotEvoPetConfig>() },
        { PetRank.Uncommon,  new List<BrainrotEvoPetConfig>() },
        { PetRank.Rare,      new List<BrainrotEvoPetConfig>() },
        { PetRank.Epic,      new List<BrainrotEvoPetConfig>() },
        { PetRank.Legendary, new List<BrainrotEvoPetConfig>() }
    };

            for (int i = 0; i < list.Count; i++)
            {
                var cfg = list[i];
                byRank[cfg.petRank].Add(cfg);
            }

            var picked = PickRandomFrom(byRank[target]);
            if (picked != null) return picked;

            int t = (int)target;
            for (int r = t - 1; r >= (int)PetRank.Common; r--)
            {
                picked = PickRandomFrom(byRank[(PetRank)r]);
                if (picked != null) return picked;
            }
            for (int r = t + 1; r <= (int)PetRank.Legendary; r++)
            {
                picked = PickRandomFrom(byRank[(PetRank)r]);
                if (picked != null) return picked;
            }
            return null;
        }


        private BrainrotEvoPetConfig PickRandomFrom(List<BrainrotEvoPetConfig> pool)
        {
            if (pool == null || pool.Count == 0) return null;
            int idx = Random.Range(0, pool.Count);
            return pool[idx];
        }
    }
}
