using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Game
{
    public class StealBrainrotPool : MonoPool<StealBrainrot_Brainrot, StealBrainrotPool> { }

    public class StealBrainrot_Spawner : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private StealBrainrot_Brainrot _prefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _startPos;
        [SerializeField] private Transform _endPos;

        [Title("Spawn Settings")]
        [SerializeField, Min(0.1f)] private float _spawnInterval = 2f; // thời gian giữa các lần spawn
        [SerializeField] private int _spawnCount = 10; // số lượng spawn tối đa, -1 = vô hạn

        private Coroutine _spawnRoutine;

        private void Awake()
        {
            StealBrainrotPool.instance.Configure(_prefab, _parent);
            StealBrainrotPool.instance.Prewarm(20);
        }
        private void Start()
        {
            StartAutoSpawn();
        }

        [Button("Spawn Test")]
        private void SpawnOne()
        {
            var obj = StealBrainrotPool.instance.Get();
            obj.transform.position = _startPos.position;
            obj.transform.rotation = Quaternion.identity;

            obj.SetPosition(_startPos, _endPos);
            obj.target = _endPos;
            obj.canMove = true;
        }

        [Button("Start Auto Spawn")]
        private void StartAutoSpawn()
        {
            if (_spawnRoutine != null)
                StopCoroutine(_spawnRoutine);

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
    }
}
