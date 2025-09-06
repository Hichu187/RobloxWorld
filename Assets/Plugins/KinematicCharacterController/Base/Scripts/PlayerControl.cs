using Hichu;
using UnityEngine;

namespace Kcc.Base
{
    public class PlayerControl : MonoBehaviour
    {
        private CharacterController _characterControl;
        private CharacterCamera _characterCamera;
        public Transform _cameraFollowTarget;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private void Awake()
        {
            _characterControl = Player.Instance.cControl;
            _characterCamera = Player.Instance.cCamera;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            _characterCamera.SetFollowTransform(_cameraFollowTarget ? _cameraFollowTarget : _characterControl.TransformCached);

            // Ignore the character's collider(s) for camera obstruction checks
            _characterCamera.IgnoredColliders.Clear();
            _characterCamera.IgnoredColliders.AddRange(_characterControl.GetComponentsInChildren<Collider>());

            LDebug.Log<PlayerControl>($"{Player.Instance.Test}");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
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

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

            // Apply inputs to the camera
            _characterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                _characterCamera.TargetDistance = (_characterCamera.TargetDistance == 0f) ? _characterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            CharacterInput characterInputs = new CharacterInput();

            Vector3 inputMove = new Vector3(Input.GetAxisRaw(HorizontalInput), 0f, Input.GetAxisRaw(VerticalInput));

            // Build the CharacterInputs struct
            //characterInputs.MoveVector.z = Input.GetAxisRaw(VerticalInput);
            //characterInputs.MoveVector.x = Input.GetAxisRaw(HorizontalInput);
            //characterInputs.CameraRotation = _characterCamera.Transform.rotation;
            characterInputs.Jump = Input.GetKeyDown(KeyCode.Space);
            //characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
            //characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

            Transform cameraTransform = Camera.main.transform;

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.forward, _characterControl.Motor.CharacterUp).normalized;

            if (cameraPlanarDirection.sqrMagnitude == 0f)
                cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.up, _characterControl.Motor.CharacterUp).normalized;

            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _characterControl.Motor.CharacterUp);

            // Move and look inputs
            characterInputs.MoveVector = cameraPlanarRotation * inputMove;
            characterInputs.LookVector = characterInputs.MoveVector.normalized;

            // Apply inputs to character
            _characterControl.SetInputs(ref characterInputs);
        }

        private void AdjustInputByCameraView(Transform cameraTransform, Vector3 inputMove, Vector3 characterUp)
        {

        }
    }
}
