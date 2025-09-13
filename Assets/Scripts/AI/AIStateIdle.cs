using Hichu;
using System;
using UnityEngine;

namespace Game
{
    public class AIStateIdle : IStateMachine
    {
        private AI _ai;

        private float _time;

        private float _timeOut = 0f;

        private Transform _target;

        private float _targetDistance;

        public event Action eventComplete;

        public AIStateIdle(AI ai)
        {
            _ai = ai;
        }

        private void Complete()
        {
            if (!_ai.character.motor.GroundingStatus.IsStableOnGround)
                return;

            eventComplete?.Invoke();
        }

        public void SetTimeOut(float timeOut)
        {
            _timeOut = timeOut;

            _target = null;
            _targetDistance = 0f;
        }

        public void SetTargetDistance(Transform target, float distance)
        {
            _timeOut = 10f;

            _target = target;
            _targetDistance = distance;
        }

        void IStateMachine.Init()
        {
        }

        void IStateMachine.OnStart()
        {
            _time = 0f;
        }

        void IStateMachine.OnUpdate()
        {
            _ai.JumpIfNecessery();
            _ai.MoveToGoal();

            _time += Time.deltaTime;

            if (_time >= _timeOut)
                Complete();

            if (_target != null && Vector3.Distance(_ai.character.transformCached.position, _target.position) < _targetDistance)
                Complete();
        }
        void IStateMachine.OnFixedUpdate()
        {

        }

        void IStateMachine.OnStop()
        {
        }
    }
}