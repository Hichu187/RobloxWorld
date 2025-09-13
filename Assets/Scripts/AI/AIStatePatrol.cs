using Hichu;
using System;
using UnityEngine;
using Kcc.Base;

namespace Game
{
    public class AIStatePatrol : IStateMachine
    {
        private AI _ai;
        private float _radius;
        private Vector3 _center;
        private Vector3 _nextDestination;

        public event Action eventComplete;

        public AIStatePatrol(AI ai)
        {
            _ai = ai;
        }

        void IStateMachine.Init()
        {

        }

        void IStateMachine.OnStart()
        {
            _center = _ai.character.transform.position;
            PickNextPoint();
        }

        void IStateMachine.OnUpdate()
        {
            if (_ai.MoveTo(_nextDestination))
            {
                PickNextPoint();
            }
        }

        void IStateMachine.OnFixedUpdate()
        {

        }

        void IStateMachine.OnStop()
        {

        }

        private void PickNextPoint()
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * _radius;
            _nextDestination = _center + new Vector3(r.x, 0f, r.y);
        }

        public void SetRadius(float radius, Vector3 center)
        {
            _radius = radius;
            _center = center;
        }
    }
}
