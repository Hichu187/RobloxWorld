using Hichu;
using TMPro;
using UnityEngine;
using System.Collections;

namespace Game
{
    public class StealBrainrot_ButtonLock : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] private StealBrainrot_Base _base;
        [SerializeField] private TextMeshPro lockTxt;
        [SerializeField] private float lockTime = 60f;

        private bool isLocked = false;
        private Coroutine lockRoutine;

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (!isLocked)
                Lock();
        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character) { }
        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character) { }

        // Lock Logic
        public void Lock()
        {
            if (isLocked) return;

            isLocked = true;
            _base.SetLock(true);
            if (lockRoutine != null) StopCoroutine(lockRoutine);
            lockRoutine = StartCoroutine(Co_LockCountdown());
        }

        private IEnumerator Co_LockCountdown()
        {
            float timer = lockTime;

            if (lockTxt != null)
                lockTxt.gameObject.SetActive(true);

            while (timer > 0f)
            {
                timer -= Time.deltaTime;

                if (lockTxt != null)
                    lockTxt.text = Mathf.CeilToInt(timer).ToString();

                yield return null;
            }

            Unlock();
        }

        private void Unlock()
        {
            isLocked = false;
            _base.SetLock(false);

            if (lockTxt != null)
                lockTxt.gameObject.SetActive(false);

            lockRoutine = null;
        }
    }
}
