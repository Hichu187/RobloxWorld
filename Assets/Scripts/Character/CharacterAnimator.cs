using UnityEngine;

namespace Game
{
    public class CharacterAnimator : MonoBehaviour
    {
        public Animator _animator;
        public Animator animator { get { if (_animator == null) _animator = GetComponentInChildren<Animator>(); return _animator; } }

        [SerializeField] private float groundProbeDistance = 2f;
        [SerializeField] private LayerMask groundMask = ~0; // mặc định mọi layer
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        private bool inAir = false;
        private Coroutine _airProbeRoutine;

        public void SetJumping(bool isJumping)
        {
            if (animator == null) return;

            animator.SetBool(HashDictionary.jumping, isJumping);

            if (isJumping)
            {
                BeginAirPauseCheck();
            }
            else
            {
                EndAirPauseCheck();
            }
        }

        public void SetClimbing(bool isClimbing)
        {
            if (animator == null) return;

            animator.SetBool(HashDictionary.climbing, isClimbing);

            if (isClimbing == false)
            {
                animator.speed = 1f;
            }

            if (isClimbing)
            {
                EndAirPauseCheck();
            }
        }

        public void SetClimbY(float y)
        {
            if (animator == null) return;
            animator.SetFloat(HashDictionary.climbY, Mathf.Clamp(y, -1f, 1f));
        }

        public void SetVelocityZ(float z)
        {
            if (animator == null) return;
            animator.SetFloat(HashDictionary.velocityZ, z);
        }

        public float GetVelocityZ()
        {
            if (animator == null) return 0f;
            return animator.GetFloat(HashDictionary.velocityZ);
        }

        public void SetMoveMotion(float motion)
        {
            if (animator == null) return;
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
            if (animator == null) return;
            animator.SetLayerWeight(1, 0);
            animator.SetFloat("randomValue", Random.Range(0, 5));
            animator.Play(HashDictionary.win);
        }

        public void PlayDie()
        {
            if (animator == null) return;
            animator.SetFloat("randomValue", Random.Range(0, 5));
            animator.SetBool(HashDictionary.dead, true);
        }

        public void PlayRevive()
        {
            if (animator == null) return;
            animator.SetBool("Dead", false);
        }

        public void PlayKnockback() { }
        public void PlayTug() { }

        // === Air pause control ===
        private void BeginAirPauseCheck()
        {
            if (animator == null) return;

            inAir = true;

            if (_airProbeRoutine != null) StopCoroutine(_airProbeRoutine);
            _airProbeRoutine = StartCoroutine(Co_AirProbe());
        }

        private void EndAirPauseCheck()
        {
            inAir = false;
            if (animator != null) animator.speed = 1f;

            if (_airProbeRoutine != null)
            {
                StopCoroutine(_airProbeRoutine);
                _airProbeRoutine = null;
            }
        }

        private System.Collections.IEnumerator Co_AirProbe()
        {
            var tr = transform;
            RaycastHit hit;

            yield return new WaitForSeconds(0.3f);
            animator.speed = 0.15f;
            while (inAir)
            {
                Vector3 origin = tr.position;
                Vector3 dir = Vector3.down;

                Debug.DrawRay(origin, dir * groundProbeDistance, Color.red);

                if (Physics.Raycast(origin, dir, out hit, groundProbeDistance, groundMask, triggerInteraction))
                {
                    if (animator != null) 
                    animator.speed = 1f;
                    Debug.Log("hit");
                    inAir = false;
                    break;
                }
                yield return null;
            }

            _airProbeRoutine = null;
        }
    }
}
