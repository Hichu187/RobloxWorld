using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_AiManager : MonoBehaviour
    {
        public static StealBrainrot_AiManager instance;

        [SerializeField] private AI _aiPrefab;

        [SerializeField] private List<Transform> _spawnPos;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private List<AIWaypoint> _buyBrainrotWaypoints;
        public List<AIWaypoint> buyBrainrotWaypoints { get { return _buyBrainrotWaypoints; } }


        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            SpawnAIByListPosition();
        }

        public void SpawnAIByListPosition()
        {
            for (int i = 1; i < StealBrainrot_Manager.instance.baseLists.Count; i++)
            {
                AI ai = _aiPrefab.Create(_spawnParent);

                ai.character.motor.SetPositionAndRotation(StealBrainrot_Manager.instance.baseLists[i].playerSpawnPosition.position, StealBrainrot_Manager.instance.baseLists[i].playerSpawnPosition.rotation);
                //ai.gameObject.AddComponent<AIFollowWaypoint>();
                ai.gameObject.AddComponent<StealBrainrot_AI>();

                ai.gameObject.GetComponent<StealBrainrot_AI>().curBase = StealBrainrot_Manager.instance.baseLists[i];
            }
        }
    }
}
