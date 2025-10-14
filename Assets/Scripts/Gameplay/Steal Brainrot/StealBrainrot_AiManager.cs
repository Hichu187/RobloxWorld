using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_AiManager : MonoBehaviour
    {
        [SerializeField] private AI _aiPrefab;

        [SerializeField] private List<Transform> _spawnPos;
        [SerializeField] private Transform _spawnParent;

        private void Start()
        {
            SpawnAIByListPosition();
        }

        public void SpawnAIByListPosition()
        {
            for (int i = 0; i < _spawnPos.Count; i++)
            {
                AI ai = _aiPrefab.Create(_spawnParent);

                ai.character.motor.SetPositionAndRotation(_spawnPos[i].position, _spawnPos[i].rotation);
                ai.gameObject.AddComponent<AIFollowWaypoint>();

            }
        }
    }
}
