using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game
{
    public enum AIState
    {
        Idle,        // Đứng yên / chờ
        BuyPet,  // Đi đến chỗ shop để mua pet
        StealPet,    // Đi trộm pet ở nhà khác
        ChasePlayer, // Đuổi theo player khác khi phát hiện trộm
        ReturnHome,
        FollowWaypoint,
        LockDoor
    }

    public class StealBrainrot_AI : MonoBehaviour
    {
        public StealBrainrot_Base curBase;

        private AI _ai;
        private AIState _state;

        private AIWaypoint _curWaypoint;
        private bool _isStealing = false;
        private bool _waypointChasing = false;
        private bool _isGoHome = false;

        public AIState GetState()
        {
            return _state;
        }

        public void SetState(AIState state)
        {
            this._state = state;
        }

        void Revive()
        {
            _ai.Stop();
        }

        private void Awake()
        {
            _ai = GetComponent<AI>();
            _ai.character.eventDie += Revive;
        }

        private void Start()
        {
            _ai.eventChaseComplete += AI_EventChaseComplete;
            _ai.eventIdleComplete += AI_EventIdleComplete;

            FollowNearestWaypoint();
        }

        private void Update()
        {
            Attack();
        }


        public void FollowNearestWaypoint()
        {
            _curWaypoint = AIWaypointManager.Instance.GetNearestWaypoint(_ai.character.transformCached.position);
            _ai.Chase(_curWaypoint.GetRandomPosition());
        }

        public void AI_EventChaseComplete()
        {
            _ai.Idle();
        }

        public void AI_EventIdleComplete()
        {
            switch (_state)
            {
                case AIState.Idle:
                    RandomStateChase();
                    _ai.Idle();
                    break;
                case AIState.BuyPet:
                    ChaseBuyPet();
                    break;
                case AIState.StealPet:
/*                    if (StealPetManager.instance.hasPlayerStealPet == false)
                    {
                        ChaseStealPet();
                    }
                    else
                    {
                        state = BotState.Idle;
                        _ai.Idle();
                    }*/
                    break;
                case AIState.ReturnHome:
                    ChaseHome();
                    break;
                case AIState.ChasePlayer:
                    ChasePlayer();
                    break;
                case AIState.FollowWaypoint:
                    FollowWaypoint();
                    break;
                case AIState.LockDoor:
                    ChaseLockDoor();
                    break;
            }

        }

        public void RandomStateChase()
        {
            int k = Random.Range(0, 6);

/*            if (k == 1)
            {
                var list = StealPetManager.instance.randomBotSteal;
                if (list.Contains(_character.indBase))
                    _state = AIState.StealPet;
                else
                    RandomStateChase();
                return;
            }*/

            _state = k switch
            {
                0 => AIState.Idle,
                1 => AIState.StealPet,
                2 => AIState.ReturnHome,
                3 => AIState.BuyPet,
                4 => AIState.FollowWaypoint,
                5 => AIState.LockDoor,
                _ => AIState.Idle
            };
        }
        public int stepBuy = 0;
        private void ChaseBuyPet()
        {
            LDebug.Log<StealBrainrot_AI>($"Start State Chase Buy Pet {stepBuy}");
            switch (stepBuy)
            {
                case 0:
                    FollowNearestWaypoint();
                    stepBuy++;
                    break;
                case 1:
                    AIWaypoint aiWaypoint = StealBrainrot_AiManager.instance.buyBrainrotWaypoints.GetRandom();
                    _ai.Chase(aiWaypoint.GetRandomPosition());
                    stepBuy++;
                    break;
                case 2:
                    _ai.Idle();
                    StealBrainrot_Brainrot brainrot = FindNearestBrainrot();
                    if (brainrot != null && brainrot.isBought == false)
                    {
                        brainrot.brainrotInfo.Buy(curBase.baseID, brainrot, this);

                        stepBuy = 0;
                        RandomStateChase();
                        _ai.Idle();
                    }
                    break;
            }
        }

        private int stepRtHome = 0;
        private void ChaseHome()
        {
            LDebug.Log<StealBrainrot_AI>($"Start State Chase Home");
            if (stepRtHome == 0)
            {
                FollowNearestWaypoint();
                RandomStateChase();
                stepRtHome++;
            }
            else
            {
                if (stepRtHome == 1)
                {
                    _ai.Chase(curBase.playerSpawnPosition);
                    stepRtHome = 0;
                    RandomStateChase();
                }
            }

        }
        private int stepChasePlayer = 0;
        public void ChasePlayer()
        {
            switch (stepChasePlayer)
            {
                case 0:
                    FollowNearestWaypoint();
                    stepChasePlayer++;
                    break;
                case 1:
                    Character c = Player.Instance.character;
                    if (_isStealing == false) _ai.Chase(c.transform.position);
                    else
                    {
                        break;
                    }

                    if (c.transform.GetComponent<StealBrainrot_Player>().isStealing == false)
                    {
                        stepChasePlayer = 0;
                        _state = AIState.ReturnHome;
                        _ai.Idle();
                    }
                    break;
            }

        }

        private int stepFollowWP = 0;
        private void FollowWaypoint()
        {
            LDebug.Log<StealBrainrot_AI>($"Start State Follow Waypoint");
            if (stepFollowWP == 0)
            {
                FollowNearestWaypoint();
                stepFollowWP = 1;
            }
            else
            {
                AIWaypoint waypoint = AIWaypointManager.Instance.waypoints.GetRandom();
                _ai.Chase(waypoint.GetRandomPosition());
                RandomStateChase();
                stepFollowWP = 0;
            }
        }

        private int stepLockDoor = 0;
        private void ChaseLockDoor()
        {
            LDebug.Log<StealBrainrot_AI>($"Start State Lock Door");
            switch (stepLockDoor)
            {
                case 0:
                    _ai.Chase(curBase.playerSpawnPosition);
                    stepLockDoor++;
                    break;
                case 1:
                    _ai.Chase(curBase.lockButton);
                    stepLockDoor = 0;
                    RandomStateChase();
                    break;
            }
        }

        private StealBrainrot_Brainrot FindNearestBrainrot()
        {
            var list = _ai.character.fov.interactables;
            if (list == null || list.Count == 0)
                return null;

            StealBrainrot_Brainrot nearest = null;
            float minSqrDist = float.MaxValue;
            Vector3 currentPos = _ai.character.transform.position;

            foreach (var obj in list)
            {
                if (obj == null) continue;

                var brainrot = obj.GetComponentInParent<StealBrainrot_Brainrot>();
                if (brainrot == null) continue;

                float sqrDist = (brainrot.transform.position - currentPos).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearest = brainrot;
                }
            }

            return nearest;
        }

        [Title("Attack Config")]
        [SerializeField, Min(0.01f)] private float _attackCooldown = 1.25f;
        private float _attackCdTimer = 0f;
        private void Attack()
        {
            if (_state != AIState.ChasePlayer) return;

            if (_attackCdTimer > 0f)
            {
                _attackCdTimer -= Time.deltaTime;
                return;
            }

            Debug.Log("[AI] Attack");

            _attackCdTimer = _attackCooldown; 
        }
    }
}
