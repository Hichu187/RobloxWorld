using Cysharp.Threading.Tasks;
using Hichu;
using Kcc;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class Character : MonoCached
    {
        public enum State
        {
            Normal,
            Die,
        }

        public bool isPlayer = false;
        public bool isAutoLoadSkin = false;

        public KccMotor motor;
        public CharacterControl cControl;
        public CharacterRenderer cRender;
        public CharacterCamera cCamera;
        public CharacterCombat cCombat;
        public CharacterAnimator cAnim;
        public CharacterInteract cInteract;
        public CharacterRagdoll cRagdoll;
        public FieldOfView fov;
        public CharacterItemManager itemManager;

        private State _state = State.Normal;

        public event Action eventDie;
        public event Action eventRevive;

        private void OnEnable()
        {

        }

        private void Awake()
        {
            if (isAutoLoadSkin)
            {
                if (isPlayer)
                {
                    cRender.LoadSkin(0);
                }
                else
                {
                    cRender.LoadSkin(UnityEngine.Random.Range(0, FactorySkin.skin.Count));
                }
            }
        }

        private void Start()
        {
            SetItemManager();
        }


        private void OnCollisionEnter(Collision collision)
        {
            ICharacterCollidable collidable = collision.gameObject.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnCollisionEnter(GetComponent<CharacterControl>());
        }

        private void OnCollisionExit(Collision collision)
        {
            ICharacterCollidable collidable = collision.gameObject.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnCollisionExit(GetComponent<CharacterControl>());
        }

        private void OnTriggerEnter(Collider other)
        {
            ICharacterCollidable collidable = other.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnTriggerEnter(GetComponent<CharacterControl>());
        }

        private void OnTriggerExit(Collider other)
        {
            ICharacterCollidable collidable = other.GetComponent<ICharacterCollidable>();

            if (collidable != null)
                collidable.OnTriggerExit(GetComponent<CharacterControl>());
        }

        public void SetItemManager()
        {
            itemManager = GetComponentInChildren<CharacterItemManager>();
            if (itemManager == null) return;
            if (isPlayer)
            {
                itemManager.ActiveItem(DataItem.current);
            }
            else
            {
                itemManager.ActiveItem(UnityEngine.Random.Range(0,7));
            }

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