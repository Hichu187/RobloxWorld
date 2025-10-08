using Hichu;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_BrainrotInfor : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private GameObject btnColect;
        [SerializeField] private GameObject btnSteal;
        [SerializeField] private GameObject btnSell;

        [Header("Texts")]
        public TMP_Text txtName;
        public TMP_Text txtRank;
        public TMP_Text txtEarn;
        public TMP_Text txtCost;

        [Header("Raycast")]
        [SerializeField] private LayerMask layerMaskPet;

        [Header("Refs")]
        [SerializeField] private StealBrainrot_Brainrot brainrot;

        [Header("Config")]
        [Min(0.1f)]
        [SerializeField] private float interactDistance = 5f;

        [Header("State")]
        public bool isSteal = false;
        public GameObject buy;
        public GameObject ads;

        private Transform _playerT;
        private Transform _brainrotHead;
        private float _interactSqr;
        private StealBrainrot_Player player;

        private void Awake()
        {
            _interactSqr = interactDistance * interactDistance;
        }

        private void Start()
        {
            if (Player.Instance && Player.Instance.character)
            {
                _playerT = Player.Instance.character.transform;
                player = Player.Instance.character.GetComponent<StealBrainrot_Player>();
            }

            if (brainrot && brainrot.transform.childCount > 0)
                _brainrotHead = brainrot.transform.GetChild(0);
            else
                _brainrotHead = brainrot ? brainrot.transform : transform;
        }

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

            if (Camera.main)
                transform.LookAt(Camera.main.transform);
        }

        private void LateUpdate()
        {
            if (_playerT == null || _brainrotHead == null || brainrot == null)
            {
                SafeSetActive(btnColect, false);
                SafeSetActive(btnSteal, false);
                SafeSetActive(btnSell, false);
                return;
            }

            bool near = IsPlayerNear();

            if (!brainrot.isBought)
            {
                SafeSetActive(btnColect, near);
                if (brainrot.outline) brainrot.outline.SetFloat("_OutlineWidth", near ? 0.1f : 0f);
            }
            else
            {
                SafeSetActive(btnColect, false);

                if (brainrot.indBase != 0)
                {
                    bool canShowSteal =
                        near &&
                        !brainrot.isMovingHome &&
                        !isSteal &&
                        !(player != null && player.isStealing);

                    SafeSetActive(btnSteal, canShowSteal);
                    SafeSetActive(btnSell, false);
                }
                else
                {
                    bool canShowSell = near && !brainrot.isMovingHome;
                    SafeSetActive(btnSell, canShowSell);

                    if (canShowSell && btnSell != null)
                    {
                        var tmp = btnSell.transform.GetChild(0).GetComponent<TextMeshPro>();
                        if (tmp != null) tmp.text = $"Sell  {(int)(brainrot.cost / 2)}$";
                    }

                    SafeSetActive(btnSteal, false);
                }
            }
        }

        private bool IsPlayerNear()
        {
            Vector3 playerFocus = _playerT.position + Vector3.up * 3f;
            Vector3 delta = _brainrotHead.position - playerFocus;
            return delta.sqrMagnitude <= _interactSqr;
        }

        private static void SafeSetActive(GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }

        void Tap()
        {
            if (!Camera.main) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, layerMaskPet))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject == btnColect)
                {
                    Buy(0, this.brainrot);
                }
                else if (hitObject == btnSteal)
                {
                    Steal();
                }
                else if (hitObject == btnSell)
                {
                    Sell();
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
            player.StealingBrainrot(brainrot);
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
            brainrot.indBase = p.baseSlot.baseID;

            int slotIndex = p.baseSlot.slots.IndexOf(slot);
            if (slotIndex >= 0)
                DataStealBrainrot.AddOrUpdateBaseSlot(slotIndex, brainrot.bConfig.ID);

            brainrot.BuyBrainrot();
        }
    }
}
