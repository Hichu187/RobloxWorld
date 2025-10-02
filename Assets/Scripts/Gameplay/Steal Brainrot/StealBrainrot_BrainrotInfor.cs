using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_BrainrotInfor : MonoBehaviour
    {
        [SerializeField] private GameObject btnColect;
        [SerializeField] private GameObject btnSteal;
        [SerializeField] private GameObject btnSell;

        public TMP_Text txtName;
        public TMP_Text txtRank;
        public TMP_Text txtEarn;
        public TMP_Text txtCost;

        [SerializeField] private LayerMask layerMaskPet;
        [SerializeField] private StealBrainrot_Brainrot brainrot;
        public bool isSteal = false;

        public GameObject buy;
        public GameObject ads;

        [Button]
        public void SetupData(StealBrainrot_BrainrotConfig config)
        {
            txtName.text = config.brainrotName;
            txtRank.text = config.rank.ToString();
            txtEarn.text = $"$ {config.earningPerSecond}/s";

            int cost = config.costToBuy;
            txtCost.text = $"${StealBrainrot_Manager.FormatMoney(cost)}";
        }
    }
}
