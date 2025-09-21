using Hichu;
using log4net.Core;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoPlayer : MonoBehaviour
    {
        [SerializeField] CharacterCombat characterCombat;

        [SerializeField] BrainrotEvoConfig _currentConfig;
        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Subscribe(EventLevelUp);

            InitData();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
            StaticBus<Event_Player_Level_Up>.Unsubscribe(EventLevelUp);
        }

        public void InitData()
        {
            _currentConfig = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level];

            characterCombat._maxHealth = (int)_currentConfig.health;
            characterCombat._currentHealth = characterCombat._maxHealth;
            characterCombat._damage = (int)_currentConfig.damage;
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
