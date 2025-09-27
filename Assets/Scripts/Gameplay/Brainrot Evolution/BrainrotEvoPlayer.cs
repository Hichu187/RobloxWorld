using Cysharp.Threading.Tasks;
using Hichu;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoPlayer : MonoBehaviour
    {
        [SerializeField] CharacterCombat characterCombat;
        [SerializeField] BrainrotEvoConfig _currentConfig;
        [SerializeField] Transform _meshTransform;
        [SerializeField] Transform _petTarget;
        [SerializeField] List<BrainrotPet> _pets;

        private Player _player;
        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);
            StaticBus<Event_BrainrotEvo_EquipPet>.Subscribe(EventEquipPet);
            StaticBus<Event_BrainrotEvo_UnequipPet>.Subscribe(EventUnequipPet);

            _player = GetComponent<Player>();

            InitData();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
            StaticBus<Event_BrainrotEvo_EquipPet>.Unsubscribe(EventEquipPet);
            StaticBus<Event_BrainrotEvo_UnequipPet>.Unsubscribe(EventUnequipPet);
        }

        public async void InitData()
        {
            _currentConfig = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level];




            characterCombat._maxHealth = (int)_currentConfig.health;
            characterCombat._currentHealth = characterCombat._maxHealth;
            characterCombat._damage = (int)_currentConfig.damage;

            await UniTask.WaitForSeconds(0.35f);

            for (int i = _meshTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_meshTransform.GetChild(i).gameObject);
            }

            var model = Instantiate(_currentConfig.model, _meshTransform);

            await UniTask.WaitUntil(() => model != null);

            _player.character.cAnim.InitAnimator();
            _player.control.canMove = true;
            _player.character.GetComponent<CharacterDie>().ObjRoot = model.gameObject;

            SpawnPet();

            await UniTask.WaitForEndOfFrame();

            SetPetBonusDamage();
        }


        private void SpawnPet()
        {
            foreach(var petID in DataBrainrotEvo.equippedPet)
            {
                BrainrotEvoPetConfig petData = FactoryBrainrotEvo.pets[petID];

                BrainrotPet pet = petData.petModel.Create(this.transform).gameObject.GetComponent<BrainrotPet>();

                pet.bonusDamage = FactoryBrainrotEvo.pets[petID].bonusDamage;
                pet.GetComponent<BrainrotPetPosition>().target = _petTarget;
                _pets.Add(pet);
            }
        }

        public void SetPetBonusDamage()
        {
            float totalPetBonus = 0;

            foreach (var pet in _pets)
            {
                totalPetBonus += pet.bonusDamage;
            }
            characterCombat.petBonus = totalPetBonus;
        }

        private void EventAddExp(Event_Player_Add_Exp e)
        {
            LDebug.Log<BrainrotEvoPlayer>($"Add {e.exp} exp");
            DataBrainrotEvo.instance.AddExp(e.exp);
        }

        private void EventLevelUp(Event_Player_Level_Up e)
        {
            LDebug.Log<BrainrotEvoPlayer>($"Player level Up {e.level}, exp : {DataBrainrotEvo.exp}");

            InitData();
        }

        private void EventEquipPet(Event_BrainrotEvo_EquipPet e)
        {
            BrainrotPet pet = e.petData.petModel.Create(this.transform).gameObject.GetComponent<BrainrotPet>();

            int petID = FactoryBrainrotEvo.pets.IndexOf(e.petData);

            pet.bonusDamage = FactoryBrainrotEvo.pets[petID].bonusDamage;

            pet.GetComponent<BrainrotPetPosition>().target = _petTarget;
            _pets.Add(pet);

            SetPetBonusDamage();
        }

        private void EventUnequipPet(Event_BrainrotEvo_UnequipPet e)
        {
            if (_pets == null || _pets.Count == 0) return;

            int idx = -1;
            string targetPrefabName = (e != null && e.petData != null && e.petData.petModel != null)
                ? e.petData.petModel.name
                : null;

            // Ưu tiên tìm theo tên prefab gốc
            if (!string.IsNullOrEmpty(targetPrefabName))
            {
                for (int i = 0; i < _pets.Count; i++)
                {
                    var p = _pets[i];
                    if (p == null) { idx = i; break; }
                    var goName = p.gameObject.name;
                    if (!string.IsNullOrEmpty(goName) && goName.StartsWith(targetPrefabName))
                    {
                        idx = i;
                        break;
                    }
                }
            }
            else
            {
                // Nếu không có dữ liệu trong event, fallback: gỡ phần tử null đầu tiên nếu có
                for (int i = 0; i < _pets.Count; i++)
                {
                    if (_pets[i] == null)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            if (idx >= 0)
            {
                if (_pets[idx] != null)
                {
                    Destroy(_pets[idx].gameObject);
                    _pets[idx] = null;
                }
                _pets.RemoveAll(p => p == null);
            }
            else
            {
                _pets.RemoveAll(p => p == null);
                Debug.LogWarning("[Brainrot] Không tìm thấy pet để unequip hoặc dữ liệu event không khớp.");
            }

            SetPetBonusDamage();
        }
    }
}
