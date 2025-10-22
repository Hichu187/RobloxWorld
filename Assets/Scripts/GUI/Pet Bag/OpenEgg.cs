using Hichu;
using System.Collections.Generic;
using TMPro;
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
        [SerializeField] private Button _btnAdsCash;
        [SerializeField] private int openCash = 1000;
        [SerializeField] private List<OpenEggOption> _options = new();
        [SerializeField] private List<float> _rankRates = new() { RATE_COMMON, RATE_UNCOMMON, RATE_RARE, RATE_EPIC, RATE_LEGENDARY };
        private int _mapDataIndex = -1;

        private void Start()
        {

            _btnHatch.onClick.AddListener(Hatch);
            _btnAdsCash.onClick.AddListener(AdsCash);

            
            UpdatePriceLabels();
        }

        public void Init(int index)
        {
            var mapDatas = FactoryBrainrotEvo.mapDatas;
            if (mapDatas == null || mapDatas.Count == 0 || index < 0 || index >= mapDatas.Count) return;
            _mapDataIndex = index;
            var md = mapDatas[_mapDataIndex];
            openCash = md.price;

            SpawnOptions();

            UpdatePriceLabels();
            ApplyRatesFrom(FactoryBrainrotEvo.petRate[index].rate);
        }

        private void UpdatePriceLabels()
        {
            if (_btnHatch && _btnHatch.transform.childCount > 0)
            {
                var txt = _btnHatch.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (txt) txt.text = openCash.ToString();
            }
            if (_btnAdsCash && _btnAdsCash.transform.childCount > 0)
            {
                var txt = _btnAdsCash.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (txt) txt.text = openCash.ToString();
            }
        }

        private void ApplyRatesFrom(List<int> petRate)
        {
            if (petRate != null && petRate.Count >= 5)
            {
                float c = Mathf.Max(0, petRate[0]);
                float uc = Mathf.Max(0, petRate[1]);
                float r = Mathf.Max(0, petRate[2]);
                float e = Mathf.Max(0, petRate[3]);
                float l = Mathf.Max(0, petRate[4]);
                float s = c + uc + r + e + l;
                if (s > 0f)
                {
                    _rankRates[0] = c / s;
                    _rankRates[1] = uc / s;
                    _rankRates[2] = r / s;
                    _rankRates[3] = e / s;
                    _rankRates[4] = l / s;
                    return;
                }
            }
            _rankRates[0] = RATE_COMMON;
            _rankRates[1] = RATE_UNCOMMON;
            _rankRates[2] = RATE_RARE;
            _rankRates[3] = RATE_EPIC;
            _rankRates[4] = RATE_LEGENDARY;
        }

        private void AdsCash()
        {
            DataBrainrotEvo.instance.CashUpdate(openCash);
        }

        private void SpawnOptions()
        {
            if (_optionPrefab == null || _optionParent == null) return;
            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap;
            for (int i = 0; i < list.Count; i++)
            {
                var opt = _optionPrefab.Create(_optionParent);
                opt.InitData(_mapDataIndex,i);
                _options.Add(opt);
            }
        }

        private void ReinitOptions()
        {
            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap;
            if (_options.Count != list.Count)
            {
                for (int i = 0; i < _options.Count; i++)
                    if (_options[i] != null) DestroyImmediate(_options[i].gameObject);
                _options.Clear();
                SpawnOptions();
                return;
            }
            for (int i = 0; i < _options.Count; i++)
                _options[i].InitData(_mapDataIndex,i);
        }

        private void Hatch()
        {
            if (DataBrainrotEvo.cash < openCash)
            {
                UINotificationText.Push("Not enough money");
                return;
            }
            DataBrainrotEvo.instance.CashUpdate(-openCash);
            PetRank rolledRank = RollRank();
            BrainrotEvoPetConfig petData = PickPetConfigFromCurrentMapByRankWithFallback(rolledRank);
            if (petData == null) return;
            int petId = FactoryBrainrotEvo.pets.IndexOf(petData);
            if (petId < 0) return;
            DataBrainrotEvo.AddOwnedPet(petId);
            ReinitOptions();
        }

        private PetRank RollRank()
        {
            float total = 0f;
            for (int i = 0; i < _rankRates.Count; i++) total += _rankRates[i];
            float r = Random.value * total;
            float acc = 0f;
            acc += _rankRates[0]; if (r < acc) return PetRank.Common;
            acc += _rankRates[1]; if (r < acc) return PetRank.Uncommon;
            acc += _rankRates[2]; if (r < acc) return PetRank.Rare;
            acc += _rankRates[3]; if (r < acc) return PetRank.Epic;
            return PetRank.Legendary;
        }

        private BrainrotEvoPetConfig PickPetConfigFromCurrentMapByRankWithFallback(PetRank target)
        {
            var map = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var list = map.petMap;
            var byRank = new Dictionary<PetRank, List<BrainrotEvoPetConfig>>(5)
            {
                { PetRank.Common, new List<BrainrotEvoPetConfig>() },
                { PetRank.Uncommon, new List<BrainrotEvoPetConfig>() },
                { PetRank.Rare, new List<BrainrotEvoPetConfig>() },
                { PetRank.Epic, new List<BrainrotEvoPetConfig>() },
                { PetRank.Legendary, new List<BrainrotEvoPetConfig>() }
            };
            for (int i = 0; i < list.Count; i++)
            {
                var cfg = list[i];
                if (cfg == null) continue;
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
