using UnityEngine;
namespace Game
{
    public class PlatformKill : MonoBehaviour, ICharacterCollidable
    {
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            character.GetComponent<Character>().Kill();
        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            character.GetComponent<Character>().Kill();
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
