using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class PlatformCheckpoint : MonoBehaviour, ICharacterCollidable
    {
        [Title("Config")]
        [SerializeField] private GameObject _vfx;
        [SerializeField] private AudioConfig _sfx;

        private int _index;
        public int index { get { return _index; } }
        public void SetIndex(int index)
        {
            _index = index;
        }

        public void PlayFX()
        {
            if (_vfx != null) _vfx.Create(transform.position, transform.rotation);

            if (_sfx != null) AudioManager.Play(_sfx);
        }

        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
            if (character.GetComponentInParent<Player>())
            {
                StaticBus<Event_Checkpoint>.Post(new Event_Checkpoint(this, character));
            }
        }
        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }
        void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
/*            if (character.GetComponentInParent<Player>())
            {
                StaticBus<Event_Checkpoint>.Post(new Event_Checkpoint(this, character));
            }*/
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
