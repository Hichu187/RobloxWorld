using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Slot : MonoBehaviour, ICharacterCollidable
    {
        public StealBrainrot_Brainrot brainrot;
        public TMP_Text txtTotalEarn;
        public Transform stayPosition;
        public GameObject buttonCollect;

        public int baseId;
        public int slotId;
        public bool isEmpty = true;
        private bool isGenerating = false;
        public int totalEarn = 0;

        private Coroutine _generateRoutine;

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (!isEmpty) return;
            if (character.GetComponent<StealBrainrot_Player>().baseSlot.baseID != baseId) return;
            if (!character.GetComponent<StealBrainrot_Player>()) return;
            if (!character.GetComponent<StealBrainrot_Player>().isStealing) return;

            character.GetComponent<StealBrainrot_Player>().StealingDone(this);

        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }

        public void SetBrainrot(StealBrainrot_Brainrot br)
        {
            brainrot = br;
            isEmpty = false;
        }

        [Button]
        public void StartGenerating()
        {
            if (isGenerating) return;
            isGenerating = true;
            if (txtTotalEarn) txtTotalEarn.gameObject.SetActive(true);
            _generateRoutine = StartCoroutine(GenerateLoop());
        }

        public void StopGenerating()
        {
            if (!isGenerating) return;
            isGenerating = false;
            if (_generateRoutine != null)
            {
                StopCoroutine(_generateRoutine);
                _generateRoutine = null;
            }
            if (txtTotalEarn) txtTotalEarn.gameObject.SetActive(false);
        }

        private IEnumerator GenerateLoop()
        {
            var wait = new WaitForSeconds(1f);
            while (isGenerating)
            {
                if (brainrot == null)
                {
                    isGenerating = false;
                    _generateRoutine = null;
                    yield break;
                }

                yield return wait;

                if (!isGenerating) yield break;

                totalEarn += brainrot.earn;
                if (txtTotalEarn)
                    txtTotalEarn.text = "Collect\n$" + StealBrainrot_Manager.FormatMoney(totalEarn);
            }
            _generateRoutine = null;
        }

        public void CollectCash()
        {
            if (totalEarn > 0)
                DataStealBrainrot.instance.CashUpdate(totalEarn);

            totalEarn = 0;
            if (txtTotalEarn)
                txtTotalEarn.text = "Collect\n$" + StealBrainrot_Manager.FormatMoney(totalEarn);
        }

        public void ResetBrainrot()
        {
            StopGenerating();
            isEmpty = true;
            brainrot = null;
            totalEarn = 0;
            if (txtTotalEarn)
                txtTotalEarn.text = "Collect\n$0";
        }

        private void OnDisable()
        {
            StopGenerating();
        }
    }
}
