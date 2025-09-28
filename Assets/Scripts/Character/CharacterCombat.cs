using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace Game
{
    public class CharacterCombat : TargetTrait
    {
        [Title("Stats")]
        [SerializeField] private bool isTakeDamage = false;
        [ShowIf("isTakeDamage", true)]
        public int _maxHealth = 100;
        [ShowIf("isTakeDamage", true)]
        public int _currentHealth;
        [ShowIf("isTakeDamage", true)]
        public int _damage = 10;
        public float attackSpeed = 1f;

        [Title("Reference")]
        public GameObject _stats;
        public TextMeshProUGUI _nameText;
        public TextMeshProUGUI _hpText;
        public Image _hp_Bar;


        [Title("Damage Bonus")]
        [Min(1)] public float petBonus = 1;   // đảm bảo tối thiểu = 1
        public int specialBonus = 0;

        [Title("Knockback Config")]
        [SerializeField] private bool _knockback = false;
        [SerializeField, ShowIf("_knockback", true)] private LayerMask _hitMask = ~0;
        [SerializeField, ShowIf("_knockback", true)] private float _knockbackForce = 10f;
        [SerializeField, ShowIf("_knockback", true), Range(0f, 89f)] private float _knockbackAngleDeg = 45f;

        [Title("Explosion")]
        float distanceFactor = 1f;
        float durationFactor = 0.1f;
        float heightFactor = 0.15f;
        private List<Vector3> trajectoryPoints = new List<Vector3>();
        private Coroutine _coroutineExplosion;

        private Character _character;
        private float _lastAttackTime;
        public bool hasDied = false;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _character = GetComponent<Character>();
        }

        public float GetTotalDamage()
        {
            float safePetBonus = Mathf.Max(1, petBonus);
            return (_damage * safePetBonus) + specialBonus;
        }

        public async void Attack(FieldOfView fov)
        {
            if (hasDied) return;

            if (_character != null)
            {
                if (_character.cControl.StateMachine.CurrentState != CharacterControl.State.Ground) return;
            }

            if (Time.time >= _lastAttackTime + attackSpeed)
            {
                _lastAttackTime = Time.time;

                if (_character) _character.cAnim.Attack();

                PlayerControl pControl = GetComponentInParent<PlayerControl>();
                if (pControl != null)
                {
                    pControl.canMove = false;
                }

                await UniTask.WaitForSeconds(0.4f);

                if (fov.combatables == null || fov.combatables.Count == 0) return;
                float angRad = _knockbackAngleDeg * Mathf.Deg2Rad;

                float totalDamage = GetTotalDamage();

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

                    Vector3 dir = horiz * Mathf.Cos(angRad) + Vector3.up * Mathf.Sin(angRad);
                    dir.Normalize();

                    if (target.GetComponent<CharacterCombat>())
                    {
                        target.GetComponent<CharacterCombat>().TakeDamage((int)totalDamage, _knockbackForce, dir);
                    }
                }
            }
            else
            {
                float remain = _lastAttackTime + attackSpeed - Time.time;
            }
        }

        public virtual void TakeDamage(int amount, float force, Vector3 direction)
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

            if (_knockback)
            {
                KnockBack(force, direction);
            }

            if (_character != null && _character.isPlayer)
            {
                _stats.SetActive(true);
            }

            InitData();
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

        public void InitData()
        {
            _hpText.text = $"{_currentHealth}/{_maxHealth}";

            float denom = Mathf.Max(1, _maxHealth);
            float fill = Mathf.Clamp01((float)_currentHealth / denom);

            _hp_Bar.fillAmount =fill;
        }

        public void ReSpawn()
        {
            _currentHealth = _maxHealth;

            InitData();
        }

        protected virtual void Die()
        {
            hasDied = true;

            if (_stats != null) _stats.SetActive(false);

            if (_character != null && _character.isPlayer)
            {
                _character.Kill();
            }
        }

        public bool IsAlive()
        {
            return _currentHealth > 0;
        }
    }
}
