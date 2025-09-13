using Hichu;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Vertx.Debugging;
using Kcc.Base;

namespace Game
{
    public class AI : MonoBehaviour
    {
        static readonly float s_distanceThreshold = 0.2f;

        private enum State
        {
            Stop,
            Idle,
            Chase,
        }

        [Title("Reference")]
        [SerializeField] private Character _character;
        [SerializeField] private Transform _characterFoward;

        [Title("Config")]
        [SerializeField] private float _turnSpeed = 120f;
        [SerializeField] private Vector2 _idleDurationRange = new Vector2(0.5f, 1f);

        private CharacterInputAI _inputAI;

        private AIAvoidance _avoidance;

        private StateMachine<State> _stateMachine;

        private AIStateIdle _stateIdle;
        private AIStateChase _stateChase;

        private Transform _targetTransform;
        private Vector3 _targetPosition;

        public Character character { get { return _character; } }

        public Transform characterFoward { get { return _characterFoward; } }

        public AIAvoidance avoidance { get { return _avoidance; } }

        public Vector3 positionGoal { get { if (_targetTransform != null) return _targetTransform.position; return _targetPosition; } }

        public event Action eventIdleComplete;
        public event Action eventChaseComplete;
        
        private void Start()
        {
            _avoidance = GetComponent<AIAvoidance>();

            _inputAI = new CharacterInputAI();

            _character.eventDie += Character_EventDie;
            _character.eventRevive += Character_EventRevive;

            InitStateMachine();
        }

        private void Update()
        {
            D.raw(new Shape.Text(_character.transformCached.position, _stateMachine.CurrentState));

            _stateMachine.Update();

            _character.cControl.SetInputs(ref _inputAI);
        }

        private void Character_EventDie()
        {
            _stateMachine.CurrentState = State.Stop;
        }

        private void Character_EventRevive()
        {
            _stateMachine.CurrentState = State.Idle;
        }

        private void InitStateMachine()
        {
            _stateMachine = new StateMachine<State>();

            _stateIdle = new AIStateIdle(this);
            _stateChase = new AIStateChase(this);

            _stateIdle.eventComplete += StateIdle_EventComplete;
            _stateChase.eventComplete += StateChase_EventComplete;

            _stateMachine.AddState(State.Idle, _stateIdle);
            _stateMachine.AddState(State.Chase, _stateChase);
            _stateMachine.AddState(State.Stop);

            Stop();
        }

        private void StateChase_EventComplete()
        {
            eventChaseComplete?.Invoke();
        }

        private void StateIdle_EventComplete()
        {
            eventIdleComplete?.Invoke();
        }

        public void Stop()
        {
            _stateMachine.CurrentState = State.Stop;
        }

        public void Win()
        {
            _stateMachine.CurrentState = State.Stop;

            //_character.Win();
        }

        public void Idle()
        {
            _stateIdle.SetTimeOut(_idleDurationRange.RandomWithin());

            _stateMachine.CurrentState = State.Idle;
        }

        public void IdleWaitForDistance(Transform target, float distance)
        {
            _stateIdle.SetTargetDistance(target, distance);

            _stateMachine.CurrentState = State.Idle;
        }

        public void Chase(Transform target)
        {
            _targetTransform = target;
            _targetPosition = target.position;

            _stateMachine.CurrentState = State.Chase;
        }

        public void Chase(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            _targetTransform = null;

            _stateMachine.CurrentState = State.Chase;
        }

        public void JumpIfNecessery()
        {
            if (_character.motor.GroundingStatus.IsStableOnGround)
            {
                _characterFoward.forward = Vector3.ProjectOnPlane(_character.motor.CharacterForward, _character.motor.GroundingStatus.GroundNormal);

                if (_avoidance.IsObstacleAhead())
                    _inputAI.jump = true;
                else if (_avoidance.IsFallAhead())
                    _inputAI.jump = true;
                else
                    _inputAI.jump = false;
            }
            else
            {
                _inputAI.jump = false;
            }
        }

        public bool MoveToGoal()
        {
            return MoveTo(positionGoal);
        }

        public bool MoveTo(Vector3 destination)
        {
            D.raw(new Shape.Line(_character.transformCached.position, destination), Color.red);

            if (Mathf.Abs(_character.transformCached.position.x - destination.x) < s_distanceThreshold && Mathf.Abs(_character.transformCached.position.z - destination.z) < s_distanceThreshold)
            {
                _inputAI.moveVector = Vector3.zero;

                return true;
            }
            else
            {
                _inputAI.moveVector = Vector3.RotateTowards(_character.motor.CharacterForward, (destination - _character.transformCached.position).normalized, _turnSpeed * Time.deltaTime, 0f);
                _inputAI.moveVector.Normalize();
                _inputAI.moveVector.y = 0f;

                _inputAI.lookVector = _inputAI.moveVector;

                return false;
            }
        }

        public void SetIdleDurationRange(Vector2 range)
        {
            _idleDurationRange = range;
        }
    }
}