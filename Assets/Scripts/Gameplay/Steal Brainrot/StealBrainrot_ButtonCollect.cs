using Hichu;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game
{
    public class StealBrainrot_ButtonCollect : MonoBehaviour, ICharacterCollidable
    {
        private bool isCollected = false;
        private StealBrainrot_Slot slot;
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (isCollected) return;

            slot = GetComponentInParent<StealBrainrot_Slot>();

            if (slot.brainrot != null && /*basePetSlot.petAi.indBase == 0 &&*/ slot.totalEarn > 0)
            {
                if (character.GetComponent<Character>().isPlayer)
                {
                    isCollected = true;
                    StaticBus<Event_Cash_Update>.Post(null);
                }
            }
        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {
            if (character.GetComponent<Character>().isPlayer)
            {
                isCollected = false;
            }
        }

    }
}
