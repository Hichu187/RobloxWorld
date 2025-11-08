using Cysharp.Threading.Tasks;
using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class BrainrotEvoGameplay : BaseGameplay
    {
        [SerializeField] Transform _mapParent;

        [Title("AI")]
        private AIWaypointManager _waypointManager;
        [SerializeField] AI _aiPrefab;

        private List<AI> _ais = new List<AI>();
        private GameObject _currentMap;
        private async void Awake()
        {
            _currentMap = FactoryBrainrotEvo.maps[DataBrainrotEvo.currentMap].Create(_mapParent);

            await UniTask.WaitForEndOfFrame();

            _waypointManager = FindAnyObjectByType<AIWaypointManager>();

            for(int i = 0; i < 5; i++)
            {
                var ai = _aiPrefab.Create(_mapParent);

                ai.character.Revive(_waypointManager.waypoints[i].transform.position, _waypointManager.waypoints[i].transform.rotation);

                ai.gameObject.AddComponent<AIFollowWaypoint>();

                _ais.Add(ai);
            }
        }

        public override void Start()
        {
            base.Start();

            Debug.Log("Test");
        }


        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Subscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Unsubscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
        }

        public void EventChangeMapSpace(Event_BrainrotEvo_Change_Space e)
        {
            LDebug.Log<BrainrotEvoGameplay>($"CHANGE MAP");
            SceneLoaderHelper.Reload();
        }

        public override async void EventPlayerDead(Event_Player_Dead e)
        {
            base.EventPlayerDead(e);

            UINotificationText.Push("YOU DIED");

            await UniTask.WaitForSeconds(0.5f);
            if (startPosition != null)
                player.character.Revive(startPosition.position, startPosition.rotation);

            player.GetComponent<BrainrotEvoPlayer>().InitData();

            await UniTask.WaitForSeconds(0.25f);
            player.character.motor.enabled = true;
            player.character.cCombat.ReSpawn();

        }
    }
}
