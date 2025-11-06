using Hichu;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class BrainrotEvoEgg : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] AssetReferenceGameObject eggView;
        [SerializeField] TextMeshPro priceText;
        [SerializeField] int map;
        [SerializeField] int id;
        private View view;

        private void Start()
        {
            priceText.text = FactoryBrainrotEvo.mapDatas[map].eggData[id].price.ToString();
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
                view = await ViewHelper.PushAsync(eggView);

                if(view != null) view.GetComponent<OpenEgg>().Init(id);
            }
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
