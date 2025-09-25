using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PetOption : MonoBehaviour
    {
        [SerializeField] Image _border;
        [SerializeField] List<Sprite> _borderRankImage;
        [SerializeField] Image _icon;
        [SerializeField] TextMeshProUGUI _petName;

        [SerializeField] GameObject _status;

        private void Start()
        {

        }
        public void InitData(int index)
        {
            _border.sprite = _borderRankImage[(int)FactoryBrainrotEvo.pets[index].petRank];
            _icon.sprite = FactoryBrainrotEvo.pets[index].petIcon;
            _icon.SetNativeSize();
            _petName.text = FactoryBrainrotEvo.pets[index].petName;
        }

        public void Equip(bool choose)
        {
            _status.SetActive(choose);
        }

        public bool IsEquipped()
        {
            return _status.activeSelf;
        }
    }
}
