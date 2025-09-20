using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class CharacterRagdoll : MonoBehaviour
    {
        public enum ExplodeMode { ClonesOnly, RagdollParts }

        [System.Serializable]
        public struct Part
        {
            public Transform transform;
            public Rigidbody rigidbody;
            public Collider collider;
        }

        [Title("Reference")]
        [SerializeField] private Part[] _parts;

        [SerializeField] private List<SkinnedMeshRenderer> _smrs = new();
        private readonly List<GameObject> _explodedClones = new();

        [Title("Config")]
        [SerializeField] private ExplodeMode _mode = ExplodeMode.ClonesOnly;
        [SerializeField] private float _explodeForce = 0f;
        [SerializeField] private float _explodeRadius = 2f;
        [SerializeField] private float _upwardsModifier = 0.25f;
        [SerializeField] private bool _useGravityOnClones = true;

        private Animator _anim;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            if (_smrs.Count == 0)
                _smrs.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        [Button]
        public void Explode()
        {
            if (_anim) _anim.enabled = false;
            foreach (var smr in _smrs)
                if (smr) smr.updateWhenOffscreen = true;

            switch (_mode)
            {
                case ExplodeMode.ClonesOnly:
                    Explode_ClonesOnly();
                    break;
                case ExplodeMode.RagdollParts:
                    Explode_RagdollParts();
                    break;
            }
        }

        private void Explode_ClonesOnly()
        {
            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i].rigidbody)
                {
                    _parts[i].rigidbody.linearVelocity = Vector3.zero;
                    _parts[i].rigidbody.angularVelocity = Vector3.zero;
                    _parts[i].rigidbody.isKinematic = true;
                }
                if (_parts[i].collider) _parts[i].collider.enabled = false;
            }

            CreateStaticClonesFromSmrs();

            foreach (var go in _explodedClones)
            {
                if (!go) continue;
                var rb = go.GetComponent<Rigidbody>();
                if (!rb) continue;

                rb.useGravity = _useGravityOnClones;
                if (_explodeForce > 0f)
                {
                    rb.AddExplosionForce(_explodeForce, transform.position, _explodeRadius, _upwardsModifier, ForceMode.Impulse);
                }
            }

            foreach (var smr in _smrs) if (smr) smr.enabled = false;
        }

        private void Explode_RagdollParts()
        {
            foreach (var smr in _smrs) if (smr) smr.enabled = false;

            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i].collider) _parts[i].collider.enabled = true;

                if (_parts[i].rigidbody)
                {
                    _parts[i].rigidbody.isKinematic = false;
                    _parts[i].rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                    if (_explodeForce > 0f)
                    {
                        _parts[i].rigidbody.AddExplosionForce(
                            _explodeForce,
                            transform.position,
                            _explodeRadius,
                            _upwardsModifier,
                            ForceMode.Impulse
                        );
                    }
                }
            }
        }

        private void CreateStaticClonesFromSmrs()
        {
            if (_explodedClones.Count > 0)
            {
                foreach (var go in _explodedClones) if (go) Destroy(go);
                _explodedClones.Clear();
            }

            foreach (var smr in _smrs)
            {
                if (!smr || !smr.sharedMesh) continue;

                var baked = new Mesh();
                smr.BakeMesh(baked, useScale: true);
                baked.RecalculateBounds();

                var clone = new GameObject($"{smr.name}_ExplodedClone");
                _explodedClones.Add(clone);

                clone.layer = smr.gameObject.layer;
                clone.transform.SetParent(smr.transform.parent, false);
                clone.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
                clone.transform.localScale = smr.transform.lossyScale;

                var mf = clone.AddComponent<MeshFilter>();
                mf.sharedMesh = baked;

                var mr = clone.AddComponent<MeshRenderer>();
                var srcMats = smr.sharedMaterials;
                var mats = new Material[baked.subMeshCount];
                for (int m = 0; m < mats.Length; m++)
                    mats[m] = (m < srcMats.Length) ? srcMats[m] : (srcMats.Length > 0 ? srcMats[0] : null);
                mr.sharedMaterials = mats;

                mr.shadowCastingMode = smr.shadowCastingMode;
                mr.receiveShadows = smr.receiveShadows;
                mr.lightProbeUsage = smr.lightProbeUsage;
                mr.reflectionProbeUsage = smr.reflectionProbeUsage;
                mr.probeAnchor = smr.probeAnchor;

                var sc = clone.AddComponent<SphereCollider>();
                sc.center = baked.bounds.center;
                sc.radius = baked.bounds.extents.magnitude * 0.5f;

                var rb = clone.AddComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }

        [Button]
        public void Restore()
        {
            foreach (var go in _explodedClones) if (go) DestroyImmediate(go);
            _explodedClones.Clear();

            foreach (var smr in _smrs) if (smr) smr.enabled = true;

            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i].rigidbody)
                {
                    _parts[i].rigidbody.linearVelocity = Vector3.zero;
                    _parts[i].rigidbody.angularVelocity = Vector3.zero;
                    _parts[i].rigidbody.isKinematic = true;
                }
                if (_parts[i].collider) _parts[i].collider.enabled = false;
            }

            if (_anim) _anim.enabled = true;
        }

        [Button]
        public void ActivateRagdoll()
        {

        }

        [Button]
        private void GetParts()
        {
            var rigidbodies = GetComponentsInChildren<Rigidbody>(true);
            _parts = new Part[rigidbodies.Length];

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                _parts[i].rigidbody = rigidbodies[i];
                _parts[i].transform = rigidbodies[i].transform;
                _parts[i].collider = rigidbodies[i].GetComponent<Collider>();

                if (_parts[i].collider) _parts[i].collider.enabled = false;
                rigidbodies[i].isKinematic = true;
            }

            _smrs.Clear();
            _smrs.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }
    }
}
