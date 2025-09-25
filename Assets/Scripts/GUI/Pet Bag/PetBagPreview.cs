using System.Collections.Generic;
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

        [SerializeField] List<Color> _rankColor;

        PetBag petBag;

        // Lưu lại ID pet đang preview để xử lý click
        private int _currentPetId = -1;

        private void Start()
        {
            petBag = GetComponentInParent<PetBag>();
        }

        public void InitPreview(int petId)
        {
            if (petId == -1)
            {
                _currentPetId = -1;
                transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                _currentPetId = petId;
                transform.GetChild(0).gameObject.SetActive(true);
                DataInit(petId);
            }
        }

        private void DataInit(int id)
        {
            // id ở đây là ID của pet (index vào FactoryBrainrotEvo.pets)
            BrainrotEvoPetConfig petData = FactoryBrainrotEvo.pets[id];
            _petName.text = petData.petName;
            _petRank.text = petData.petRank.ToString();
            _petRank.color = _rankColor[(int)petData.petRank];
            _icon.sprite = petData.petIcon;
            _icon.SetNativeSize();
            _petExp.text = $"x {petData.bonusDamage} muscle";
            _petExp.color = _rankColor[(int)petData.petRank];

            // Cập nhật button text + callback theo trạng thái equipped hiện tại
            RefreshEquipButtonVisual(id);
            WireEquipButton(id);
        }

        private void RefreshEquipButtonVisual(int id)
        {
            bool isEquipped = DataBrainrotEvo.equippedPet != null &&
                              DataBrainrotEvo.equippedPet.Contains(id);

            _btnText.text = isEquipped ? "Equipped" : "Equip";
        }

        private void WireEquipButton(int id)
        {
            _btnEquip.onClick.RemoveAllListeners();

            _btnEquip.onClick.AddListener(() =>
            {
                bool isEquippedNow = DataBrainrotEvo.equippedPet != null &&
                                     DataBrainrotEvo.equippedPet.Contains(id);

                if (isEquippedNow)
                {
                    bool ok = DataBrainrotEvo.UnequipPet(id);
                    if (!ok)
                    {
                        Debug.Log($"[PetBagPreview] Unequip failed for id={id}");
                    }
                    // Reload list & reselect đúng pet
                    if (petBag != null) petBag.RefreshAndReselect(id);
                    else RefreshEquipButtonVisual(id); // fallback nếu không có petBag
                }
                else
                {
                    if (DataBrainrotEvo.equippedPet != null &&
                        DataBrainrotEvo.equippedPet.Count >= 5)
                    {
                        Debug.Log("Equipped full");
                    }
                    else
                    {
                        bool ok = DataBrainrotEvo.EquipPet(id); // cho phép multi-instance theo quota
                        if (!ok)
                        {
                            Debug.Log($"[PetBagPreview] Equip failed for id={id} (limit/quota?)");
                        }
                        // Reload list & reselect đúng pet
                        if (petBag != null) petBag.RefreshAndReselect(id);
                        else RefreshEquipButtonVisual(id); // fallback
                    }
                }
            });
        }

    }
}
