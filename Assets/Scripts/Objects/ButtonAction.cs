using Kcc.Base;
using UnityEngine;
using Hichu;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;

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

        [Header("Sequences (auto-managed, do not remove)")]
        [SerializeField] private AnimationSequence sequence;
        [SerializeField] private AnimationSequence sequenceBack;

        [Header("Settings")]
        [SerializeField] private bool isMoveBack = false;

        private bool isMoved = false;

        void ICharacterCollidable.OnCollisionEnter(Character character) { }
        void ICharacterCollidable.OnTriggerEnter(Character character) { MovingDown(); }
        void ICharacterCollidable.OnTriggerExit(Character character) { MovingUp(); }
        void ICharacterCollidable.OnCollisionExit(Character character) { }

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureAnimator();
            EnsureExactlyTwoSequences(showDialog: true);
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
        }



        private void EnsureExactlyTwoSequences(bool showDialog)
        {
            List<AnimationSequence> all = GetComponents<AnimationSequence>().ToList();

            if (sequence == null || !all.Contains(sequence))
            {
                if (all.Count > 0) sequence = all[0];
            }

            while (all.Count < 2)
            {
                var added = gameObject.AddComponent<AnimationSequence>();
                all.Add(added);
                if (showDialog)
                    EditorUtility.DisplayDialog("Không thể xóa",
                        "ButtonAction yêu cầu chính xác 2 AnimationSequence. Thành phần vừa bị xóa đã được khôi phục.",
                        "OK");
            }

            if (all.Count > 2)
            {
                if (sequence == null || !all.Contains(sequence))
                    sequence = all[0];

                AnimationSequence keepOther = all.FirstOrDefault(x => x != sequence);

                foreach (var extra in all)
                {
                    if (extra == sequence || extra == keepOther) continue;
                    if (!Application.isPlaying) DestroyImmediate(extra);
                    else Destroy(extra);

                }

                if (showDialog)
                    EditorUtility.DisplayDialog("Limit 2 AnimationSequence",
                        "Cannot Remove Component",
                        "OK");

                all = GetComponents<AnimationSequence>().ToList();
            }

            if (sequence == null || !all.Contains(sequence))
                sequence = all[0];

            sequenceBack = all.First(x => x != sequence);

            LockComponentNotEditable(sequence);
            LockComponentNotEditable(sequenceBack);
            EditorUtility.SetDirty(sequence);
            EditorUtility.SetDirty(sequenceBack);
        }

        private void LockComponentNotEditable(Component c)
        {

        }
#endif
    }
}
