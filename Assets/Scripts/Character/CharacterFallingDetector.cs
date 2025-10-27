using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game
{
    public class CharacterFallingDetector : MonoBase
    {
        [Title("Config")]
        [SerializeField] private float _threshold = 10f;

        [Space]

        [SerializeField] private bool _raycastCheckGround = false;
        [SerializeField] private float _raycastCheckGroundDistance = 15f;

        private Character _character;

        private float _groundY;

        public float threshold { get { return _threshold; } set { _threshold = value; } }

        private void Awake()
        {
            _character = GetComponent<Character>();

            _groundY = _character.transformCached.position.y;

            _character.eventRevive += Character_EventRevive;

        }

        private void OnDestroy()
        {
            _character.eventRevive -= Character_EventRevive;
        }

        protected override void Tick()
        {
            if (_character.motor.GroundingStatus.IsStableOnGround || _character.cControl.StateMachine.CurrentState == CharacterControl.State.ClimbLadder)
            {
                _groundY = _character.transformCached.position.y;
            }
            else if (_groundY - _character.transformCached.position.y > _threshold)
            {
                if (_raycastCheckGround && Physics.Raycast(_character.transformCached.position, Vector3.down, _raycastCheckGroundDistance, 0))
                    _groundY = _character.transformCached.transform.position.y;
                else
                    _character.Kill();
            }
        }

        private void Character_EventRevive()
        {
            _groundY = _character.transformCached.position.y;
        }
    }
}
