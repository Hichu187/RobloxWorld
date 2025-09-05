using DG.Tweening;
using UnityEngine;

namespace Hichu
{
    public class AnimationSequenceStepInterval : AnimationSequenceStep
    {
        [SerializeField] private float _duration;

        public override string DisplayName { get { return "Interval"; } }

        public override void AddToSequence(AnimationSequence animationSequence)
        {
            animationSequence.sequence.AppendInterval(_duration);
        }
    }
}