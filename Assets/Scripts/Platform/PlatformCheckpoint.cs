using Hichu;
using UnityEngine;

namespace Game
{
    public class PlatformCheckpoint : MonoBehaviour, ICharacterCollidable
    {
        private int _index;
        public void SetIndex(int index)
        {
            _index = index;
        }

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            if (character.GetComponentInParent<Player>())
            {
                StaticBus<Event_Checkpoint>.Post(new Event_Checkpoint(this, character));
            }
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
