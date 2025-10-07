using Hichu;
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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Tap();
            }

            transform.LookAt(Camera.main.transform);

        }

        private void LateUpdate()
        {
            if (!brainrot.isBought)
            {
                if (Vector3.Distance(brainrot.transform.GetChild(0).position, Player.Instance.character.transform.position + Vector3.up*3) <
                    5f)
                {
                    btnColect.SetActive(true);
                    if (brainrot.outline) brainrot.outline.SetFloat("_OutlineWidth", 0.1f);
                }
                else
                {
                    btnColect.SetActive(false);
                    if (brainrot.outline) brainrot.outline.SetFloat("_OutlineWidth", 0f);
                }
            }
            else
            {
                btnColect.SetActive(false);

                if (brainrot.indBase != 0)
                {
                    if (!isSteal)
                    {

                    }

                }
                else
                {
                    if (Vector3.Distance(brainrot.transform.GetChild(0).position, Player.Instance.character.transform.position + Vector3.up * 3) < 5f && brainrot.isMovingHome == false)
                    {
                        btnSell.SetActive(true);
                        btnSell.transform.GetChild(0).GetComponent<TextMeshPro>().text = $"Sell  {(int)(brainrot.cost / 2)}$";
                    }
                    else
                    {
                        btnSell.SetActive(false);
                    }

                }
            }
        }

        void Tap()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, layerMaskPet))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject == btnColect.gameObject)
                {
                    Buy(0, this.brainrot);
                }
                else
                {
                    if (hitObject == btnSteal.gameObject)
                    {
                        Steal();
                    }

                    if (hitObject == btnSell.gameObject)
                    {
                        Sell();
                    }
                }
            }
        }

        private void Sell()
        {
            LDebug.Log<StealBrainrot_BrainrotInfor>($"Sell");

            DataStealBrainrot.instance.CashUpdate((int)(brainrot.cost / 2));
            DataStealBrainrot.RemoveBaseSlot(brainrot.targetSlot.slotId);

            brainrot.SellBrainrot();


        }
        public void Steal()
        {
            LDebug.Log<StealBrainrot_BrainrotInfor>($"Steal");
        }

        public void Buy(int playerId, StealBrainrot_Brainrot brainrot)
        {
            var p = FindAnyObjectByType<StealBrainrot_Player>();
            if (p == null || p.baseSlot == null)
            {
                Debug.LogWarning("Không tìm thấy player hoặc baseSlot.");
                return;
            }

            var slot = p.baseSlot.GetFirstEmptySlot();
            if (slot == null)
            {
                UINotificationText.Push("FULL SLOT");
                return;
            }

            if (playerId == 0)
            {
                if (brainrot.bConfig.reward)
                {
                    LDebug.Log<StealBrainrot_BrainrotInfor>("Ads Buy");
                    BuyBrainrot(p, slot, brainrot);
                }
                else
                {
                    if (DataStealBrainrot.cash >= brainrot.cost)
                    {
                        DataStealBrainrot.instance.CashUpdate(-brainrot.cost);
                        LDebug.Log<StealBrainrot_BrainrotInfor>("Cash Buy");
                        BuyBrainrot(p, slot, brainrot);
                    }
                    else
                    {
                        UINotificationText.Push("NOT ENOUGH CASH");
                    }
                }
            }
            else
            {
                BuyBrainrot(p, slot, brainrot);
            }
        }

        private void BuyBrainrot(StealBrainrot_Player p, StealBrainrot_Slot slot, StealBrainrot_Brainrot brainrot)
        {
            if (slot == null) return;

            var tSlot = slot.stayPosition != null ? slot.stayPosition : slot.transform;

            brainrot.isBought = true;
            brainrot.targetSlot = slot;
            brainrot.target = tSlot;
            brainrot.canMove = true;
            brainrot.isMovingHome = true;

            int slotIndex = p.baseSlot.slots.IndexOf(slot);
            if (slotIndex >= 0)
                DataStealBrainrot.AddOrUpdateBaseSlot(slotIndex, brainrot.bConfig.ID);

            brainrot.BuyBrainrot();
        }


    }
}
