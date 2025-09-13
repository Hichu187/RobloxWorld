using Hichu;
using Kcc.Base;
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

        void ICharacterCollidable.OnCollisionEnter(Character character)
        {

        }
        void ICharacterCollidable.OnCollisionExit(Character character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(Character character)
        {
            if (character.GetComponentInParent<Player>())
            {
                StaticBus<Event_Checkpoint>.Post(new Event_Checkpoint(this, character));
            }
        }

        void ICharacterCollidable.OnTriggerExit(Character character)
        {

        }
    }
}
