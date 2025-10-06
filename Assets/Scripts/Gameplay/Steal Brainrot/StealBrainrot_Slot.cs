using Hichu;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Game
{
    public class StealBrainrot_Slot : MonoBehaviour
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

        public void SetBrainrot(StealBrainrot_Brainrot br)
        {
            brainrot = br;
            isEmpty = false;

            //StartGenerating();
        }

        [Button]
        public void StartGenerating()
        {
            if (!isGenerating)
            {
                txtTotalEarn.gameObject.SetActive(true);
                isGenerating = true;
                StartCoroutine(GenerateLoop());
            }
        }

        public void StopGenerating()
        {
            txtTotalEarn.gameObject.SetActive(false);
            isGenerating = false;
            StopCoroutine(GenerateLoop());
        }

        private IEnumerator GenerateLoop()
        {
            while (isGenerating)
            {
                yield return new WaitForSeconds(1f);
                totalEarn += brainrot.earn;
                txtTotalEarn.text = "Collect\n" + "$" + StealBrainrot_Manager.FormatMoney(totalEarn);
            }
        }

        public void CollectCash()
        {
            StaticBus<Event_Cash_Update>.Post(new Event_Cash_Update(totalEarn));
            totalEarn = 0;
            txtTotalEarn.text = "Collect\n" + "$" + StealBrainrot_Manager.FormatMoney(totalEarn);
        }
    }
}
