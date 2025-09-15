using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public enum BrainroEvoType
    {
        Normal = 0,
        CounterAttack = 1
    }
    public class BrainrotEvoBot : CharacterCombat
    {
        [Title("Data")]
        public float exp = 0;
        public BrainroEvoType type;

        private void Start()
        {
            
        }

        public void InitData()
        {

        }

        protected override void Die()
        {
            base.Die();

            StaticBus<Event_Player_Add_Exp>.Post(new Event_Player_Add_Exp(exp));
        }
    }
}
