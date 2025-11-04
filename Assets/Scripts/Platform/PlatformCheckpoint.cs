using Cysharp.Threading.Tasks;
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

        public async void PlayFX()
        {
            if (_vfx != null) _vfx.gameObject.SetActive(true);

            if (_sfx != null) AudioManager.Play(_sfx);

            await UniTask.WaitForSeconds(2f);

            if (_vfx != null) _vfx.gameObject.SetActive(false);
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
