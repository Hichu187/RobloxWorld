using UnityEngine;

namespace Game
{
    public class CharacterAnimator : MonoBehaviour
    {
        public Animator _animator;
        public Animator animator { get { if (_animator == null) _animator = GetComponentInChildren<Animator>(); return _animator; } }

        public void SetJumping(bool isJumping)
        {
            if (animator == null)
                return;
            animator.SetBool(HashDictionary.jumping, isJumping);
        }

        public void SetClimbing(bool isClimbing)
        {
            if (animator == null)
                return;

            animator.SetBool(HashDictionary.climbing, isClimbing);
        }

        public void SetClimbY(float y)
        {
            if (animator == null)
                return;

            animator.SetFloat(HashDictionary.climbY, y * 0.5f);
        }

        public void SetVelocityZ(float z)
        {
            if (animator == null)
                return;

            animator.SetFloat(HashDictionary.velocityZ, z);
        }

        public float GetVelocityZ()
        {
            if (animator == null)
                return 0f;

            return animator.GetFloat(HashDictionary.velocityZ);
        }

        public void SetMoveMotion(float motion)
        {
            if (animator == null)
                return;

            animator.SetFloat("MoveMotion", motion);
        }

        public void PlayIdleReset()
        {
            animator.SetBool("Dead", false);
        }

        public void PlayBlock()
        {
            animator.SetTrigger("Block");
        }

        public void PlayIdle(float fadeDuration = 0.3f)
        {
            if (animator == null) return;

            animator.CrossFade("Ground", fadeDuration);
        }

        public void PlayWin()
        {
            if (animator == null)
                return;
            animator.SetLayerWeight(1, 0);
            animator.SetFloat("randomValue", Random.Range(0, 5));
            animator.Play(HashDictionary.win);
        }
        public void PlayDie()
        {
            if (animator == null)
                return;
            animator.SetFloat("randomValue", Random.Range(0, 5));
            animator.SetBool(HashDictionary.dead, true);
        }

        public void PlayRevive()
        {
            if (animator == null)
                return;
            animator.SetBool("Dead", false);
        }

        public void PlayKnockback()
        {

        }
        public void PlayTug()
        {

        }
    }
}
