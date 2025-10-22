
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PetOpenPreview : MonoBehaviour
    {
        [SerializeField] Image _image;
        [SerializeField] Image _light;
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _rate;
        [SerializeField] List<Color> _color;


        public void InitData(BrainrotEvoPetConfig petData)
        {
            _image.sprite = petData.petIcon;
            _name.text = petData.petName;
            _rate.text = $"x {petData.bonusDamage} muscle";
            _rate.color = _color[(int)petData.petRank];
            _light.color = _color[(int)petData.petRank];
        }
    }
}
