using Codice.Client.BaseCommands;
using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class ItemBox : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] ItemConfig itemData;
        [SerializeField] AssetReferenceGameObject itemView;
        private View view;

        private void Start()
        {
            if (DataItem.IsUnlocked(itemData.itemName))
            {
                this.gameObject.SetActive(false);
            }

            StaticBus<Event_Buy_Item>.Subscribe(EventBuy);
        }
        private void OnDestroy()
        {
            StaticBus<Event_Buy_Item>.Unsubscribe(EventBuy);
        }

        public void EventBuy(Event_Buy_Item e)
        {
            if (DataItem.IsUnlocked(itemData.itemName))
            {
                this.gameObject.SetActive(false);
            }
        }
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        async void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            if (character.GetComponentInParent<Player>())
            {
                view = await ViewHelper.PushAsync(itemView);

                if (view != null) view.GetComponent<ItemUnlock>().InitItem(itemData);
            }
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
