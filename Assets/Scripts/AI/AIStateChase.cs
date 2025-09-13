using Hichu;
using System;
using UnityEngine;

namespace Game
{
    public class AIStateChase : IStateMachine
    {
        private AI _ai;

        private bool _evading = false;
        private Vector3 _evadePosition;
        private float _timeOut;

        public event Action eventComplete;

        public AIStateChase(AI ai)
        {
            _ai = ai;
        }

        void IStateMachine.Init()
        {
        }

        void IStateMachine.OnStart()
        {
            _timeOut = 5.0f;
            _evading = false;
        }

        void IStateMachine.OnUpdate()
        {
            _ai.JumpIfNecessery();

            _timeOut -= Time.deltaTime;

            if (_timeOut <= 0f)
            {
                eventComplete?.Invoke();
                return;
            }

            // Evade character ahead
            if (!_evading)
            {
                Collider characterAhead = _ai.avoidance.GetCharacterAhead();

                if (characterAhead != null)
                {
                    _evading = true;
                    _evadePosition = characterAhead.transform.position + Quaternion.AngleAxis(UnityEngine.Random.Range(-90f, -270f), Vector3.up) * (_ai.positionGoal - characterAhead.transform.position).normalized * 2f;
                }
            }

            if (_evading)
            {
                if (_ai.MoveTo(_evadePosition))
                    _evading = false;
            }
            else
            {
                if (_ai.MoveToGoal())
                    eventComplete?.Invoke();
            }
        }

        void IStateMachine.OnFixedUpdate()
        {

        }

        void IStateMachine.OnStop()
        {
        }
    }
}
