using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class StealBrainrotPool : MonoPool<StealBrainrot_Brainrot, StealBrainrotPool> { }

    public class StealBrainrot_Spawner : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private StealBrainrot_Base playerBase;
        [SerializeField] private StealBrainrot_Brainrot _prefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _startPos;
        [SerializeField] private Transform _endPos;

        [Title("Spawn Settings")]
        [SerializeField, Min(0.1f)] private float _spawnInterval = 2f;
        [SerializeField] private int _spawnCount = 10;

        [Title("Rank Weights")]
        [SerializeField] private List<float> _rankWeights = new() { 60f, 25f, 10f, 4f, 1f };

        private Coroutine _spawnRoutine;

        private void Awake()
        {
            StealBrainrotPool.instance.Configure(_prefab, _parent);
            StealBrainrotPool.instance.Prewarm(20);

        }

        private void Start()
        {
            SpawnFromData();
            StartAutoSpawn();
        }

        [Button("Spawn Test")]
        private void SpawnOne()
        {
            if (_startPos == null || _endPos == null) return;

            var obj = StealBrainrotPool.instance.Get();
            PetRank rank = PickRankByWeight();

            List<StealBrainrot_BrainrotConfig> list = null;
            int safeCount = 0;

            while (safeCount < 10)
            {
                list = FactoryStealBrainrot.brainrotConfigs.FindAll(c => c.rank == rank);
                if (list != null && list.Count > 0)
                    break;

                if (rank == PetRank.Common)
                    break;

                rank = (PetRank)((int)rank - 1);
                safeCount++;
            }

            if (list == null || list.Count == 0) return;

            var config = list[Random.Range(0, list.Count)];

            obj.transform.position = _startPos.position;
            obj.transform.rotation = Quaternion.identity;
            obj.InitBrainrotData(config);
            obj.SetPosition(_startPos, _endPos);
            obj.Setup(rank, true);
            obj.target = _endPos;
            obj.canMove = true;
        }

        [Button("Start Auto Spawn")]
        private void StartAutoSpawn()
        {
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _spawnRoutine = StartCoroutine(Co_AutoSpawn());
        }

        [Button("Stop Auto Spawn")]
        private void StopAutoSpawn()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        private IEnumerator Co_AutoSpawn()
        {
            int spawned = 0;
            while (_spawnCount < 0 || spawned < _spawnCount)
            {
                SpawnOne();
                spawned++;
                yield return new WaitForSeconds(_spawnInterval);
            }
            _spawnRoutine = null;
        }

        [Button("Clear Pool")]
        private void ClearPool()
        {
            StealBrainrotPool.instance.Clear();
        }

        private PetRank PickRankByWeight()
        {
            float sum = 0f;
            for (int i = 0; i < _rankWeights.Count; i++) sum += _rankWeights[i];
            if (sum <= 0f) return PetRank.Common;

            float r = Random.value * sum;
            for (int i = 0; i < _rankWeights.Count; i++)
            {
                r -= _rankWeights[i];
                if (r <= 0f) return (PetRank)i;
            }

            return PetRank.Common;
        }

        [Button("Spawn From Data")]
        public void SpawnFromData()
        {
            if (playerBase == null) return;
            if (FactoryStealBrainrot.brainrotConfigs == null || FactoryStealBrainrot.brainrotConfigs.Count == 0)
                return;

            foreach (var kv in DataStealBrainrot.BaseSlots)
            {
                int slotIndex = kv.Key;
                int brainrotId = kv.Value;

                if (slotIndex < 0 || slotIndex >= playerBase.slots.Count) continue;
                if (brainrotId < 0 || brainrotId >= FactoryStealBrainrot.brainrotConfigs.Count) continue;

                var slot = playerBase.slots[slotIndex];
                var config = FactoryStealBrainrot.brainrotConfigs[brainrotId];

                var obj = StealBrainrotPool.instance.Get();
                obj.InitBrainrotData(config);
                obj.Setup(config.rank, true);
                obj.targetSlot = slot;
                obj.transform.position = slot.stayPosition != null ? slot.stayPosition.position : slot.transform.position;
                obj.transform.rotation = slot.stayPosition.rotation * Quaternion.Euler(0, 180, 0);
                obj.indBase = 0;
                obj.isBought = true;
                obj.canMove = false;

                slot.isEmpty = false;
                slot.brainrot = obj;
                slot.StartGenerating();
            }
        }
    }
}
