using Cysharp.Threading.Tasks;
using Hichu;
using log4net.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoPlayer : MonoBehaviour
    {
        [SerializeField] CharacterCombat characterCombat;
        [SerializeField] BrainrotEvoConfig _currentConfig;
        [SerializeField] Transform _meshTransform;

        private Player _player;
        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);

            _player = GetComponent<Player>();

            InitData();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
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
    }
}
