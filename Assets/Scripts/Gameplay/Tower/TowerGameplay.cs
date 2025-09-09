using DG.Tweening.Core.Easing;
using Kcc.Base;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class TowerGameplay : BaseGameplay
    {
        [SerializeField] Player player;
        protected override void SubscribeEvent()
        {
            base.SubscribeEvent();

        }

        protected override void UnsubscribeEvent()
        {
            base.UnsubscribeEvent();

        }

        [Button]
        public void RespawnCheckpoint()
        {
            player.character.Revive(_curCheckpoint.transform.position, _curCheckpoint.transform.rotation);
        }

    }
}
