using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hichu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ItemUnlock : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI itemName;
        [SerializeField] Image itemIcon;
        [SerializeField] Button adsUnlock;
        [SerializeField] Button cashUnlock;
        [SerializeField] TextMeshProUGUI price;
        [SerializeField] Button nothanks;

        private ItemConfig curData;
        private View view;
        private void Start()
        {
            cashUnlock.onClick.AddListener(CashBuy);
            adsUnlock.onClick.AddListener(AdsBuy);

            view = GetComponent<View>();

        }

        public async void InitItem(ItemConfig data)
        {
            curData = data;

            itemName.text = curData.itemName;
            itemIcon.sprite = curData.sprite;
            itemIcon.SetNativeSize();

            adsUnlock.gameObject.SetActive(false);
            cashUnlock.gameObject.SetActive(false);

            switch (curData.currency)
            {
                case Currency.Cash:
                    price.text = $"{curData.price}";
                    cashUnlock.gameObject.SetActive(true);
                    break;
                case Currency.Ads:
                    adsUnlock.gameObject.SetActive(true);
                    break;
            }

            await UniTask.WaitForSeconds(2f);

            nothanks.gameObject.SetActive(true);
            nothanks.GetComponent<RectTransform>().DOScale(1, 0.35f).SetEase(Ease.OutBack).ChangeStartValue(Vector2.zero);
        }

        private void CashBuy()
        {
            if(DataPlayer.cash < curData.price)
            {
                UINotificationText.Push("NOT ENOUGH MONEY");
            }
            else
            {
                DataPlayer.instance.AddCash(-curData.price);

                Player.Instance.character.itemManager.UnlockItem(curData);
                DataItem.SetCurrentItem(curData.itemName);

                StaticBus<Event_Buy_Item>.Post(null);

                Easypapa.EasypapaAdSdk.LogEvent($"item_unlock_{curData.itemName}", "item", curData.itemName);

                view.Close();
            }
        }

        private void AdsBuy()
        {
            // reward
            Easypapa.AdHelper.ShowRewarded(
                "brainrot_Unlock_Item",
                rewarded =>
                {
                    if (rewarded)
                    {
                        Player.Instance.character.itemManager.UnlockItem(curData);
                        DataItem.SetCurrentItem(curData.itemName);
                        StaticBus<Event_Buy_Item>.Post(null);
                        Easypapa.EasypapaAdSdk.LogEvent($"item_unlock_{curData.itemName}", "item", curData.itemName);
                        view.Close();
                    }
                    else
                    {
                        Debug.Log("Rewarded NOT completed.");
                    }
                });
        }
    }
}
