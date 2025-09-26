using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Game.BrainrotEvoGachaRates; // dùng trực tiếp RATE_*

namespace Game
{
    public class OpenEggOption : MonoBehaviour
    {
        [SerializeField] Image _border;
        [SerializeField] List<Sprite> _borderRankImage;
        [SerializeField] Image _icon;
        [SerializeField] TextMeshProUGUI _petName;
        [SerializeField] TextMeshProUGUI _petRate;

        public void InitData(int index)
        {
            BrainrotEvoPetConfig petData = FactoryBrainrotEvo.mapDatas[DataBrainrotEvo.currentMap].petMap[index];

            _border.sprite = _borderRankImage[(int)petData.petRank];
            _icon.sprite = petData.petIcon;
            _icon.SetNativeSize();

            int indexOfFull = FactoryBrainrotEvo.pets.IndexOf(petData);
            _icon.color = DataBrainrotEvo.ownedPet.Contains(indexOfFull) ? Color.white : Color.black;

            _petName.text = petData.petName;

            float rate = GetRateByRank(petData.petRank);
            _petRate.text = $"{rate:P0}";
        }

        private float GetRateByRank(PetRank rank)
        {
            switch (rank)
            {
                case PetRank.Common: return RATE_COMMON;
                case PetRank.Uncommon: return RATE_UNCOMMON;
                case PetRank.Rare: return RATE_RARE;
                case PetRank.Epic: return RATE_EPIC;
                case PetRank.Legendary: return RATE_LEGENDARY;
                default: return 0f;
            }
        }
    }
}
