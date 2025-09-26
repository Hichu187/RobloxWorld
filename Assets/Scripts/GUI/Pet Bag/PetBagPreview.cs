using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PetBagPreview : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _petName;
        [SerializeField] private TextMeshProUGUI _petRank;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _petExp;
        [SerializeField] private Button _btnEquip;
        [SerializeField] private TextMeshProUGUI _btnText;
        [SerializeField] private System.Collections.Generic.List<Color> _rankColor;

        private PetBag petBag;
        private int _currentPetId = -1;
        private int _currentIndex = -1;

        private void Start()
        {
            petBag = GetComponentInParent<PetBag>();
        }

        public void InitPreview(int petId)
        {
            if (petId == -1)
            {
                _currentPetId = -1;
                _currentIndex = -1;
                transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                // Nếu chỉ có petId, cố gắng tìm index đầu tiên (fallback)
                int idx = petBag != null ? petBag.GetPetIdAt(0) : -1;
                _currentPetId = petId;
                _currentIndex = -1;
                transform.GetChild(0).gameObject.SetActive(true);
                DataInit(petId);
            }
        }

        public void InitPreviewByIndex(int index)
        {
            _currentIndex = index;
            _currentPetId = (petBag != null) ? petBag.GetPetIdAt(index) : -1;

            if (_currentPetId == -1)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                return;
            }

            transform.GetChild(0).gameObject.SetActive(true);
            DataInit(_currentPetId);
        }

        private void DataInit(int id)
        {
            BrainrotEvoPetConfig petData = FactoryBrainrotEvo.pets[id];
            _petName.text = petData.petName;
            _petRank.text = petData.petRank.ToString();
            _petRank.color = _rankColor[(int)petData.petRank];
            _icon.sprite = petData.petIcon;
            _icon.SetNativeSize();
            _petExp.text = $"x {petData.bonusDamage} muscle";
            _petExp.color = _rankColor[(int)petData.petRank];

            RefreshEquipButtonVisual();
            WireEquipButton();
        }

        private void RefreshEquipButtonVisual()
        {
            if (petBag == null || _currentIndex < 0)
            {
                _btnText.text = "Equip";
                return;
            }

            int eqCount = petBag.GetEquippedCount();
            bool isEquippedByIndex = (_currentIndex < eqCount);
            _btnText.text = isEquippedByIndex ? "Equipped" : "Equip";
        }

        private void WireEquipButton()
        {
            _btnEquip.onClick.RemoveAllListeners();

            _btnEquip.onClick.AddListener(() =>
            {
                if (petBag == null || _currentPetId == -1) return;

                int eqCount = petBag.GetEquippedCount();

                if (_currentIndex >= 0 && _currentIndex < eqCount)
                {
                    bool ok = DataBrainrotEvo.UnequipPet(_currentPetId);
                    if (!ok) Debug.Log($"[PetBagPreview] Unequip failed for id={_currentPetId}");
                    petBag.RefreshAndReselect(_currentPetId);
                }
                else
                {
                    if (eqCount < 5)
                    {
                        bool ok = DataBrainrotEvo.EquipPet(_currentPetId);
                        if (!ok) Debug.Log($"[PetBagPreview] Equip failed for id={_currentPetId} (limit/quota?)");
                    }
                    else
                    {
                        Debug.Log("FULL SLOT");
                    }
                    petBag.RefreshAndReselect(_currentPetId);
                }
            });
        }
    }
}
