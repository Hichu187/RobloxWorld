using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kcc.Base
{
    public class CharacterCombat : TargetTrait
    {
        [Title("Stats")]
        [SerializeField] private bool isTakeDamage = false;
        [ShowIf("isTakeDamage",true)]
        [SerializeField] private int _maxHealth = 100;
        [ShowIf("isTakeDamage", true)]
        [SerializeField] private int _currentHealth;
        [ShowIf("isTakeDamage", true)]
        [SerializeField] private int _damage = 10;

        [Title("Combat Settings")]
        [SerializeField] private float _attackSpeed = 1f;
        [Title("Knockback Config")]
        [SerializeField]
        private LayerMask _hitMask = ~0;
        [SerializeField] private float _knockbackForce = 10f;
        [SerializeField, Range(0f, 89f)] private float _knockbackAngleDeg = 45f;

        [Title("Explosion")]
        float distanceFactor = 1f;
        float durationFactor = 0.1f;
        float heightFactor = 0.15f;
        private List<Vector3> trajectoryPoints = new List<Vector3>();
        private Coroutine _coroutineExplosion;

        private Character _character;
        private float _lastAttackTime;
        private bool hasDied = false;
        private void Awake()
        {
            _currentHealth = _maxHealth;
            _character = GetComponent<Character>();
        }

        public void Attack(FieldOfView fov)
        {
            if (Time.time >= _lastAttackTime + _attackSpeed)
            {
                _lastAttackTime = Time.time;

                if (fov.combatables.Count == 0 || fov.combatables == null) return;
                float angRad = _knockbackAngleDeg * Mathf.Deg2Rad;

                foreach (var target in fov.combatables)
                {
                    Vector3 toTarget = target.position - transform.position;

                    Vector3 horiz = Vector3.ProjectOnPlane(toTarget, Vector3.up);
                    if (horiz.sqrMagnitude < 1e-6f)
                    {
                        horiz = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                        if (horiz.sqrMagnitude < 1e-6f) horiz = Vector3.forward;
                    }
                    horiz.Normalize();

                    // Nâng góc lên 45° (hoặc theo _knockbackAngleDeg)
                    // dir = horiz * cos(θ) + up * sin(θ)
                    Vector3 dir = horiz * Mathf.Cos(angRad) + Vector3.up * Mathf.Sin(angRad);

                    dir.Normalize();

                    if (target.GetComponent<CharacterCombat>())
                        target.GetComponent<CharacterCombat>().TakeDamage(_damage, _knockbackForce, dir);
                }
            }
            else
            {
                float remain = (_lastAttackTime + _attackSpeed) - Time.time;
            }
        }


        public void TakeDamage(int amount, float force, Vector3 direction)
        {
            if (hasDied) return;
            if (isTakeDamage)
            {
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

            KnockBack(force, direction);

        }
        private void KnockBack(float force, Vector3 direction)
        {
            LDebug.Log<CharacterCombat>($"KnockBack");

            if (_coroutineExplosion != null)
                StopCoroutine(_coroutineExplosion);
            Vector3 dirNormalized = direction.normalized;
            _coroutineExplosion = StartCoroutine(HandleExplosion(force, dirNormalized));

        }

        IEnumerator HandleExplosion(float force, Vector3 direction)
        {
            trajectoryPoints.Clear();

            float explodeDst = force * distanceFactor;
            float explodeDuration = Mathf.Max(force * durationFactor, 0.1f);
            float maxHeight = force * heightFactor;

            Vector3 start = _character.transformCached.position;
            Vector3 destination = start + direction.normalized * explodeDst;

            float elapsedTime = 0f;
            Vector3 lastPos = start;

            while (elapsedTime < explodeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / explodeDuration;

                Vector3 newPosition = Vector3.Lerp(start, destination, t);
                float arcHeight = Mathf.Sin(t * Mathf.PI) * maxHeight;
                newPosition.y = start.y + arcHeight;

                if (Physics.Linecast(lastPos, newPosition, out RaycastHit hit, _hitMask, QueryTriggerInteraction.Ignore))
                {
                    _character.motor.SetPosition(hit.point);
                    trajectoryPoints.Add(hit.point);
                    break;
                }

                trajectoryPoints.Add(newPosition);
                _character.motor.SetPosition(newPosition);
                lastPos = newPosition;

                yield return null;
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
