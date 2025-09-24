using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class SettingsToggle : UIButtonBase
    {
        [System.Serializable]
        public enum Type
        {
            Music = 0,
            Sound = 1,
            Vibration = 2,
        }

        [Title("Reference")]
        [SerializeField] GameObject _objOn;
        [SerializeField] GameObject _objOff;

        [Space]

        [SerializeField] Type _type;

        private bool _isOn
        {
            get
            {
                switch (_type)
                {
                    case Type.Music:
                        return DataSettings.musicVolume.value > 0.0f;
                    case Type.Sound:
                        return DataSettings.soundVolume.value > 0.0f;
                    case Type.Vibration:
                        return DataSettings.vibration.value;
                    default:
                        return false;
                }
            }
            set
            {
                switch (_type)
                {
                    case Type.Music:
                        DataSettings.musicVolume.value = value ? 1.0f : 0.0f;
                        break;
                    case Type.Sound:
                        DataSettings.soundVolume.value = value ? 1.0f : 0.0f;
                        break;
                    case Type.Vibration:
                        DataSettings.vibration.value = value;
                        break;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            UpdateUI();
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            _isOn = !_isOn;

            UpdateUI();
        }

        private void UpdateUI()
        {
            _objOn.SetActive(_isOn);
            _objOff.SetActive(!_isOn);
        }
    }
}
