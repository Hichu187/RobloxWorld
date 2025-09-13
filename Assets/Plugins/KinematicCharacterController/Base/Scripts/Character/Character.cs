using Hichu;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Kcc.Base
{
    public class Character : MonoCached
    {
        public enum State
        {
            Normal,
            Die,
        }

        public bool isPlayer = false;

        public KccMotor motor;
        public CharacterController cControl;
        public CharacterCamera cCamera;
        public CharacterCombat cCombat;
        public CharacterInteract cInteract;
        public FieldOfView fov;


        private State _state = State.Normal;

        public event Action eventDie;
        public event Action eventRevive;


        private void OnCollisionEnter(Collision collision)
        {
            ICharacterCollidable collidable = collision.gameObject.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnCollisionEnter(this);
        }

        private void OnCollisionExit(Collision collision)
        {
            ICharacterCollidable collidable = collision.gameObject.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnCollisionExit(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            ICharacterCollidable collidable = other.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnTriggerEnter(this);
        }

        private void OnTriggerExit(Collider other)
        {
            ICharacterCollidable collidable = other.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnTriggerExit(this);
        }


        public void Kill()
        {
            Die();
        }

        private void Die()
        {
            if (_state == State.Die)
                return;

            _state = State.Die;
            SetEnabled(false);

            eventDie?.Invoke();
        }

        public void Revive(Vector3 position, Quaternion rotation)
        {
            _state = State.Normal;

            motor.SetPositionAndRotation(position, rotation);

            SetEnabled(true);

            eventRevive?.Invoke();
        }

        public void SetEnabled(bool enabled)
        {
            motor.enabled = enabled;
            motor.GetComponent<Rigidbody>().isKinematic = !enabled;
        }
    }
}
