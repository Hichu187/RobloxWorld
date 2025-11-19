using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CharacterCamera : MonoBehaviour
    {
        public Transform defaultTarget;

        public Camera Camera;
        public Vector2 FollowPointFraming = new Vector2(0f, 0f);
        public float FollowingSharpness = 10000f;

        public float DefaultDistance = 6f;
        public float MinDistance = 0f;
        public float MaxDistance = 10f;
        public float DistanceMovementSpeed = 5f;
        public float DistanceMovementSharpness = 10f;

        public bool InvertX = false;
        public bool InvertY = false;
        [Range(-90f, 90f)]
        public float DefaultVerticalAngle = 20f;
        [Range(-90f, 90f)]
        public float MinVerticalAngle = -90f;
        [Range(-90f, 90f)]
        public float MaxVerticalAngle = 90f;
        public float RotationSpeed = 1f;
        public float RotationSharpness = 10000f;
        public bool RotateWithPhysicsMover = false;

        public float ObstructionCheckRadius = 0.2f;
        public LayerMask ObstructionLayers = -1;
        public float ObstructionSharpness = 10000f;
        public List<Collider> IgnoredColliders = new List<Collider>();

        public Transform Transform { get; private set; }
        public Transform FollowTransform { get; private set; }

        public Vector3 PlanarDirection { get; set; }
        public float TargetDistance { get; set; }

        private bool _distanceIsObstructed;
        private float _currentDistance;
        private float _targetVerticalAngle;
        private int _obstructionCount;
        private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
        private Vector3 _currentFollowPosition;

        private float _savedMinVerticalAngle;
        private float _savedMaxVerticalAngle;
        private float _savedDefaultVerticalAngle;

        private const int MaxObstructions = 32;

        void OnValidate()
        {
            DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
        }

        void Awake()
        {
            Transform = transform;

            _savedMinVerticalAngle = MinVerticalAngle;
            _savedMaxVerticalAngle = MaxVerticalAngle;
            _savedDefaultVerticalAngle = DefaultVerticalAngle;

            _currentDistance = DefaultDistance;
            TargetDistance = _currentDistance;

            _targetVerticalAngle = DefaultVerticalAngle;
            PlanarDirection = Vector3.forward;

            if (defaultTarget != null && FollowTransform == null)
            {
                SetFollowTransform(defaultTarget, false);
            }
        }

        public void SetFollowTransform(Transform t, bool forceZeroVertical = false)
        {
            FollowTransform = t;

            if (FollowTransform != null)
            {
                PlanarDirection = FollowTransform.forward;
                _currentFollowPosition = FollowTransform.position;
            }

            if (forceZeroVertical)
            {
                MinVerticalAngle = 30f;
                MaxVerticalAngle = 30f;
            }
            else
            {
                MinVerticalAngle = _savedMinVerticalAngle;
                MaxVerticalAngle = _savedMaxVerticalAngle;
                DefaultVerticalAngle = _savedDefaultVerticalAngle;

                _targetVerticalAngle = DefaultVerticalAngle;
            }
        }

        public void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput)
        {
            if (!FollowTransform)
                return;

            if (InvertX)
            {
                rotationInput.x *= -1f;
            }
            if (InvertY)
            {
                rotationInput.y *= -1f;
            }

            Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(FollowTransform.up, Vector3.Cross(PlanarDirection, FollowTransform.up));
            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);

            _targetVerticalAngle -= rotationInput.y * RotationSpeed;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);

            Quaternion targetRotation = Quaternion.Slerp(
                Transform.rotation,
                planarRot * verticalRot,
                1f - Mathf.Exp(-RotationSharpness * deltaTime));

            Transform.rotation = targetRotation;

            if (_distanceIsObstructed && Mathf.Abs(zoomInput) > 0f)
            {
                TargetDistance = _currentDistance;
            }
            TargetDistance += zoomInput * DistanceMovementSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);

            _currentFollowPosition = Vector3.Lerp(
                _currentFollowPosition,
                FollowTransform.position,
                1f - Mathf.Exp(-FollowingSharpness * deltaTime));

            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;

            _obstructionCount = Physics.SphereCastNonAlloc(
                _currentFollowPosition,
                ObstructionCheckRadius,
                -Transform.forward,
                _obstructions,
                TargetDistance,
                ObstructionLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < _obstructionCount; i++)
            {
                bool isIgnored = false;

                for (int j = 0; j < IgnoredColliders.Count; j++)
                {
                    if (IgnoredColliders[j] == _obstructions[i].collider)
                    {
                        isIgnored = true;
                        break;
                    }
                }

                if (!isIgnored &&
                    _obstructions[i].distance < closestHit.distance &&
                    _obstructions[i].distance > 0)
                {
                    closestHit = _obstructions[i];
                }
            }

            if (closestHit.distance < Mathf.Infinity)
            {
                _distanceIsObstructed = true;
                _currentDistance = Mathf.Lerp(
                    _currentDistance,
                    closestHit.distance,
                    1f - Mathf.Exp(-ObstructionSharpness * deltaTime));
            }
            else
            {
                _distanceIsObstructed = false;
                _currentDistance = Mathf.Lerp(
                    _currentDistance,
                    TargetDistance,
                    1f - Mathf.Exp(-DistanceMovementSharpness * deltaTime));
            }

            Vector3 targetPosition = _currentFollowPosition - targetRotation * Vector3.forward * _currentDistance;

            targetPosition += Transform.right * FollowPointFraming.x;
            targetPosition += Transform.up * FollowPointFraming.y;

            Transform.position = targetPosition;
        }
    }
}
