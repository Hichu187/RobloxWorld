using Hichu;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class Tower : MonoSingleton<Tower>
    {
        [SerializeField] private PlatformCheckpoint[] _checkpoints;

        [SerializeField] private GameObject[] _floorModules;

        private void Start()
        {
            for (int i = 0; i < _checkpoints.Length; i++)
            {
                _checkpoints[i].SetIndex(i);
            }
        }

#if UNITY_EDITOR

        [Button]
        private void TowerSetup()
        {
            float height = 0f;
            Quaternion rotation = Quaternion.identity;

            for (int i = 0; i < _floorModules.Length; i++)
            {
                TowerFloor floor = _floorModules[i].GetComponent<TowerFloor>();

                floor.transform.position = Vector3.up * height;
                floor.transform.rotation = rotation;

                height += floor.height;
            }

            _checkpoints = GetComponentsInChildren<PlatformCheckpoint>();
        }

#endif
    }
}
