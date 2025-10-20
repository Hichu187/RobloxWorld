using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BrainrotOption : MonoBehaviour
    {
        public PetRank rank;
        [SerializeField] Image _border;
        [SerializeField] List<Sprite> _borderRankImage;
        [SerializeField] Image _icon;
        [SerializeField] TextMeshProUGUI _petName;

        public void InitData(int index)
        {
            _border.sprite = _borderRankImage[(int)FactoryStealBrainrot.brainrotConfigs[index].rank];
            _icon.sprite = FactoryStealBrainrot.brainrotConfigs[index].texture;
            _icon.SetNativeSize();
            _petName.text = FactoryStealBrainrot.brainrotConfigs[index].brainrotName;

            rank = FactoryStealBrainrot.brainrotConfigs[index].rank;
        }
    }
}
