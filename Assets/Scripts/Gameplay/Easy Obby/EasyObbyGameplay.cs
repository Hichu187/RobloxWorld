using Cysharp.Threading.Tasks;
using Hichu;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class EasyObbyGameplay : BaseGameplay
    {
        public override void Start()
        {
            base.Start();

            int curCheckpointIndex = DataAchievement.easyObbyCheckpoint;

            curCheckpoint = checkpoints[curCheckpointIndex];
            player.character.motor.SetPositionAndRotation(checkpoints[curCheckpointIndex].transform.position, checkpoints[curCheckpointIndex].transform.rotation);

            foreach (var c in checkpoints)
            {
                c.ActiveArrow(false);
            }
            checkpoints[DataAchievement.easyObbyCheckpoint + 1].ActiveArrow(true);
        }

        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();

            StaticBus<Event_Player_Dead>.Subscribe(EventPlayerDead);
        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();

            StaticBus<Event_Player_Dead>.Unsubscribe(EventPlayerDead);
        }

        public override async void EventPlayerDead(Event_Player_Dead e)
        {
            base.EventPlayerDead(e);

            Easypapa.AdHelper.ShowInterstitial("player_dead");

            await UniTask.WaitForSeconds(2f);
            RespawnCheckpoint();
        }

        public override void EventCheckpoint(Event_Checkpoint e)
        {
            base.EventCheckpoint(e);

            if (curCheckpoint == e.checkpoint) return;

            int checkpointIndex = checkpoints.IndexOf(e.checkpoint);

            if (DataAchievement.easyObbyCheckpoint >= checkpointIndex) return;

            DataAchievement.SetEasyObbyCheckpoint(checkpointIndex);

            checkpoints[checkpointIndex + 1].ActiveArrow(true);
            view.GetComponent<ObbyCanvas>().InitProgress(checkpointIndex + 1, checkpoints.Count);
            curCheckpoint = e.checkpoint;
            curCheckpoint.PlayFX();

            Easypapa.AdHelper.ShowInterstitial("checkpoint");
        }
    }
}
