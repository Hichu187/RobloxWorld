using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class OpenEggOption : MonoBehaviour
    {
        [SerializeField] Image _border;
        [SerializeField] List<Sprite> _borderRankImage;
        [SerializeField] Image _icon;
        [SerializeField] TextMeshProUGUI _petName;
        [SerializeField] TextMeshProUGUI _petRate;

        public void InitData(int eggIndex, int index)
        {
            var mapData = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap];
            var petData = mapData.petMap[index];
            _border.sprite = _borderRankImage[(int)petData.petRank];
            _icon.sprite = petData.petIcon;
            _icon.SetNativeSize();

            int indexOfFull = FactoryBrainrotEvo.pets.IndexOf(petData);
            _icon.color = DataBrainrotEvo.ownedPet.Contains(indexOfFull) ? Color.white : Color.black;

            _petName.text = petData.petName;

            float rate = GetRateByRank(petData.petRank, FactoryBrainrotEvo.petRate[eggIndex].rate);
            _petRate.text = $"{rate:P0}";
        }

        private float GetRateByRank(PetRank rank, List<int> petRate)
        {
            if (petRate == null || petRate.Count < 5) return 0f;
            float c = Mathf.Max(0, petRate[0]);
            float uc = Mathf.Max(0, petRate[1]);
            float r = Mathf.Max(0, petRate[2]);
            float e = Mathf.Max(0, petRate[3]);
            float l = Mathf.Max(0, petRate[4]);
            float total = c + uc + r + e + l;
            if (total <= 0f) return 0f;

            return ((int)rank) switch
            {
                0 => c / total,
                1 => uc / total,
                2 => r / total,
                3 => e / total,
                4 => l / total,
                _ => 0f
            };
        }
    }
}
