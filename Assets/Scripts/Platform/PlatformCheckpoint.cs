using Hichu;
using Kcc.Base;
using UnityEngine;

namespace Game
{
    public class PlatformCheckpoint : MonoBehaviour, ICharacterCollidable
    {
        void ICharacterCollidable.OnCollisionEnter(Character character)
        {

        }
        void ICharacterCollidable.OnCollisionExit(Character character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(Character character)
        {
            StaticBus<Event_Checkpoint>.Post(new Event_Checkpoint(this, character));
        }

        void ICharacterCollidable.OnTriggerExit(Character character)
        {

        }
    }
}
