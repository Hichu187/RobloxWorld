using Hichu;
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

        private readonly List<AI> _ais = new();
        private Character playerCharacter;
        private BaseGameplay cachedGameplay;

        private void Awake()
        {
            playerCharacter = Player.Instance != null ? Player.Instance.character : null;
        }

        private void Start()
        {
            cachedGameplay = FindAnyObjectByType<BaseGameplay>();
            SpawnAI();
            StaticBus<Event_AI_Dead>.Subscribe(EventAIDead);
        }

        private void OnDestroy()
        {
            StaticBus<Event_AI_Dead>.Unsubscribe(EventAIDead);
        }

        public void SpawnAI()
        {
            if (_aiPrefab == null) return;
            if (_spawnPos == null || _spawnPos.Count == 0) return;

            int indexBase = GetBaseSpawnIndex();

            for (int i = 0; i < amount; i++)
            {
                AI ai = _aiPrefab.Create(_spawnParent);
                int index = RandomizedIndexAround(indexBase, -2, 3);
                index = Mathf.Clamp(index, 0, _spawnPos.Count - 1);

                var p = _spawnPos[index];
                ai.character.motor.SetPositionAndRotation(p.position, p.rotation);
                ai.gameObject.AddComponent<AIFollowWaypoint>();
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
                    return baseIdx + Random.Range(-2, 3);
                case Minigame.TowerTroll:
                case Minigame.TowerHell:
                    var t = GetPlayerTransform();
                    int near = t != null ? FindNearestSpawnIndex(t.position) : _spawnPos.Count / 2;
                    return near + Random.Range(-2, 3);
                default:
                    return Random.Range(0, _spawnPos.Count);
            }
        }

        private int RandomizedIndexAround(int baseIndex, int inclusiveOffsetMin, int exclusiveOffsetMax)
        {
            int offset = Random.Range(inclusiveOffsetMin, exclusiveOffsetMax);
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
    }
}
