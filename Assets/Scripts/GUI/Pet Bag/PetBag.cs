using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PetBag : MonoBehaviour
    {
        [SerializeField] private PetBagPreview _preview;
        [SerializeField] private PetOption _optionPrefab;
        [SerializeField] private Transform _optionParent;

        [SerializeField] private List<PetOption> _options = new List<PetOption>();

        private List<int> ordered = new List<int>();

        private void Start()
        {
            Refresh();
            WireOptionButtons();
        }

        [Button("Refresh Spawn")]
        public void Refresh()
        {
            ClearOptions();
            ordered = BuildOrderedPetList();
            SpawnOptions(ordered);
            if (_preview != null) _preview.InitPreview(-1);
            WireOptionButtons();
        }

        public void ShowPreview(int index)
        {
            if (index < 0 || index >= ordered.Count) return;
            _preview.InitPreviewByIndex(index);
        }

        private List<int> BuildOrderedPetList()
        {
            var owned = DataBrainrotEvo.ownedPet;
            var equipped = DataBrainrotEvo.equippedPet;

            var resultFirst = new List<int>(owned.Count);
            var resultRest = new List<int>(owned.Count);

            var equipCount = new Dictionary<int, int>();
            if (equipped != null)
            {
                for (int i = 0; i < equipped.Count; i++)
                {
                    int id = equipped[i];
                    if (equipCount.TryGetValue(id, out int c)) equipCount[id] = c + 1;
                    else equipCount[id] = 1;
                }
            }

            for (int i = 0; i < owned.Count; i++)
            {
                int id = owned[i];
                if (equipCount.TryGetValue(id, out int c) && c > 0)
                {
                    resultFirst.Add(id);
                    equipCount[id] = c - 1;
                }
                else
                {
                    resultRest.Add(id);
                }
            }

            resultFirst.AddRange(resultRest);
            return resultFirst;
        }

        private void SpawnOptions(List<int> petIds)
        {
            if (_optionPrefab == null || _optionParent == null) return;

            var equipCount = new Dictionary<int, int>();
            var equipped = DataBrainrotEvo.equippedPet;
            if (equipped != null)
            {
                for (int i = 0; i < equipped.Count; i++)
                {
                    int id = equipped[i];
                    if (equipCount.TryGetValue(id, out int c)) equipCount[id] = c + 1;
                    else equipCount[id] = 1;
                }
            }

            for (int i = 0; i < petIds.Count; i++)
            {
                int id = petIds[i];

                var opt = _optionPrefab.Create(_optionParent);
                opt.InitData(id);

                if (equipCount.TryGetValue(id, out int c) && c > 0)
                {
                    equipCount[id] = c - 1;
                    opt.Equip(true);
                }
                else
                {
                    opt.Equip(false);
                }

                _options.Add(opt);
            }
        }

        private void ClearOptions()
        {
            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i] != null)
                    DestroyImmediate(_options[i].gameObject);
            }
            _options.Clear();

            if (_optionParent != null)
            {
                for (int i = _optionParent.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(_optionParent.GetChild(i).gameObject);
                }
            }
        }

        [Button]
        private void SortOwnedPet()
        {
            DataBrainrotEvo.SortOwnedDescending();
            DataBrainrotEvo.Save();
        }

        public void UnequipOne(int petId)
        {
            bool ok = DataBrainrotEvo.UnequipPet(petId);
            if (ok) Refresh();
            else Debug.Log($"[PetBag] Unequip failed for id={petId} (not equipped?).");
        }

        public void EquipOne(int petId)
        {
            bool ok = DataBrainrotEvo.EquipPet(petId);
            if (ok) Refresh();
            else Debug.Log($"[PetBag] Equip failed for id={petId} (limit/quota?).");
        }

        public void UnequipAtIndex(int index)
        {
            if (index < 0 || index >= ordered.Count) return;
            UnequipOne(ordered[index]);
        }

        public void EquipAtIndex(int index)
        {
            if (index < 0 || index >= ordered.Count) return;
            EquipOne(ordered[index]);
        }

        private void WireOptionButtons()
        {
            for (int i = 0; i < _options.Count; i++)
            {
                int index = i;
                var btn = _options[index].GetComponent<Button>();
                if (btn == null) continue;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowPreview(index));
            }
        }

        public bool ShowPreviewByPetId(int petId)
        {
            if (ordered == null || ordered.Count == 0) return false;
            int idx = ordered.IndexOf(petId);
            if (idx >= 0)
            {
                ShowPreview(idx);
                return true;
            }
            return false;
        }

        public int GetEquippedCount()
        {
            return DataBrainrotEvo.equippedPet != null ? DataBrainrotEvo.equippedPet.Count : 0;
        }

        public int GetIndexOfPetId(int petId)
        {
            if (ordered == null || ordered.Count == 0) return -1;
            return ordered.IndexOf(petId);
        }

        public int GetPetIdAt(int index)
        {
            if (ordered == null || index < 0 || index >= ordered.Count) return -1;
            return ordered[index];
        }

        public void RefreshAndReselect(int petId)
        {
            Refresh();
            int idx = GetIndexOfPetId(petId);
            if (idx >= 0) ShowPreview(idx);
            else if (_preview != null) _preview.InitPreview(-1);
        }
    }
}
