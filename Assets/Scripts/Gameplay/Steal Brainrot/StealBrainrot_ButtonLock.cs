using Hichu;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class StealBrainrot_ButtonLock : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] private StealBrainrot_Base _base;
        [SerializeField] private TextMeshPro lockTxt;
        [SerializeField] private float lockTime = 60f;

        [SerializeField] AssetReferenceGameObject lockView;

        public bool isLocked;
        private Coroutine lockRoutine;

        async void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (character.GetComponent<StealBrainrot_Player>())
            {
                if (character.GetComponent<StealBrainrot_Player>().baseSlot != _base) return;

                View view = await ViewHelper.PushAsync(lockView);
                if (!isLocked)
                {
                    Lock(lockTime);
                }

            }

            if (character.GetComponentInParent<StealBrainrot_AI>())
            {
                if (character.GetComponentInParent<StealBrainrot_AI>().curBase != _base) return;
                if (!isLocked)
                    Lock(lockTime);
            }
        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character) { }
        void ICharacterCollidable.OnTriggerExit(CharacterControl character) { }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character) { }

        public void SetLock(bool locked)
        {
            isLocked = locked;

            if (lockTxt != null)
            {
                lockTxt.gameObject.SetActive(true);
                lockTxt.color = locked ? Color.green : Color.red;
                if (!locked)
                    lockTxt.text = "Lock";
            }
        }

        public void StartLockCountdown(float duration)
        {
            if (_base != null)
                _base.SetLock(true);

            if (lockRoutine != null)
                StopCoroutine(lockRoutine);

            isLocked = true;

            if (lockTxt != null)
            {
                lockTxt.gameObject.SetActive(true);
                lockTxt.color = Color.green;
            }

            lockRoutine = StartCoroutine(Co_LockCountdown(duration));
        }

        public void ForceUnlockImmediate()
        {
            if (lockRoutine != null)
            {
                StopCoroutine(lockRoutine);
                lockRoutine = null;
            }

            UnlockInternal();
        }

        private void Lock(float duration)
        {

            if (isLocked) return;

            isLocked = true;

            if (_base != null)
                _base.SetLock(true);

            if (lockTxt != null)
            {
                lockTxt.gameObject.SetActive(true);
                lockTxt.color = Color.green;
            }

            if (lockRoutine != null)
                StopCoroutine(lockRoutine);

            lockRoutine = StartCoroutine(Co_LockCountdown(duration));
        }

        private IEnumerator Co_LockCountdown(float duration)
        {
            float timer = Mathf.Max(0f, duration);

            while (timer > 0f)
            {
                timer -= Time.deltaTime;

                if (lockTxt != null)
                    lockTxt.text = Mathf.CeilToInt(timer).ToString();

                yield return null;
            }

            lockRoutine = null;
            UnlockInternal();
        }

        private void UnlockInternal()
        {
            isLocked = false;

            if (_base != null)
                _base.SetLock(false);

            if (lockTxt != null)
            {
                lockTxt.text = "Lock";
                lockTxt.color = Color.red;
            }
        }
    }
}
