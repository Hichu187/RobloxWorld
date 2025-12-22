using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using DamageNumbersPro;

namespace Game
{
    public class CharacterCombat : TargetTrait
    {
        [Title("Stats")]
        [SerializeField] private bool isTakeDamage = false;
        [ShowIf("isTakeDamage")] public int _maxHealth = 100;
        [ShowIf("isTakeDamage")] public int _currentHealth;
        [ShowIf("isTakeDamage")] public int _damage = 10;

        [Title("Attack Interval")]
        [Min(0.1f)] public float attackSpeed = 1f;

        [Title("Reference")]
        public GameObject _stats;
        public GameObject _level;
        public TextMeshProUGUI _nameText;
        public TextMeshProUGUI _hpText;
        public TextMeshProUGUI _levelText;
        public Image _hp_Bar;
        public GameObject hitPrefab;
        public GameObject takeDamagePrefab;
        public DamageNumberMesh damageText;
        [Title("Damage Bonus")]
        [Min(1)] public float petBonus = 1;
        public int specialBonus = 1;

        [Title("Knockback Config")]
        public bool _knockback = false;
        [ShowIf("_knockback")] public LayerMask _hitMask = ~0;
        [ShowIf("_knockback")] public float _knockbackForce = 10f;
        [ShowIf("_knockback"), Range(0f, 89f)] public float _knockbackAngleDeg = 45f;

        [Title("Hit Filters (Early & Cheap)")]
        [SerializeField, Min(0f)] private float maxAttackDistance = 10f;
        [SerializeField, Range(-1f, 1f)] private float minFacingDot = -0.2f;
        [SerializeField] private LayerMask damageMask = ~0;

        [Title("Explosion")]
        float distanceFactor = 1f;
        float durationFactor = 0.1f;
        float heightFactor = 0.15f;
        private List<Vector3> trajectoryPoints = new List<Vector3>();
        private Coroutine _coroutineExplosion;

        [Title("Auto Regen")]
        [SerializeField] private bool _autoRegen = false;
        [ShowIf("_autoRegen"), Min(0.1f)][SerializeField] private float _regenDelaySeconds = 10f;
        private Coroutine _regenRoutine;

        [Title("AI Auto Attack")]
        public bool isAIAutoAttack = false;
        [ShowIf("isAIAutoAttack"), Range(0f, 1f)] public float autoAttackChance = 0.6f;
        [ShowIf("isAIAutoAttack"), Min(0.05f)] public float autoThinkInterval = 0.2f;
        [ShowIf("isAIAutoAttack"), Min(0f)] public float startDelay = 1.2f;

        // Cached refs
        private Coroutine _autoAttackRoutine;
        private Character _character;
        private CharacterAnimator _anim;
        private CharacterControl _control;
        private Kcc.KccMotor _motor;
        private CharacterRagdoll _ragdoll;
        private FieldOfView _fov;
        private StealBrainrot_Player _stealPlayer;
        private StealBrainrot_AI _stealAI;

        private bool _isAttackAttempting = false;
        private float _nextAttackTime = 0f;
        private readonly List<Transform> _bufTargets = new List<Transform>(16);
        private float MaxDistSqr => maxAttackDistance <= 0f ? float.PositiveInfinity : maxAttackDistance * maxAttackDistance;

        public bool hasDied = false;

        private void Start()
        {
            _currentHealth = _maxHealth;

            _character = GetComponent<Character>();
            if (_character != null)
            {
                _anim = _character.cAnim;
                _control = _character.cControl;
                _motor = _character.motor;
                _ragdoll = _character.cRagdoll;
            }

            _fov = GetComponent<FieldOfView>();
            _stealPlayer = GetComponent<StealBrainrot_Player>();
            _stealAI = GetComponentInParent<StealBrainrot_AI>();

            if (isAIAutoAttack)
            {
                attackSpeed = Random.Range(3f, 5f);
            }
            if (_level != null)
            {
                _level.gameObject.SetActive(true);
                _levelText.text = $"Level {DataBrainrotEvo.level + 1}";

            }

            StaticBus<Event_Buff_Countdown_Start>.Subscribe(DamageBuff);
            StaticBus<Event_Buff_Countdown_End>.Subscribe(StopBuff);
        }

        private void OnEnable()
        {
            if (isAIAutoAttack && _autoAttackRoutine == null)
            {
                _autoAttackRoutine = StartCoroutine(Co_AutoAttackLoop());
            }
        }

        private void OnDisable()
        {
            if (_autoAttackRoutine != null)
            {
                StopCoroutine(_autoAttackRoutine);
                _autoAttackRoutine = null;
            }
        }
        private void OnDestroy()
        {
            StaticBus<Event_Buff_Countdown_Start>.Unsubscribe(DamageBuff);
            StaticBus<Event_Buff_Countdown_End>.Unsubscribe(StopBuff);
        }

        private void DamageBuff(Event_Buff_Countdown_Start e)
        {
            specialBonus = 2;
            GetComponent<CharacterControl>().MoveSpeedMultiple = 1.5f;
        }
        private void StopBuff(Event_Buff_Countdown_End e)
        {
            specialBonus = 1;
            GetComponent<CharacterControl>().MoveSpeedMultiple = 1f;
        }

        public float GetTotalDamage()
        {
            float safePetBonus = Mathf.Max(1, petBonus);
            return (_damage * safePetBonus) * specialBonus;
        }

        [SerializeField] private bool attackNearestOnly = false;

        public async void Attack(FieldOfView fov)
        {
            if (hasDied) return;
            if (_stealPlayer && _stealPlayer.isStealing) return;
            if (_control != null && _control.StateMachine.CurrentState != CharacterControl.State.Ground) return;
            if (Time.time < _nextAttackTime) return;

            _isAttackAttempting = true;
            _nextAttackTime = Time.time + attackSpeed;

            _anim?.Attack();
            await UniTask.WaitForSeconds(0.4f);

            if (fov == null || fov.combatables == null || fov.combatables.Count == 0)
            {
                _isAttackAttempting = false;
                return;
            }

            _bufTargets.Clear();
            Vector3 selfPos = transform.position;
            Vector3 fwd = transform.forward;

            _bufTargets.Add(fov.combatables[0]);

            float angRad = _knockbackAngleDeg * Mathf.Deg2Rad;
            float totalDamage = GetTotalDamage();

            for (int i = 0; i < _bufTargets.Count; i++)
            {
                Transform target = _bufTargets[i];
                if (!target) continue;

                Vector3 toTarget = target.position - selfPos;
                Vector3 horiz = Vector3.ProjectOnPlane(toTarget, Vector3.up);
                if (horiz.sqrMagnitude < 1e-6f)
                {
                    horiz = Vector3.ProjectOnPlane(fwd, Vector3.up);
                    if (horiz.sqrMagnitude < 1e-6f) horiz = Vector3.forward;
                }
                horiz.Normalize();

                Vector3 dirKnock = horiz * Mathf.Cos(angRad) + Vector3.up * Mathf.Sin(angRad);
                dirKnock.Normalize();

                if (target.TryGetComponent<CharacterCombat>(out var cc))
                {
                    cc.TakeDamage((int)totalDamage, _knockbackForce, dirKnock);

                    if (damageText != null)
                    {
                        DamageNumber damageNumber = damageText.Spawn(cc.transform.position, -(int)totalDamage);
                    }


                    if (hitPrefab)
                    {
                        hitPrefab.SetActive(true);
                        hitPrefab.transform.position = target.position;
                    }
                }
            }

            _isAttackAttempting = false;
        }

        public async virtual void TakeDamage(int amount, float force, Vector3 direction)
        {
            if (hasDied) return;

            if (isTakeDamage)
            {
                if(damageText != null)
                {
                    DamageNumber damageNumber = damageText.Spawn(transform.position, -amount);
                }

                _currentHealth -= amount;
                if (_currentHealth < 0) _currentHealth = 0;

                if (takeDamagePrefab) takeDamagePrefab.SetActive(true);
                ResetRegenCountdown();

                if (_currentHealth <= 0)
                {
                    Die();
                }
                else
                {
                    StartRegenCountdown();
                }

                if (_character != null && _character.isPlayer)
                {
                    _stats.SetActive(true);
                    if(_level != null)
                    {
                        _level.gameObject.SetActive(false);
                        _levelText.text = $"Level {DataBrainrotEvo.level + 1}";
                    }
                }

            }

            if (_knockback)
            {
                if (_motor) _motor.enabled = false;
                if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().isKinematic = true;
                _character?.cRagdoll.ActivateRagdoll(force * direction, direction);
                _character.cCamera?.SetFollowTransform(_character.cRagdoll.transform.GetChild(0).GetChild(0), true);

                if (_stealPlayer != null && _stealPlayer.isStealing) _stealPlayer.ResetSteal();
                if (_stealAI != null && _stealAI.isStealing) _stealAI.ResetSteal();

                DOVirtual.DelayedCall(2.5f, () =>
                {
                    if (_motor) _motor.enabled = true;
                    if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().isKinematic = false;
                    _character?.cRagdoll.SetRagdollActive(false);
                    if (_control != null) _control.StateMachine.CurrentState = CharacterControl.State.Ground;

                    _character?.cRagdoll.SetPos(_character);
                    _character.cCamera?.SetFollowTransform(_character.cCamera.defaultTarget);
                }).OnComplete(async () =>
                {
                    _character.cRagdoll.gameObject.SetActive(true);
                });

            }

            InitData();
        }

        private void KnockBack(float force, Vector3 direction)
        {
            if (_coroutineExplosion != null)
                StopCoroutine(_coroutineExplosion);

            Vector3 dirNormalized = direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.up;
            _coroutineExplosion = StartCoroutine(HandleExplosion(force, dirNormalized));
        }

        IEnumerator HandleExplosion(float force, Vector3 direction)
        {
            trajectoryPoints.Clear();

            float explodeDst = force * distanceFactor;
            float explodeDuration = Mathf.Max(force * durationFactor, 0.1f);
            float maxHeight = force * heightFactor;

            Vector3 start = _character.transformCached.position;
            Vector3 destination = start + direction * explodeDst;

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
                    _motor.SetPosition(hit.point);
                    trajectoryPoints.Add(hit.point);
                    break;
                }

                trajectoryPoints.Add(newPosition);
                _motor.SetPosition(newPosition);
                lastPos = newPosition;

                yield return null;
            }
        }

        public void InitData()
        {
            _hpText.text = $"{_currentHealth}/{_maxHealth}";
            float denom = Mathf.Max(1, _maxHealth);
            float fill = Mathf.Clamp01((float)_currentHealth / denom);
            _hp_Bar.fillAmount = fill;
        }

        public void ReSpawn()
        {
            _currentHealth = _maxHealth;
            InitData();
            StopRegen();
            hasDied = false;

            if (isAIAutoAttack)
            {
                attackSpeed = Random.Range(0.6f, 1.2f);
            }

            _nextAttackTime = Time.time + startDelay;
        }

        protected virtual void Die()
        {
            hasDied = true;
            if (_stats)
            {
                _stats.SetActive(false);
                if (_level != null)
                {
                    _level.gameObject.SetActive(true);
                    _levelText.text = $"Level {DataBrainrotEvo.level +1}";

                }
            }
            StopRegen();

            if (_character != null && _character.isPlayer)
                _character.Kill();
        }

        public bool IsAlive() => _currentHealth > 0;

        private void StartRegenCountdown()
        {
            if (!_autoRegen || hasDied || !isTakeDamage) return;

            if (_regenRoutine != null)
                StopCoroutine(_regenRoutine);

            _regenRoutine = StartCoroutine(RegenCountdownRoutine());
        }

        private void ResetRegenCountdown()
        {
            if (_regenRoutine != null)
            {
                StopCoroutine(_regenRoutine);
                _regenRoutine = null;
            }
        }

        private void StopRegen()
        {
            if (_regenRoutine != null)
            {
                StopCoroutine(_regenRoutine);
                _regenRoutine = null;
            }
        }

        private IEnumerator RegenCountdownRoutine()
        {
            float t = 0f;
            while (t < _regenDelaySeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (!hasDied && isTakeDamage)
            {
                _currentHealth = _maxHealth;
                if (_stats)
                {
                    _stats.SetActive(false);
                    if (_level != null)
                    {
                        _level.gameObject.SetActive(true);
                        _levelText.text = $"Level {DataBrainrotEvo.level +1}";

                    }
                }
                InitData();
            }

            _regenRoutine = null;
        }

        private IEnumerator Co_AutoAttackLoop()
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            var wait = new WaitForSeconds(autoThinkInterval);

            while (isAIAutoAttack)
            {
                if (Time.time >= _nextAttackTime)
                {
                    if (!hasDied && !_isAttackAttempting)
                        Attack(_fov);

                    if (_nextAttackTime <= Time.time)
                        _nextAttackTime = Time.time + attackSpeed;
                }

                yield return wait;
            }
        }

        public void EvoUplevel()
        {
            if (_level != null && _level.activeSelf)
            {
                _levelText.text = $"Level {DataBrainrotEvo.level + 1}";
            }
        }
    }
}
