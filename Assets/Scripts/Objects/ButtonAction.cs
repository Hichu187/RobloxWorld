using UnityEngine;
using Hichu;
using DG.Tweening;
using Kcc;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ButtonAction : MonoBehaviour, ICharacterCollidable
    {
        [Header("Refs")]
        [SerializeField] private Animator anim;

        [SerializeField] private AnimationSequence sequence;
        [SerializeField] private AnimationSequence sequenceBack;

        [Header("Settings")]
        [SerializeField] private bool isMoveBack = false;

        private bool isMoved = false;

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character) { }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character) { MovingDown(); }
        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { MovingUp(); }
        void ICharacterCollidable.OnCollisionExit(CharacterControl  character) { }

        private void Awake()
        {
            EnsureAnimator();
        }


        private void Reset()
        {
            EnsureAnimator();
        }

        private void MovingDown()
        {

            if (anim != null) anim.SetBool("Up", false);

            if (isMoved) return;

            if (sequenceBack != null && sequenceBack.sequence != null && sequenceBack.sequence.IsPlaying())
                sequenceBack.sequence.OnComplete(() =>
                {
                    isMoved = true;
                });

            if (sequence != null && sequence.sequence != null && !sequence.sequence.IsPlaying())
                sequence.sequence.Play();
        }

        private void MovingUp()
        {
            if (anim != null) anim.SetBool("Up", true);

            if (!isMoveBack) return;
            if (!isMoved) return;

            if (sequence != null && sequence.sequence != null && sequence.sequence.IsPlaying())
                sequence.sequence.OnComplete(() =>
                {
                    isMoved = false;
                });

            if (sequenceBack != null && sequenceBack.sequence != null && !sequenceBack.sequence.IsPlaying())
                sequenceBack.sequence.PlayForward();
        }
        private void EnsureAnimator()
        {
            if (anim == null)
                anim = GetComponentInChildren<Animator>();
        }
    }
}
