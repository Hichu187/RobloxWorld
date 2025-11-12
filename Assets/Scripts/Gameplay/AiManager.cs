using Hichu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum Minigame { EaseObby, MegaObby, TowerTroll, TowerHell }

    [DisallowMultipleComponent]
    public class AiManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Minigame minigame = Minigame.EaseObby;
        [SerializeField] private AI _aiPrefab;
        [SerializeField, Min(0)] private int amount = 5;
        [SerializeField] private List<Transform> _spawnPos = new();
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private bool fallingDetected = false;

        [Header("Auto Respawn (Tower Only)")]
        [SerializeField] private bool autoRespawnAI = false;
        [SerializeField, Min(1f)] private float autoRespawnInterval = 10f;

        private readonly List<AI> _ais = new();
        private Character playerCharacter;
        private BaseGameplay cachedGameplay;
        private Coroutine _autoRespawnRoutine;

        private void Awake()
        {
            playerCharacter = Player.Instance != null ? Player.Instance.character : null;
        }

        private void Start()
        {
            cachedGameplay = FindAnyObjectByType<BaseGameplay>();
            SpawnAI();
            StaticBus<Event_AI_Dead>.Subscribe(EventAIDead);

            if (autoRespawnAI && (minigame == Minigame.TowerTroll || minigame == Minigame.TowerHell))
                _autoRespawnRoutine = StartCoroutine(Co_AutoRespawnLoop());
        }

        private void OnDestroy()
        {
            StaticBus<Event_AI_Dead>.Unsubscribe(EventAIDead);

            if (_autoRespawnRoutine != null)
            {
                StopCoroutine(_autoRespawnRoutine);
                _autoRespawnRoutine = null;
            }
        }

        public void SpawnAI()
        {
            if (_aiPrefab == null || _spawnPos == null || _spawnPos.Count == 0) return;

            int indexBase = GetBaseSpawnIndex();

            for (int i = 0; i < amount; i++)
            {
                AI ai = _aiPrefab.Create(_spawnParent);
                int index = RandomizedIndexAround(indexBase, -2, 3);
                index = Mathf.Clamp(index, 0, _spawnPos.Count - 1);

                var p = _spawnPos[index];
                ai.character.motor.SetPositionAndRotation(p.position, p.rotation);
                ai.gameObject.AddComponent<AIFollowWaypoint>();
                ai.character.GetComponent<CharacterFallingDetector>().enabled = fallingDetected;
                _ais.Add(ai);
            }
        }

        public void EventAIDead(Event_AI_Dead e)
        {
            if (e == null || e.ai == null || _spawnPos == null || _spawnPos.Count == 0) return;

            int posIndex = GetReviveIndex();
            posIndex = Mathf.Clamp(posIndex, 0, _spawnPos.Count - 1);

            var p = _spawnPos[posIndex];
            e.ai.character.Revive(p.position, p.rotation);
        }

        private int GetBaseSpawnIndex()
        {
            if (_spawnPos == null || _spawnPos.Count == 0) return 0;

            switch (minigame)
            {
                case Minigame.EaseObby:
                    return Mathf.Clamp(DataAchievement.easyObbyCheckpoint, 0, _spawnPos.Count - 1);
                case Minigame.MegaObby:
                    return Mathf.Clamp(DataAchievement.megaObbyCheckpoint, 0, _spawnPos.Count - 1);
                case Minigame.TowerTroll:
                case Minigame.TowerHell:
                    var t = GetPlayerTransform();
                    return t != null ? FindNearestSpawnIndex(t.position) : _spawnPos.Count / 2;
                default:
                    return 0;
            }
        }

        private int GetReviveIndex()
        {
            if (_spawnPos == null || _spawnPos.Count == 0) return 0;

            switch (minigame)
            {
                case Minigame.EaseObby:
                case Minigame.MegaObby:
                    int baseIdx = 0;
                    if (cachedGameplay != null)
                        baseIdx = Mathf.Clamp(cachedGameplay.CurrentCheckpointIndex(), 0, _spawnPos.Count - 1);
                    return baseIdx + UnityEngine.Random.Range(-2, 3);

                case Minigame.TowerTroll:
                case Minigame.TowerHell:
                    var t = GetPlayerTransform();
                    int near = t != null ? FindNearestSpawnIndex(t.position) : _spawnPos.Count / 2;
                    return near + UnityEngine.Random.Range(-4, 4);

                default:
                    return UnityEngine.Random.Range(0, _spawnPos.Count);
            }
        }

        private int RandomizedIndexAround(int baseIndex, int inclusiveOffsetMin, int exclusiveOffsetMax)
        {
            int offset = UnityEngine.Random.Range(inclusiveOffsetMin, exclusiveOffsetMax);
            return baseIndex + offset;
        }

        private int FindNearestSpawnIndex(Vector3 srcPos)
        {
            int nearestIndex = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < _spawnPos.Count; i++)
            {
                var sp = _spawnPos[i];
                if (sp == null) continue;

                float d = Vector3.SqrMagnitude(sp.position - srcPos);
                if (d < minDist)
                {
                    minDist = d;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private Transform GetPlayerTransform()
        {
            if (playerCharacter == null && Player.Instance != null)
                playerCharacter = Player.Instance.character;

            return playerCharacter != null ? playerCharacter.transform : null;
        }

        // ========= AUTO RESPAWN (Tower only) =========

        private IEnumerator Co_AutoRespawnLoop()
        {
            var wait = new WaitForSeconds(autoRespawnInterval);
            while (autoRespawnAI && (minigame == Minigame.TowerTroll || minigame == Minigame.TowerHell))
            {
                AutoRespawnTick();
                yield return wait;
            }
        }

        private void AutoRespawnTick()
        {
            if (_ais.Count == 0 || _spawnPos == null || _spawnPos.Count == 0) return;

            var playerT = GetPlayerTransform();
            if (playerT == null) return;

            int nearIdx = FindNearestSpawnIndex(playerT.position);
            nearIdx = Mathf.Clamp(nearIdx, 0, _spawnPos.Count - 1);
            var targetSpawn = _spawnPos[nearIdx];

            var pool = new List<(AI ai, float d2)>(_ais.Count);
            Vector3 pPos = playerT.position;

            for (int i = 0; i < _ais.Count; i++)
            {
                var ai = _ais[i];
                if (ai == null || ai.character == null) continue;
                float d2 = (ai.character.transform.position - pPos).sqrMagnitude;
                pool.Add((ai, d2));
            }
            if (pool.Count == 0) return;

            pool.Sort((a, b) => b.d2.CompareTo(a.d2));

            int reviveCount = Mathf.Min(UnityEngine.Random.Range(1, 3), pool.Count);
            for (int i = 0; i < reviveCount; i++)
            {
                var ai = pool[i].ai;
                if (ai == null || ai.character == null) continue;

                ai.character.Revive(targetSpawn.position, targetSpawn.rotation);
                //Debug.Log($"[AiManager] AutoRespawn → Revive AI '{ai.name}' gần player tại spawn index {nearIdx} (cách cũ: {Mathf.Sqrt(pool[i].d2):0.0}m)");
            }
        }
    }
}
