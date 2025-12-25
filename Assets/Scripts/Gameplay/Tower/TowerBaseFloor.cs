using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_DropFloor : IEvent
    {

    }

    public class TowerBaseFloor :MonoBehaviour, ICharacterCollidable
    {
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (character.GetComponentInParent<Player>())
            {
                TowerGameplay gameplay = GameObject.FindAnyObjectByType<TowerGameplay>();

                if (gameplay.checkpoints.IndexOf(gameplay.curCheckpoint) < 2) return;

                StaticBus<Event_DropFloor>.Post(null);
            }
        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {

        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
