using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kcc.Base
{
    public class CharacterCombat : MonoBehaviour
    {
        [Title("Combat Config")]
        [SerializeField] private float _attackCooldown = 1f;
        private float _lastAttackTime;

        public void Attack(FieldOfView fov)
        {
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                _lastAttackTime = Time.time;

                if (fov.visibleTargets.Count == 0 || fov.visibleTargets == null) return;

                LDebug.Log<CharacterCombat>($" {fov.visibleTargets.Count} enemy Get hit");
            }
            else
            {
                float remain = (_lastAttackTime + _attackCooldown) - Time.time;
            }
        }
    }
}
