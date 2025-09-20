using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class CharacterRagdoll : MonoBehaviour
    {
        [System.Serializable]
        public struct Part
        {
            public Transform transform;
            public Rigidbody rigidbody;
            public Collider collider;
        }

        [Title("Reference")]
        [SerializeField] private Part[] _parts;

        [Title("Config")]
        [SerializeField] private float _explodeForce;
        [SerializeField] private float _explodeRadius;

        public void Explode()
        {
            GetComponent<Animator>().enabled = false;

            SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.updateWhenOffscreen = true;

            for (int i = 0; i < _parts.Length; i++)
            {
                _parts[i].collider.enabled = true;
                _parts[i].rigidbody.isKinematic = false;
                _parts[i].transform.SetParent(transform);

                _parts[i].rigidbody.AddExplosionForce(_explodeForce, transform.position, _explodeRadius);
            }
        }

        [Button]
        private void GetParts()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

            _parts = new Part[rigidbodies.Length];

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                _parts[i].rigidbody = rigidbodies[i];
                _parts[i].transform = rigidbodies[i].transform;
                _parts[i].collider = rigidbodies[i].GetComponent<Collider>();

                rigidbodies[i].isKinematic = true;
                rigidbodies[i].GetComponent<Collider>().enabled = false;
            }
        }
    }
}
