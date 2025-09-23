using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class CharacterDie : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] private AudioConfig _sfx;
        [SerializeField] private GameObject _vfx;

        private Character _character;

        private GameObject _objVfx;
        private GameObject _objRagdoll;

        [SerializeField] private GameObject _objRoot;

        public GameObject ObjRoot
        {
            get => _objRoot;
            set => _objRoot = value;
        }

        private void Start()
        {
            _character = GetComponent<Character>();

            _character.eventDie += Character_EventDie;
            _character.eventRevive += Character_EventRevive;
        }

        private void Character_EventRevive()
        {
            if (_objRoot != null)
                _objRoot.SetActive(true);

            if (_objRagdoll != null)
            {
                Destroy(_objRagdoll);
                _objRagdoll = null;
            }

            if (_objVfx != null)
            {
                Destroy(_objVfx);
                _objVfx = null;
            }
        }

        private void Character_EventDie()
        {
            if (_objRoot != null)
                _objRoot.SetActive(false);

            if (_objRoot != null && _objRoot.GetComponentInChildren<CharacterRagdoll>())
            {
                _objRagdoll = _objRoot.Create(_objRoot.transform.parent);

                _objRagdoll.SetActive(true);

                _objRagdoll.GetComponentInChildren<CharacterRagdoll>().Explode();
            }

            if (_character.isPlayer)
            {
                StaticBus<Event_Player_Dead>.Post(null);

                LDebug.Log<CharacterDie>("Player Dead");
            }


            if (_objVfx != null)
            {
                _objVfx = _vfx.Create(_character.transformCached.position + Vector3.up, _character.transformCached.rotation);
            }

            if (_sfx != null)
            {
                AudioManager.Play(_sfx).transformCached.position = transform.position;
            }
        }
    }
}