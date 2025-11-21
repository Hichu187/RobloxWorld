using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
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
        [SerializeField, Min(0f)] private float wBuyPet = 40f;
        [SerializeField, Min(0f)] private float wStealPet = 25f;
        [SerializeField, Min(0f)] private float wChasePlayer = 5f;
        [SerializeField, Min(0f)] private float wReturnHome = 5f;
        [SerializeField, Min(0f)] private float wFollowWaypoint = 10f;
        [SerializeField, Min(0f)] private float wLockDoor = 5f;

        [SerializeField] private Transform holdingPos;
        public bool isStealing = false;
        private StealBrainrot_Brainrot _takedBrainrot;
        private Transform _preTrans;
        private StealBrainrot_Slot _victimSlot;
        private StealBrainrot_Slot _targetSlot;
        private Transform victimSpawn;
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
        private void ChaseStealPet()
        {
            LDebug.Log<StealBrainrot_AI>($"[STEAL] → Case {stepSteal}");

            switch (stepSteal)
            {
                case 0:
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 0 → Kiểm tra slot trống của base mình");

                        _targetSlot = curBase != null ? curBase.GetFirstEmptySlot() : null;

                        if (_targetSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Base mình KHÔNG có slot trống → Kết thúc trộm");
                            stepSteal = 5;
                            goto case 5;
                        }

                        if (StealBrainrot_Manager.instance.baseLists[_targetSlot.baseId].buttonLock.isLocked)
                        {
                            stepSteal = 5;
                            goto case 5;
                        }

                        LDebug.Log<StealBrainrot_AI>("[STEAL] Base mình có slot trống → Đi waypoint chuẩn bị trộm");
                        FollowNearestWaypoint();
                        stepSteal = 1;
                        break;
                    }

                case 1:
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 1 → Tìm slot của base khác có Pet để trộm (ngẫu nhiên có trọng số)");

                        var all = FindObjectsByType<StealBrainrot_Slot>(FindObjectsSortMode.None);
                        var validSlots = new List<StealBrainrot_Slot>();
                        Vector3 pos = _ai.character.transform.position;

                        float chanceStealFromBase0 = StealBrainrot_Manager.instance.chanceStealFromBase0;
                        bool stealFromBase0 = UnityEngine.Random.value <= chanceStealFromBase0;

                        foreach (var s in all)
                        {
                            if (s == null || s.isEmpty) continue;
                            if (curBase != null && curBase.slots.Contains(s)) continue;

                            if (stealFromBase0)
                            {
                                if (s.baseId == 0)
                                    validSlots.Add(s);
                            }
                            else
                            {
                                if (s.baseId != 0)
                                    validSlots.Add(s);
                            }
                        }

                        if (validSlots.Count == 0)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Không tìm thấy slot hợp lệ để trộm!");
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        float totalWeight = 0f;
                        var weights = new float[validSlots.Count];
                        for (int i = 0; i < validSlots.Count; i++)
                        {
                            float dist = (validSlots[i].transform.position - pos).sqrMagnitude;
                            float w = Mathf.Max(0.01f, 1f / (dist + 1f));
                            weights[i] = w;
                            totalWeight += w;
                        }

                        float pick = UnityEngine.Random.Range(0f, totalWeight);
                        float acc = 0f;
                        StealBrainrot_Slot best = validSlots[0];
                        for (int i = 0; i < validSlots.Count; i++)
                        {
                            acc += weights[i];
                            if (pick <= acc)
                            {
                                best = validSlots[i];
                                break;
                            }
                        }

                        _victimSlot = best;
                        LDebug.Log<StealBrainrot_AI>($"[STEAL] Đã chọn slot mục tiêu (ngẫu nhiên): {_victimSlot.name} (from base {best.baseId})");

                        var victimBase = StealBrainrot_Manager.instance.baseLists[best.baseId];
                        victimSpawn = victimBase != null && victimBase.playerSpawnPosition != null
                            ? victimBase.playerSpawnPosition
                            : best.transform;

                        _ai.Chase(victimSpawn.position);
                        stepSteal = 2;
                        break;
                    }

                case 2:
                    {
                        if (_victimSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Case 2 → VictimSlot null → Reset");
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 2 → Đã tới base victim, chạy tới slot victim");
                        _ai.Chase(_victimSlot.transform.position);
                        stepSteal = 3;
                        break;
                    }

                case 3:
                    {
                        var fov = _ai?.character?.fov;
                        var br = _victimSlot != null ? _victimSlot.brainrot : null;
                        if (fov?.interactables == null || br == null)
                        {
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        bool seen = false;
                        foreach (var t in fov.interactables)
                        {
                            if (t == null) continue;
                            if (t == br.transform || t.IsChildOf(br.transform) || br.transform.IsChildOf(t))
                            {
                                seen = true;
                                break;
                            }
                        }

                        if (!seen)
                        {
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        isStealing = true;
                        StealingBrainrot(br);

                        _targetSlot ??= curBase != null ? curBase.GetFirstEmptySlot() : null;
                        if (_targetSlot == null)
                        {
                            ResetSteal();
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        if(_takedBrainrot.indBase == 0)
                        {
                            //UINotificationText.Push("Alert! Someone just stole a Brainrot from your base!");
                        }

                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 3 → Đã cầm brainrot, chạy về slot của base mình");
                        _ai.Chase(victimSpawn.transform.position);
                        stepSteal = 4;
                        break;
                    }

                case 4:
                    {
                        if (_targetSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Case 4 → TargetSlot null → Reset");
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        Transform selfSpawn = curBase != null && curBase.playerSpawnPosition != null
                            ? curBase.playerSpawnPosition
                            : _targetSlot.transform;

                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 4 → Đã ra spawn victim, chạy về spawn base mình");
                        _ai.Chase(selfSpawn.position);
                        stepSteal = 5;
                        break;
                    }

                case 5:
                    {
                        if (_targetSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] Case 5 → TargetSlot null → Reset");
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 5 → Đã tới spawn base mình, chạy tới slot của mình");
                        _ai.Chase(_targetSlot.transform.position);
                        stepSteal = 6;
                        break;
                    }

                case 6:
                    {
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Case 6 → Đặt brainrot vào slot của mình");

                        if (_targetSlot == null)
                        {
                            LDebug.Log<StealBrainrot_AI>("[STEAL] TargetSlot null → Reset");
                            stepSteal = 0;
                            SetState(AIState.Empty);
                            _ai.Idle();
                            return;
                        }

                        StealingDone(_targetSlot);
                        LDebug.Log<StealBrainrot_AI>("[STEAL] Đã hoàn tất trộm, trở về trạng thái rỗng");
                        stepSteal = 0;
                        SetState(AIState.Empty);
                        _ai.Idle();
                        break;
                    }

                default:
                    {
                        LDebug.Log<StealBrainrot_AI>($"[STEAL] Case {stepSteal} không hợp lệ, reset về Empty");
                        stepSteal = 0;
                        SetState(AIState.Empty);
                        _ai.Idle();
                        break;
                    }
            }
        }
        public void StealingBrainrot(StealBrainrot_Brainrot brainrot)
        {
            isStealing = true;

            _takedBrainrot = brainrot;
            _preTrans = brainrot.transform.parent;

            if (_takedBrainrot.indBase == 0)
            {
                StealBrainrot_View view = FindAnyObjectByType<StealBrainrot_View>();
                view.StolenNotice();
            }

            // gắn lên tay AI
            _ai.character.cAnim.SetSteal(isStealing);
            brainrot.StealBrainrot(_ai.characterHoldingPos);
        }
        public void StealingDone(StealBrainrot_Slot slot)
        {
            if (_takedBrainrot == null || slot == null) return;

            if(_takedBrainrot.indBase == 0)
            {
                DataStealBrainrot.RemoveBaseSlot(_takedBrainrot.targetSlot.slotId);
            }

            _takedBrainrot.targetSlot.isEmpty = true;
            _takedBrainrot.targetSlot.brainrot = null;

            _takedBrainrot.targetSlot = slot;
            _takedBrainrot.target = slot.transform;
            _takedBrainrot.indBase = slot.baseId;

            slot.SetBrainrot(_takedBrainrot);
            slot.StartGenerating();

            ResetSteal();
        }
        public void ResetSteal()
        {
            if (_takedBrainrot != null)
            {
                _takedBrainrot.target = _takedBrainrot.targetSlot.transform;
                _takedBrainrot.transform.SetParent(_preTrans, worldPositionStays: true);
                _takedBrainrot.isMovingHome = true;
                _takedBrainrot.canMove = true;
            }
            isStealing = false;
            _ai.character.cAnim.SetSteal(isStealing);
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

                        _ai.Chase(c.transform);

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
