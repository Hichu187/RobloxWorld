using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public enum AIState
    {
        Empty,
        Idle,
        BuyPet,
        StealPet,
        ChasePlayer,
        ReturnHome,
        FollowWaypoint,
        LockDoor
    }

    [DisallowMultipleComponent]
    public class StealBrainrot_AI : MonoBehaviour
    {
        [Title("Refs")]
        public StealBrainrot_Base curBase;

        private AI _ai;
        private AIState _state;
        private AIState _lastNonEmptyState = AIState.Empty;
        private AIWaypoint _curWaypoint;

        private int stepBuy;
        private int stepRtHome;
        private int stepChasePlayer;
        private int stepFollowWP;
        private int stepLockDoor;
        private int stepSteal;

        [Title("Attack")]
        [SerializeField, Min(0.01f)] private float _attackCooldown = 1.25f;
        private float _attackCdTimer;

        [Title("State Weights")]
        [SerializeField, Min(0f)] private float wBuyPet = 0.5f; // giảm tỉ lệ Buy
        [SerializeField, Min(0f)] private float wStealPet = 0.25f;
        [SerializeField, Min(0f)] private float wChasePlayer = 0.05f;
        [SerializeField, Min(0f)] private float wReturnHome = 1f;
        [SerializeField, Min(0f)] private float wFollowWaypoint = 2f;
        [SerializeField, Min(0f)] private float wLockDoor = 1f;

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
            SetState(AIState.Empty);
            _ai.Idle();
        }

        private void OnDestroy()
        {
            if (_ai != null)
            {
                _ai.eventChaseComplete -= AI_EventChaseComplete;
                _ai.eventIdleComplete -= AI_EventIdleComplete;
                if (_ai.character != null) _ai.character.eventDie -= Revive;
            }
        }

        private void Update()
        {
            Attack();
        }

        public AIState GetState() => _state;

        public void SetState(AIState newState)
        {
            if (_state == newState) return;

            _state = newState;
            stepBuy = stepRtHome = stepChasePlayer = stepFollowWP = stepLockDoor = 0;

            if (_state != AIState.Empty)
                _lastNonEmptyState = _state;

            //LDebug.Log<StealBrainrot_AI>($"[{name}] State -> {_state}");
        }

        private void HandleState()
        {
            switch (_state)
            {
                case AIState.Empty:
                    return;

                case AIState.Idle:
                    _ai.Idle();
                    break;

                case AIState.BuyPet:
                    ChaseBuyPet();
                    break;

                case AIState.StealPet:
                    ChaseStealPet();
                    break;

                case AIState.ChasePlayer:
                    ChasePlayer();
                    break;

                case AIState.ReturnHome:
                    ChaseHome();
                    break;

                case AIState.FollowWaypoint:
                    FollowWaypoint();
                    break;

                case AIState.LockDoor:
                    ChaseLockDoor();
                    break;
            }
        }

        public void AI_EventChaseComplete()
        {
            _ai.Idle();
        }

        public void AI_EventIdleComplete()
        {
            if (_state == AIState.Empty)
            {
                RandomStateChase();
            }
            HandleState();
        }

        public void RandomStateChase()
        {
            AIState[] states =
            {
                AIState.BuyPet,
                AIState.StealPet,
                AIState.ChasePlayer,
                AIState.ReturnHome,
                AIState.FollowWaypoint,
                AIState.LockDoor
            };

            float[] weights =
            {
                wBuyPet,
                wStealPet,
                wChasePlayer,
                wReturnHome,
                wFollowWaypoint,
                wLockDoor
            };

            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] == _lastNonEmptyState)
                    weights[i] = 0f;
                else
                    weights[i] = Mathf.Max(0f, weights[i]);
            }

            float total = 0f;
            for (int i = 0; i < weights.Length; i++) total += weights[i];

            if (total <= 0f)
            {
                for (int i = 0; i < states.Length; i++)
                    weights[i] = states[i] == _lastNonEmptyState ? 0f : 1f;
                total = 5f;
            }

            float r = Random.Range(0f, total);
            float acc = 0f;
            AIState pick = states[0];

            for (int i = 0; i < states.Length; i++)
            {
                acc += weights[i];
                if (r <= acc)
                {
                    pick = states[i];
                    break;
                }
            }

            SetState(pick);
        }

        private void Revive()
        {
            _ai.Stop();
            SetState(AIState.Empty);
            _ai.Idle();
        }

        public void FollowNearestWaypoint()
        {
            _curWaypoint = AIWaypointManager.Instance.GetNearestWaypoint(_ai.character.transformCached.position);
            _ai.Chase(_curWaypoint.GetRandomPosition());
        }

        private void ChaseBuyPet()
        {
            switch (stepBuy)
            {
                case 0:
                    FollowNearestWaypoint();
                    stepBuy = 1;
                    break;

                case 1:
                    {
                        var list = StealBrainrot_AiManager.instance.buyBrainrotWaypoints;
                        if (list == null || list.Count == 0)
                        {
                            SetState(AIState.Empty);
                            _ai.Idle();
                            break;
                        }

                        var aiWaypoint = list.GetRandom();
                        _ai.Chase(aiWaypoint.GetRandomPosition());
                        stepBuy = 2;
                        break;
                    }

                case 2:
                    {
                        var slot = curBase ? curBase.GetFirstEmptySlot() : null;
                        if (slot == null)
                        {
                            stepBuy = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            break;
                        }

                        var brainrot = FindNearestBrainrot();
                        if (brainrot != null && brainrot.isBought == false)
                        {
                            brainrot.brainrotInfo.Buy(curBase.baseID, brainrot, this);
                            LDebug.Log<StealBrainrot_AI>("Buy Brainrot");
                            stepBuy = 0;
                        }

                        SetState(AIState.Empty);
                        _ai.Idle();
                        break;
                    }
            }
        }
        [SerializeField] private Transform holdingPos;
        public bool isStealing = false;
        private StealBrainrot_Brainrot _takedBrainrot;
        private Transform _preTrans;
        private StealBrainrot_Slot _victimSlot;
        private StealBrainrot_Slot _targetSlot;

        private void ChaseStealPet()
        {
            LDebug.Log<StealBrainrot_AI>($"[STEAL] → Case {stepSteal}");

            switch (stepSteal)
            {
                case 0:
                    LDebug.Log<StealBrainrot_AI>("[STEAL] Case 0 → Kiểm tra slot trống của base mình");
                    _targetSlot = curBase != null ? curBase.GetFirstEmptySlot() : null;
                    
                    if (_targetSlot == null)
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Base mình KHÔNG có slot trống → Kết thúc trộm");
                        stepSteal = 4;
                        goto case 4;
                    }

                    if (StealBrainrot_Manager.instance.baseLists[_targetSlot.baseId].buttonLock.isLocked)
                    {
                        stepSteal = 4;
                        goto case 4;
                    }

                    LDebug.Log<StealBrainrot_AI>("[STEAL] Base mình có slot trống → Đi waypoint chuẩn bị trộm");
                    FollowNearestWaypoint();
                    stepSteal = 1;
                    break;

                case 1:
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 1 → Tìm slot của base khác có Pet để trộm");

                        var all = FindObjectsOfType<StealBrainrot_Slot>();
                        StealBrainrot_Slot best = null;
                        float bestSqr = float.MaxValue;
                        Vector3 pos = _ai.character.transform.position;

                        foreach (var s in all)
                        {
                            if (s == null || s.isEmpty) continue;
                            if (curBase != null && curBase.slots.Contains(s)) continue;

                            float d = (s.transform.position - pos).sqrMagnitude;
                            if (d < bestSqr)
                            {
                                bestSqr = d;
                                best = s;
                            }
                        }

                        if (best == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Không tìm thấy slot hợp lệ để trộm!");
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        _victimSlot = best;
                        LDebug.Log<StealBrainrot_AI>($"[STEAL] Đã chọn slot mục tiêu: {_victimSlot.name}");
                        _ai.Chase(best.transform.position);
                        stepSteal = 2;
                        break;
                    }

                case 2:
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 2 → Đến vị trí nạn nhân, thực hiện StealingBrainrot()");

                        var br = _victimSlot != null ? _victimSlot.brainrot : null;
                        if (br == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Slot nạn nhân không còn brainrot!");
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        isStealing = true;

                        StealingBrainrot(br);
                        LDebug.Log<StealBrainrot_AI>($"[STEAL] Đã nhặt brainrot: {br.name}");

                        _targetSlot ??= curBase != null ? curBase.GetFirstEmptySlot() : null;
                        if (_targetSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Base mình KHÔNG còn slot trống → Reset trộm");
                            ResetSteal();
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        LDebug.Log<StealBrainrot_AI>($"[STEAL] Đang mang brainrot về slot {_targetSlot.name}");
                        _ai.Chase(_targetSlot.transform.position);
                        stepSteal = 3;
                        break;
                    }

                case 3:
                    LDebug.Log<StealBrainrot_AI>("[STEAL] Case 3 → Đặt brainrot vào slot của mình");
                    StealingDone(_targetSlot);
                    LDebug.Log<StealBrainrot_AI>("[STEAL] Đã hoàn tất trộm, trở về trạng thái rỗng");
                    stepSteal = 4;
                    break;

                case 4:
                    LDebug.Log<StealBrainrot_AI>("[STEAL] Case 4 → Kết thúc hành động trộm, set về Empty");
                    SetState(AIState.Empty);
                    _ai.Idle();
                    break;

                default:
                    LDebug.Log<StealBrainrot_AI>($"[STEAL] Case {stepSteal} không hợp lệ, reset về Empty");
                    stepSteal = 0;
                    SetState(AIState.Empty);
                    _ai.Idle();
                    break;
            }
        }

        public void StealingBrainrot(StealBrainrot_Brainrot brainrot)
        {
            _takedBrainrot = brainrot;
            _preTrans = brainrot.transform.parent;
            brainrot.targetSlot.isEmpty = true;
            brainrot.canMove = false;
            brainrot.targetSlot.brainrot = null;
            brainrot.targetSlot.StopGenerating();

            // gắn lên tay AI
            brainrot.transform.SetParent(_ai.characterHoldingPos, worldPositionStays: false);
            brainrot.transform.localPosition = Vector3.zero;
            brainrot.transform.localRotation = Quaternion.identity;
        }

        public void StealingDone(StealBrainrot_Slot slot)
        {
            if (_takedBrainrot == null || slot == null) return;

            if(_takedBrainrot.indBase == 0)
            {
                DataStealBrainrot.RemoveBaseSlot(_takedBrainrot.targetSlot.slotId);
            }

            _takedBrainrot.targetSlot = slot;
            _takedBrainrot.target = slot.transform;
            _takedBrainrot.indBase = slot.baseId;

            slot.SetBrainrot(_takedBrainrot);
            slot.StartGenerating();

            int slotIndex = curBase != null ? curBase.slots.IndexOf(slot) : -1;
            if (slotIndex >= 0)
                DataStealBrainrot.AddOrUpdateBaseSlot(slotIndex, _takedBrainrot.bConfig.ID);

            ResetSteal();
        }

        [Button]
        public void ResetSteal()
        {
            if (_takedBrainrot != null)
            {
                _takedBrainrot.transform.SetParent(_preTrans, worldPositionStays: true);
                _takedBrainrot.isMovingHome = true;
                _takedBrainrot.canMove = true;
            }
            isStealing = false;
            _takedBrainrot = null;
            _preTrans = null;
            _victimSlot = null;
            _targetSlot = null;
        }



        private void ChaseHome()
        {
            if (stepRtHome == 0)
            {
                FollowNearestWaypoint();
                stepRtHome = 1;
            }
            else
            {
                if (curBase != null) _ai.Chase(curBase.playerSpawnPosition);
                stepRtHome = 0;
                SetState(AIState.Empty);
                //_ai.Idle();
            }
        }

        private void ChasePlayer()
        {
            switch (stepChasePlayer)
            {
                case 0:
                    FollowNearestWaypoint();
                    stepChasePlayer = 1;
                    break;

                case 1:
                    {
                        var p = FindAnyObjectByType<Player>();
                        if (p == null)
                        {
                            stepChasePlayer = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            break;
                        }

                        var c = p.character;
                        var sp = c.GetComponent<StealBrainrot_Player>();

                        _ai.Chase(c.transform.position);

                        if (sp != null && sp.isStealing == false)
                        {
                            stepChasePlayer = 0;
                            //SetState(AIState.Empty);
                            //_ai.Idle();
                        }
                        break;
                    }
            }
        }

        private void FollowWaypoint()
        {
            if (stepFollowWP == 0)
            {
                FollowNearestWaypoint();
                stepFollowWP = 1;
            }
            else
            {
                var waypoint = AIWaypointManager.Instance.waypoints.GetRandom();
                _ai.Chase(waypoint.GetRandomPosition());
                stepFollowWP = 0;

                SetState(AIState.Empty);
                //_ai.Idle();
            }
        }

        private void ChaseLockDoor()
        {
            switch (stepLockDoor)
            {
                case 0:
                    if (curBase != null) _ai.Chase(curBase.playerSpawnPosition);
                    stepLockDoor = 1;
                    break;

                case 1:
                    if (curBase != null) _ai.Chase(curBase.lockButton);
                    stepLockDoor = 0;

                    SetState(AIState.Empty);
                    //_ai.Idle();
                    break;
            }
        }

        private StealBrainrot_Brainrot FindNearestBrainrot()
        {
            var list = _ai.character.fov.interactables;
            if (list == null || list.Count == 0) return null;

            StealBrainrot_Brainrot nearest = null;
            float minSqrDist = float.MaxValue;
            var currentPos = _ai.character.transform.position;

            foreach (var obj in list)
            {
                if (obj == null) continue;

                var brainrot = obj.GetComponentInParent<StealBrainrot_Brainrot>();
                if (brainrot == null) continue;

                var d = (brainrot.transform.position - currentPos).sqrMagnitude;
                if (d < minSqrDist)
                {
                    minSqrDist = d;
                    nearest = brainrot;
                }
            }

            return nearest;
        }

        private void Attack()
        {
            // Giảm cooldown mỗi frame
            if (_attackCdTimer > 0f)
            {
                _attackCdTimer -= Time.deltaTime;
                return;
            }

            // Kiểm tra state
            if (_state != AIState.ChasePlayer)
            {
                return;
            }

            // Bảo vệ null
            var ch = _ai?.character;
            var fov = ch?.fov;
            var player = FindAnyObjectByType<Player>();
            var playerT = player != null ? player.character?.transform : null;

            if (fov == null)
            {
                return;
            }
            if (playerT == null)
            {
                return;
            }
            if (fov.combatables == null)
            {
                return;
            }

            // Kiểm tra Player có trong combat range không
            bool playerInRange = false;
            foreach (var t in fov.combatables)
            {
                if (t == null) continue;

                if (t == playerT || t.IsChildOf(playerT) || playerT.IsChildOf(t))
                {
                    playerInRange = true;
                    break;
                }
            }

            if (!playerInRange)
            {
                LDebug.Log<StealBrainrot_AI>("Player NOT in range -> no attack");
                return;
            }

            // Nếu qua được hết kiểm tra

            LDebug.Log<StealBrainrot_AI>("ATTACK TRIGGERED!");
            _ai.character.cCombat.Attack(_ai.character.fov);
            _attackCdTimer = _attackCooldown;

            SetState(AIState.Empty);
        }


    }
}
