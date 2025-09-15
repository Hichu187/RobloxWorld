using Hichu;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoPlayer : MonoBehaviour
    {
        [SerializeField] CharacterCombat characterCombat;
        private void Start()
        {
            StaticBus<Event_Player_Add_Exp>.Subscribe(EventAddExp);

            InitData();
        }

        private void OnDestroy()
        {
            StaticBus<Event_Player_Add_Exp>.Unsubscribe(EventAddExp);
        }

        public void InitData()
        {
            characterCombat._maxHealth = (int)FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level].health;
            characterCombat._currentHealth = characterCombat._maxHealth;
            characterCombat._damage = (int)FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level].damage;
        }

        private void EventAddExp(Event_Player_Add_Exp e)
        {
            LDebug.Log<BrainrotEvoPlayer>($"Add {e.exp} exp");
        }
    }
}
