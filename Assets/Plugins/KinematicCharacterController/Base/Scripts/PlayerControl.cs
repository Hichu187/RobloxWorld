using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Kcc.Base
{
    public class PlayerControl : MonoBehaviour
    {
        [Title("Input")]
        public bool canMove = true;
        [SerializeField] private bool _useMobileControl = false;
        [SerializeField] private float _lookSensitive = 20f;

        private CharacterController _characterControl;
        private CharacterCamera _characterCamera;
        private PlayerGUI _gui;
        private FieldOfView _fov;
        public Transform _cameraFollowTarget;

        private float _inputLookX;
        private float _inputLookY;
        private bool _inputJumpDown = false;
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private bool _isDraggingMouse = false;
        private Vector3 _lastMousePosition;

        public bool useMobileControl { get { return _useMobileControl; } }
        private void Awake()
        {
            _characterControl = Player.Instance.cControl;
            _characterCamera = Player.Instance.cCamera;
            _gui = Player.Instance.gui;
            _fov = Player.Instance.fov;
        }

        private void Start()
        {
#if !UNITY_EDITOR
_useMobileControl = true;
#endif

            // Tell camera to follow transform
            _characterCamera.SetFollowTransform(_cameraFollowTarget ? _cameraFollowTarget : _characterControl.TransformCached);

            // Ignore the character's collider(s) for camera obstruction checks
            _characterCamera.IgnoredColliders.Clear();
            _characterCamera.IgnoredColliders.AddRange(_characterControl.GetComponentsInChildren<Collider>());

            UIPointerDrag look = _gui.look;
            look.eventDrag += UILook_EventDrag;
            look.eventDragEnd += UILook_EventDragEnd;

            UIPointerClick jump = _gui.jumpButton;
            jump.eventDown += UIJump_EventDown;
            jump.eventUp += UIJump_EventUp;

            UIPointerClick action = _gui.actionBtn;
            action.eventDown += UIAction_EventDown;
            action.eventUp += UIAction_EventUp;
        }

        private void Update()
        {
            if (!_useMobileControl)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _isDraggingMouse = true;
                    _lastMousePosition = Input.mousePosition;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _isDraggingMouse = false;
                }
            }

            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (_characterCamera.RotateWithPhysicsMover && _characterControl.Motor.AttachedRigidbody != null)
            {
                _characterCamera.PlanarDirection = _characterControl.Motor.AttachedRigidbody.GetComponent<KccMover>().RotationDeltaFromInterpolation * _characterCamera.PlanarDirection;
                _characterCamera.PlanarDirection = Vector3.ProjectOnPlane(_characterCamera.PlanarDirection, _characterControl.Motor.CharacterUp).normalized;
            }

            HandleCameraInput();
        }

        private void UILook_EventDrag(PointerEventData e)
        {
            _inputLookX = e.delta.x * _lookSensitive / Screen.dpi;
            _inputLookY = e.delta.y * (_lookSensitive / 2) / Screen.dpi;
        }

        private void UILook_EventDragEnd(PointerEventData e)
        {
            _inputLookX = 0f;
            _inputLookY = 0f;
        }
        private void UIAction_EventDown()
        {
            if (_fov.visibleTargets.Count == 0 || _fov.visibleTargets == null) return;

            LDebug.Log<FieldOfView>(_fov.visibleTargets.Count);
        }

        private void UIAction_EventUp()
        {

        }

        private void UIJump_EventDown()
        {
            _inputJumpDown = true;

        }
        private void UIJump_EventUp()
        {
            _inputJumpDown = false;
        }


        private void HandleCameraInput()
        {
            Vector3 lookInputVector = Vector3.zero;

            if (_useMobileControl)
            {
                lookInputVector = new Vector3(_inputLookX, _inputLookY, 0f);
            }
            else if (_isDraggingMouse)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 delta = currentMousePosition - _lastMousePosition;

                float deltaX = delta.x * _lookSensitive * 0.02f;
                float deltaY = delta.y * _lookSensitive * 0.02f;

                lookInputVector = new Vector3(deltaX, deltaY, 0f);
                _lastMousePosition = currentMousePosition;
            }

            float scrollInput = -Input.GetAxis(MouseScrollInput);
            _characterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);
        }

        private void HandleCharacterInput()
        {
            CharacterInput characterInputs = new CharacterInput();

            float rawH, rawV;
            if (_useMobileControl)
            {
                rawV = _gui.joystick.Vertical;
                rawH = _gui.joystick.Horizontal;
                characterInputs.Jump = _inputJumpDown;
            }
            else
            {
                rawV = Input.GetAxisRaw(VerticalInput);
                rawH = Input.GetAxisRaw(HorizontalInput);
                characterInputs.Jump = Input.GetKeyDown(KeyCode.Space);
                //characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
                //characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);
            }

            Vector3 inputMove = new Vector3(rawH, 0f, rawV);

            Transform camT = _characterCamera != null ? _characterCamera.transform : Camera.main.transform;
            Vector3 characterUp = _characterControl != null ? _characterControl.Motor.CharacterUp : Vector3.up;

            Vector3 cameraRelativeMove = AdjustInputByCameraView(camT, inputMove, characterUp);

            characterInputs.MoveVector = cameraRelativeMove;

            if (cameraRelativeMove.sqrMagnitude > 0.0001f)
                characterInputs.LookVector = cameraRelativeMove.normalized;
            else
                characterInputs.LookVector = Vector3.zero;

            _characterControl.SetInputs(ref characterInputs);
        }

        private Vector3 AdjustInputByCameraView(Transform cameraTransform, Vector3 inputMove, Vector3 characterUp)
        {
            if (inputMove.sqrMagnitude <= 0.0001f)
                return Vector3.zero;

            Vector3 camFwd = Vector3.ProjectOnPlane(cameraTransform.forward, characterUp);
            if (camFwd.sqrMagnitude < 1e-6f)
                camFwd = Vector3.ProjectOnPlane(cameraTransform.up, characterUp); 
            camFwd.Normalize();

            Vector3 camRight = Vector3.Cross(characterUp, camFwd).normalized;

            Vector3 move = camFwd * inputMove.z + camRight * inputMove.x;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            move = Vector3.ProjectOnPlane(move, characterUp);

            return move;
        }
    }
}
