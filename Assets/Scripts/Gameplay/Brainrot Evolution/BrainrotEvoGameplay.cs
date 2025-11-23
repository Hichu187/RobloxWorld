using Cysharp.Threading.Tasks;
using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;

namespace Game
{
    public class BrainrotEvoGameplay : BaseGameplay
    {
        [SerializeField] Transform _mapParent;
        [SerializeField] LineRenderer _lineTutorial;

        [Title("AI")]
        private AIWaypointManager _waypointManager;
        [SerializeField] AI _aiPrefab;

        private List<AI> _ais = new List<AI>();
        private GameObject _currentMap;

        // --- Tutorial egg line ---
        [SerializeField] private BrainrotEvoEgg _tutorialEggTarget;

        private async void Awake()
        {
            _currentMap = FactoryBrainrotEvo.maps[DataBrainrotEvo.currentMap].Create(_mapParent);

            await UniTask.WaitForEndOfFrame();

            _waypointManager = FindAnyObjectByType<AIWaypointManager>();

            for (int i = 0; i < 5; i++)
            {
                var ai = _aiPrefab.Create(_mapParent);

                ai.character.Revive(_waypointManager.waypoints[i].transform.position,
                                    _waypointManager.waypoints[i].transform.rotation);

                ai.gameObject.AddComponent<AIFollowWaypoint>();

                _ais.Add(ai);
            }

            // Đảm bảo line tắt lúc đầu
            if (_lineTutorial != null)
            {
                _lineTutorial.positionCount = 0;
                _lineTutorial.enabled = false;
                _lineTutorial.useWorldSpace = true;
            }
        }

        public override void Start()
        {
            base.Start();
            Debug.Log("Test");
        }

        private void Update()
        {
            UpdateTutorialLine();
        }

        private void UpdateTutorialLine()
        {
            if (_lineTutorial == null) return;
            if (_tutorialEggTarget == null) return;
            if (player == null || player.character == null) return;

            _lineTutorial.SetPosition(0, player.character.transform.position);
            _lineTutorial.SetPosition(1, _tutorialEggTarget.transform.position);
        }

        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Subscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
            StaticBus<Event_Cash_Update>.Subscribe(TutorialEgg);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();
            StaticBus<Event_BrainrotEvo_Change_Space>.Unsubscribe(EventChangeMapSpace);
            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
            StaticBus<Event_Cash_Update>.Unsubscribe(TutorialEgg);
        }

        public void EventChangeMapSpace(Event_BrainrotEvo_Change_Space e)
        {
            LDebug.Log<BrainrotEvoGameplay>($"CHANGE MAP");
            SceneLoaderHelper.Reload();
        }

        public void TutorialEgg(Event_Cash_Update e)
        {
            LDebug.Log<BrainrotEvoGameplay>("[TutorialEgg] Bắt đầu xử lý");

            if (DataBrainrotEvo.isTutorial)
            {
                LDebug.Log<BrainrotEvoGameplay>("[TutorialEgg] Đã ở tutorial → Tắt line");
                _lineTutorial.enabled = false;
                return;
            }

            if (DataBrainrotEvo.cash < 10)
            {
                LDebug.Log<BrainrotEvoGameplay>($"[TutorialEgg] Cash = {DataBrainrotEvo.cash}, chưa đủ 10 → STOP");
                return;
            }

            DataBrainrotEvo.Tutorial();

            _tutorialEggTarget = FindObjectsByType<BrainrotEvoEgg>(FindObjectsSortMode.None)
                                 .FirstOrDefault(x => x.id == 0);

            if (_tutorialEggTarget == null)
            {
                return;
            }

            if (_lineTutorial == null)
            {
                return;
            }

            if (player == null || player.character == null)
            {
                return;
            }

            LDebug.Log<BrainrotEvoGameplay>("[TutorialEgg] SETUP LineRenderer");

            _lineTutorial.enabled = true;
            _lineTutorial.useWorldSpace = true;
            _lineTutorial.positionCount = 2;

            _lineTutorial.SetPosition(0, player.character.transform.position);
            _lineTutorial.SetPosition(1, _tutorialEggTarget.transform.position);

            LDebug.Log<BrainrotEvoGameplay>("[TutorialEgg] Line tutorial đã được bật ✔");
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
