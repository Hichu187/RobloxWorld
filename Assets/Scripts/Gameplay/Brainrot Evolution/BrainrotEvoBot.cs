using Cysharp.Threading.Tasks;
using Hichu;
using Sirenix.OdinInspector;
using System.Threading;
using System.Threading.Tasks;
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
        public string botName;
        public int exp = 0;
        public int coin = 0;

        [Title("Reference")]
        [SerializeField] private GameObject _model;

        // === CounterAttack state ===
        private CancellationTokenSource _counterAttackCts;
        private void Start()
        {
            if (_nameText.text != botName) _nameText.text = botName;
            InitData();
        }

        public override void TakeDamage(int amount, float force, Vector3 direction)
        {
            base.TakeDamage(amount, force, direction);

            if (hasDied) return;

            if (type == BrainroEvoType.CounterAttack)
            {
                var fov = GetComponent<FieldOfView>();
                if (fov != null)
                {
                    StartCounterAttackLoop(fov);
                }
            }

            InitData();
        }

        private void StartCounterAttackLoop(FieldOfView fov)
        {
            if (_counterAttackCts != null) return;

            if (fov.combatables != null && fov.combatables.Count > 0)
            {
                Vector3 dir = fov.combatables[0].position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }

            _counterAttackCts = new CancellationTokenSource();
            var token = _counterAttackCts.Token;

            CounterAttackLoop(fov, token).Forget();
        }

        private async UniTaskVoid CounterAttackLoop(FieldOfView fov, CancellationToken token)
        {
            float interval = (attackSpeed > 0f) ? attackSpeed : 1f;

            try
            {
                while (!token.IsCancellationRequested && !hasDied)
                {
                    if (!fov.haveTarget)
                        break;

                    Attack(fov);

                    await UniTask.WaitForSeconds(interval, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {

            }
            finally
            {
                _counterAttackCts?.Dispose();
                _counterAttackCts = null;
            }
        }

        public void Respawn()
        {
            _currentHealth = _maxHealth;
            InitData();
            hasDied = false;

            _model.gameObject.SetActive(true);
            _stats.gameObject.SetActive(true);
            GetComponent<Collider>().enabled = true;
        }

        protected override async void Die()
        {
            base.Die();

            StaticBus<Event_Player_Add_Exp>.Post(new Event_Player_Add_Exp(exp));

            _stats.gameObject.SetActive(false);
            GetComponent<Collider>().enabled = false;
            await UniTask.WaitForSeconds(1f);
            _model.gameObject.SetActive(false);

            await UniTask.WaitForSeconds(5f);

            Respawn();
        }
    }
}
