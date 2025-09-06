using Sirenix.OdinInspector;
using UnityEngine;
using Hichu;

namespace Kcc.Base
{
    public class PlayerGUI : MonoBehaviour
    {
        [Title("CONTROL")]
        [SerializeField] private VariableJoystick _joystick;
        [SerializeField] private UIPointerDrag _look;
        [SerializeField] private UIPointerClick _jumpBtn;
        [SerializeField] private UIPointerClick _interactiveBtn;

        public VariableJoystick joystick { get { return _joystick; } }
        public UIPointerDrag look { get { return _look; } }
        public UIPointerClick jumpButton { get { return _jumpBtn; } }
        public UIPointerClick interactiveBtn { get { return _interactiveBtn; } }

    }
}
