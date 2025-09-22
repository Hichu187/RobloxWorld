using Hichu;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoGate : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] int levelOpen = 0;
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
;
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }

        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            if (!character.GetComponent<Character>().isPlayer) return;
            
            if(DataBrainrotEvo.level >= levelOpen)
            {
                LDebug.Log<BrainrotEvoGate>("Change Space");
                StaticBus<Event_BrainrotEvo_Change_Space>.Post(null);
                character.Motor.enabled = false;
            }
            else
            {
                LDebug.Log<BrainrotEvoGate>("Not Enough Level");
            }
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
