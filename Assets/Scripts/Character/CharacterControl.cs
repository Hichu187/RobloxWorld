using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using Vertx.Debugging;
using System;
using Hichu;
using Kcc;

namespace Game
{
    public class CharacterControl : MonoCached, IKccMotor
    {
        public enum State
        {
            Ground,
            Air,
            ClimbLadder,
        }

        [Title("Reference")]
        [SerializeField] private KccMotor _motor;

        [Title("Config")]
        [SerializeField] private Collider[] _ignoredColliders;
        [SerializeField] private CharacterConfig _config;

        [Title("Animation")]
        [SerializeField, Range(1f, 30f)] private float _animVelSharpness = 12f;

        private CharacterAnimator _animator;
        private Vector3 _inputMove;
        private Vector3 _inputRotation;
        private float _groundStableTime = 0f;
        private float _jumpTimeSinceLast = 0f;
        private float _jumpTimeSinceRequest = Mathf.Infinity;
        private float _jumpSpeedMultiple = 1f;
        private int _jumpCount = 0;
        private Vector3 _additiveVelocity = Vector3.zero;
        private Vector3 _velocityBase;
        private Ladder _ladder;
        private Vector3 _ladderClimbDirection;
        private RaycastHit _raycastHit;
        private StateMachine<State> _stateMachine;
        private float _animVelZ;

        public StateMachine<State> StateMachine { get { return _stateMachine; } }
        public KccMotor Motor { get { return _motor; } }
        public CharacterConfig Config { get { return _config; } }

        private void Awake()
        {
            InitStateMachine();
            _motor.CharacterController = this;
            _animator = GetComponent<CharacterAnimator>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _stateMachine == null || _motor == null) return;
            D.raw(new Shape.Text(transformCached.position + _motor.Capsule.center, _stateMachine.CurrentState));
        }

        private void InitStateMachine()
        {
            _stateMachine = new StateMachine<State>();
            _stateMachine.AddState(State.Ground);
            _stateMachine.AddState(State.Air);
            _stateMachine.AddState(State.ClimbLadder);
            _stateMachine.CurrentState = State.Air;
        }

        private bool CanClimbLadder()
        {
            DrawPhysics.Raycast(transformCached.position, transformCached.forward, out _raycastHit, _motor.Capsule.radius + _config.LadderClimbDetectDistance, _config.LadderClimbDetectLayerMask);
            if (_raycastHit.collider == null)
                DrawPhysics.Raycast(transformCached.TransformPoint(Vector3.up * _motor.Capsule.height * _config.LadderClimbDetectHeight), transformCached.forward, out _raycastHit, _motor.Capsule.radius + _config.LadderClimbDetectDistance, _config.LadderClimbDetectLayerMask);
            if (_raycastHit.collider == null) return false;
            _ladder = _raycastHit.collider.GetComponent<Ladder>();
            if (_ladder == null) return false;
            _ladderClimbDirection = -_raycastHit.normal;
            _ladderClimbDirection.y = 0f;
            if (Vector3.Angle(_inputMove, _ladderClimbDirection) < _config.LadderClimbTowardAngleMax) return true;
            return false;
        }

        private bool IsLeavingLadder()
        {
            if (transformCached.position.y > _ladder.TopPosition.y) return true;
            if (transformCached.position.y + _motor.Capsule.height * _config.LadderClimbDetectHeight < _ladder.BottomPosition.y) return true;
            return false;
        }

        private bool CanJump()
        {
            if (_jumpCount >= _config.JumpMultipleCount) return false;
            switch (_stateMachine.CurrentState)
            {
                case State.Air:
                    if (_jumpCount <= 0 && _groundStableTime > _config.JumpCoyoteTime) return false;
                    break;
            }
            if (_jumpCount >= 1 && _jumpTimeSinceLast < _config.JumpMultipleDelayBetween) return false;
            if (_jumpTimeSinceRequest > _config.JumpRequestBufferTime) return false;
            return true;
        }

        private void DoJump(ref Vector3 currentVelocity)
        {
            Vector3 jumpDirection = _motor.CharacterUp;
            if (_motor.GroundingStatus.FoundAnyGround && !_motor.GroundingStatus.IsStableOnGround)
                jumpDirection = _motor.GroundingStatus.GroundNormal;
            _motor.ForceUnground();
            currentVelocity += (jumpDirection * _config.JumpSpeed * _jumpSpeedMultiple) - Vector3.Project(currentVelocity, _motor.CharacterUp);
            _jumpTimeSinceRequest = Mathf.Infinity;
            _jumpTimeSinceLast = 0f;
            _jumpCount++;
            if (_animator != null) _animator.SetJumping(true);
        }

        private void DoJumpOffLadder(ref Vector3 currentVelocity)
        {
            currentVelocity += -_ladderClimbDirection * _config.JumpOffLadderSpeed;
            _jumpTimeSinceRequest = Mathf.Infinity;
            _jumpTimeSinceLast = 0f;
            _jumpCount++;
            if (_animator != null) _animator.SetClimbing(false);
            if (_animator != null) _animator.SetJumping(true);
        }

        private void HandleAdditiveVelocity(ref Vector3 currentVelocity)
        {
            if (_additiveVelocity.sqrMagnitude <= 0f) return;
            currentVelocity += _additiveVelocity;
            _additiveVelocity = Vector3.zero;
        }

        public void SetInputs(ref CharacterInput input)
        {
            _inputMove = input.MoveVector;
            _inputRotation = input.LookVector;
            if (input.Jump) _jumpTimeSinceRequest = 0f;
        }

        public void SetInputs(ref CharacterInputAI aiInput)
        {
            aiInput.moveVector.y = 0f;
            aiInput.moveVector = aiInput.moveVector.normalized;
            _inputMove = aiInput.moveVector;
            _inputRotation = aiInput.lookVector;
            if (aiInput.jump) _jumpTimeSinceRequest = 0f;
        }

        public void AddVelocity(Vector3 velocity, bool isAirForce = false)
        {
            switch (_stateMachine.CurrentState)
            {
                case State.Ground:
                    if (isAirForce) _motor.ForceUnground();
                    _additiveVelocity += velocity;
                    break;
                case State.Air:
                    if (isAirForce) _additiveVelocity += velocity;
                    break;
            }
        }

        public Vector3 GetVelocityBaseNormalized()
        {
            switch (_stateMachine.CurrentState)
            {
                case State.Ground: return _velocityBase / _config.GroundMoveSpeedMax;
                case State.Air: return _velocityBase / _config.AirMoveSpeedMax;
                default: return _velocityBase / _config.GroundMoveSpeedMax;
            }
        }

        public float GetClimbLadderPositionY()
        {
            if (_ladder == null) return 0f;
            return _ladder.TransformCached.TransformPoint(transformCached.position).y;
        }

        void IKccMotor.BeforeCharacterUpdate(float deltaTime)
        {
            switch (_stateMachine.CurrentState)
            {
                case State.Air:
                case State.Ground:
                    if (_inputMove.sqrMagnitude > Mathf.Epsilon && CanClimbLadder())
                    {
                        if(_stateMachine.CurrentState != State.ClimbLadder)
                        {
                            _stateMachine.CurrentState = State.ClimbLadder;
                            _animator.SetClimbing(true);
                        }
                    }

                    break;
            }
        }

        void IKccMotor.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 lookVector = Vector3.zero;
            switch (_stateMachine.CurrentState)
            {
                case State.Air:
                case State.Ground:
                    lookVector = _inputRotation;
                    break;
                case State.ClimbLadder:
                    lookVector = _ladderClimbDirection;
                    break;
            }

            if (lookVector.sqrMagnitude > 0f && _config.OrientationBonusSharpness > 0f)
            {
                Vector3 smoothedLookInputDirection = Vector3.Slerp(_motor.CharacterForward, lookVector, 1 - Mathf.Exp(-_config.OrientationBonusSharpness * deltaTime)).normalized;
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, _motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            Vector3 smoothedGravityDir;

            switch (_config.OrientationBonus)
            {
                case CharacterConfig.OrientationBonusMethod.TowardsGravity:
                    smoothedGravityDir = Vector3.Slerp(currentUp, -_config.Gravity.normalized, 1 - Mathf.Exp(-_config.OrientationBonusSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;

                case CharacterConfig.OrientationBonusMethod.TowardsGroundSlopeAndGravity:
                    if (_motor.GroundingStatus.IsStableOnGround)
                    {
                        Vector3 initialCharacterBottomHemiCenter = _motor.TransientPosition + (currentUp * _motor.Capsule.radius);
                        Vector3 smoothedGroundNormal = Vector3.Slerp(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-_config.OrientationBonusSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;
                        _motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * _motor.Capsule.radius));
                    }
                    else
                    {
                        smoothedGravityDir = Vector3.Slerp(currentUp, -_config.Gravity.normalized, 1 - Mathf.Exp(-_config.OrientationBonusSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    break;

                default:
                    smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-_config.OrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;
            }
        }

        void IKccMotor.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (_stateMachine.CurrentState)
            {
                case State.Air:
                    if (_inputMove.sqrMagnitude > 0f)
                    {
                        Vector3 addedVelocity = _inputMove * _config.AirAccelerationSpeed * deltaTime;
                        Vector3 currentVelocityOnPlane = Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp);
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnPlane + addedVelocity, _config.AirMoveSpeedMax);
                        addedVelocity = newTotal - currentVelocityOnPlane;
                        if (_motor.GroundingStatus.FoundAnyGround)
                        {
                            if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                            {
                                Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                            }
                        }
                        currentVelocity += addedVelocity;
                    }
                    currentVelocity += _config.Gravity * deltaTime;
                    currentVelocity *= (1f / (1f + (_config.AirDrag * deltaTime)));
                    if (CanJump()) DoJump(ref currentVelocity);
                    HandleAdditiveVelocity(ref currentVelocity);
                    _animVelZ = Mathf.Lerp(_animVelZ, 0f, 1f - Mathf.Exp(-_animVelSharpness * deltaTime));
                    if (_animator != null) _animator.SetVelocityZ(_animVelZ);
                    break;

                case State.Ground:
                    if (_motor.GroundingStatus.IsStableOnGround)
                    {
                        float currentVelocityMagnitude = currentVelocity.magnitude;
                        Vector3 effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;
                        currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
                        Vector3 inputRight = Vector3.Cross(_inputMove, _motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _inputMove.magnitude;
                        Vector3 targetMovementVelocity = reorientedInput * _config.GroundMoveSpeedMax;
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_config.GroundMoveSharpness * deltaTime));
                    }
                    if (CanJump()) DoJump(ref currentVelocity);
                    _velocityBase = currentVelocity;
                    HandleAdditiveVelocity(ref currentVelocity);

                    float forwardSpeed = Vector3.Dot(Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp), _motor.CharacterForward);
                    float norm = 0f;
                    if (_config.GroundMoveSpeedMax > 0f) norm = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / _config.GroundMoveSpeedMax);
                    _animVelZ = Mathf.Lerp(_animVelZ, norm, 1f - Mathf.Exp(-_animVelSharpness * deltaTime));
                    if (_animator != null) _animator.SetVelocityZ(_animVelZ);
                    break;

                case State.ClimbLadder:
                    float desireVelocityY = transformCached.InverseTransformPoint(transformCached.position + _inputMove).z * _config.LadderClimbSpeedMax;
                    currentVelocity.y = Mathf.Lerp(currentVelocity.y, desireVelocityY, 1f - Mathf.Exp(-_config.LadderClimbSharpness * deltaTime));
                    currentVelocity.x = 0f;
                    currentVelocity.z = 0f;

                    _animator.SetClimbY(desireVelocityY);

                    if (desireVelocityY > 0f) _motor.ForceUnground();
                    if (CanJump())
                    {
                        _stateMachine.CurrentState = State.Air;
                        DoJumpOffLadder(ref currentVelocity);
                    }
                    _animVelZ = Mathf.Lerp(_animVelZ, 0f, 1f - Mathf.Exp(-_animVelSharpness * deltaTime));
                    if (_animator != null)
                    {
                        _animator.SetVelocityZ(_animVelZ);
                        float climb01 = 0f;
                        if (_config.LadderClimbSpeedMax > 0f) climb01 = Mathf.Clamp(currentVelocity.y / _config.LadderClimbSpeedMax, -1f, 1f);
                        _animator.SetClimbY(climb01);
                    }
                    break;
            }
        }

        void IKccMotor.AfterCharacterUpdate(float deltaTime)
        {
            switch (_stateMachine.CurrentState)
            {
                case State.Ground:
                    if (!_motor.GroundingStatus.IsStableOnGround)
                        _stateMachine.CurrentState = State.Air;
                    break;
                case State.Air:
                    if (_motor.GroundingStatus.IsStableOnGround)
                    {
                        _animator.SetJumping(false);
                        _stateMachine.CurrentState = State.Ground;
                    }

                    break;
                case State.ClimbLadder:
                    if (_motor.GroundingStatus.IsStableOnGround)
                        _stateMachine.CurrentState = State.Ground;
                    if (IsLeavingLadder())
                    {
                        _animator.SetClimbing(false);
                        _stateMachine.CurrentState = State.Air;
                    }
                    break;
            }

            if (_motor.GroundingStatus.IsStableOnGround || _stateMachine.CurrentState == State.ClimbLadder)
            {
                if (_jumpTimeSinceLast > 0f) _jumpCount = 0;
            }

            _jumpTimeSinceRequest += deltaTime;
            _jumpTimeSinceLast += deltaTime;

            if (_motor.GroundingStatus.IsStableOnGround) _groundStableTime = 0f;
            else _groundStableTime += deltaTime;
        }

        void IKccMotor.PostGroundingUpdate(float deltaTime) { }

        bool IKccMotor.IsColliderValidForCollisions(Collider collider)
        {
            if (_ignoredColliders.Length == 0) return true;
            if (_ignoredColliders.Contains(collider)) return false;
            return true;
        }

        void IKccMotor.OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        void IKccMotor.OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        void IKccMotor.ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        void IKccMotor.OnDiscreteCollisionDetected(Collider hitCollider) { }
    }
}
