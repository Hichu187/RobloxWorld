using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum Minigame { EaseObby, MegaObby, TowerTroll, TowerHell}
    public class AiManager : MonoBehaviour
    {
        [SerializeField] private Minigame minigame;
        [SerializeField] private AI _aiPrefab;
        [SerializeField] private int amount;
        [SerializeField] private List<Transform> _spawnPos;
        [SerializeField] private Transform _spawnParent;

        private List<AI> _ais = new List<AI>();
        private void Start()
        {
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

            int indexBase = 0;

            switch (minigame)
            {
                case Minigame.EaseObby:
                    indexBase = DataAchievement.easyObbyCheckpoint;
                    break;
                case Minigame.MegaObby:
                    indexBase = DataAchievement.megaObbyCheckpoint;
                    break;
                case Minigame.TowerTroll:
                    break;
                case Minigame.TowerHell:
                    break;
            }

            for (int i = 0; i <= amount; i++)
            {
                AI ai = _aiPrefab.Create(_spawnParent);

                int index = indexBase + Random.Range(-2,3);

                index = Mathf.Clamp(index, 0, _spawnPos.Count - 1);

                ai.character.motor.SetPositionAndRotation(_spawnPos[index].position, _spawnPos[index].rotation);
                ai.gameObject.AddComponent<AIFollowWaypoint>();

                _ais.Add(ai);
            }
        }

        public void EventAIDead(Event_AI_Dead e)
        {
            BaseGameplay gameplay = FindAnyObjectByType<BaseGameplay>();

            int posIndex = gameplay.CurrentCheckpointIndex() + Random.Range(-2, 3);
            posIndex = Mathf.Clamp(posIndex, 0, _spawnPos.Count - 1);

            e.ai.character.Revive(_spawnPos[posIndex].position, _spawnPos[posIndex].rotation);
        }
    }
}
