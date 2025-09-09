using UnityEngine;
using Kcc.Base;

namespace Game
{
    public class PlatformKill : MonoBehaviour, ICharacterCollidable
    {
        void ICharacterCollidable.OnCollisionEnter(Character character)
        {
            character.Kill();
        }

        void ICharacterCollidable.OnTriggerEnter(Character character)
        {
            character.Kill();
        }

        void ICharacterCollidable.OnTriggerExit(Character character)
        {
        }
    }
}
