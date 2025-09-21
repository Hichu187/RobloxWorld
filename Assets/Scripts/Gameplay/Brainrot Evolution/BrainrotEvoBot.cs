using Hichu;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public BrainroEvoType type;
        
        public int exp = 0;
        public int coin = 0;

        [Title("Reference")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _hp_Bar;
        private void Start()
        {
            InitData();
        }

        public override void TakeDamage(int amount, float force, Vector3 direction)
        {
            base.TakeDamage(amount, force, direction);

            InitData();
        }

        public void InitData()
        {
            _hpText.text = $"{_currentHealth}/{_maxHealth}";
            _hp_Bar.fillAmount = _currentHealth / _maxHealth;
        }

        protected override void Die()
        {
            base.Die();

            StaticBus<Event_Player_Add_Exp>.Post(new Event_Player_Add_Exp(exp));
        }
    }
}
