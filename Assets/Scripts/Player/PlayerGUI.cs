using Sirenix.OdinInspector;
using UnityEngine;
using Hichu;

namespace Game
{
    public class PlayerGUI : MonoBehaviour
    {
        [Title("CONTROL")]
        [SerializeField] private VariableJoystick _joystick;
        [SerializeField] private UIPointerDrag _look;
        [SerializeField] private UIPointerClick _jumpBtn;
        [SerializeField] private UIPointerClick _actionBtn;
        [SerializeField] private UIPointerClick _interactiveBtn;

        public VariableJoystick joystick { get { return _joystick; } }
        public UIPointerDrag look { get { return _look; } }
        public UIPointerClick jumpButton { get { return _jumpBtn; } }
        public UIPointerClick actionBtn { get { return _actionBtn; } }
        public UIPointerClick interactiveBtn { get { return _interactiveBtn; } }

        private FieldOfView _fov;
        private void Start()
        {
            _fov = Player.Instance.character.fov;
        }
    }
}