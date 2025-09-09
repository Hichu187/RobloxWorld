using Hichu;
using UnityEngine;

namespace Kcc.Base
{
    public class CharacterCombat : TargetTrait
    {
        [Header("Stats")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _damage = 10;

        [Header("Combat Settings")]
        [SerializeField] private float _attackSpeed = 1f;
        private float _lastAttackTime;
        private bool hasDied = false;
        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void Attack(FieldOfView fov)
        {
            if (Time.time >= _lastAttackTime + _attackSpeed)
            {
                _lastAttackTime = Time.time;

                if (fov.combatables.Count == 0 || fov.combatables == null) return;

                foreach (var target in fov.combatables)
                {
                    if (target.GetComponent<CharacterCombat>())
                        target.GetComponent<CharacterCombat>().TakeDamage(_damage);
                }
            }
            else
            {
                float remain = (_lastAttackTime + _attackSpeed) - Time.time;
            }
        }

        public void TakeDamage(int amount)
        {
            if (hasDied) return;

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                LDebug.Log<CharacterCombat>($"take {amount} damage");
            }
        }

        private void Die()
        {
            hasDied = true;
            LDebug.Log($"[{name}] has died.");
            // TODO: thêm logic chết (disable, animation, v.v.)
        }

        public bool IsAlive()
        {
            return _currentHealth > 0;
        }
    }
}
