// CharacterRagdoll.cs
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

        private Rigidbody[] _ragdollBodies;
        private Collider[] _ragdollColliders;

        private Vector3[] _defaultLocalPos;
        private Quaternion[] _defaultLocalRot;

        private bool _poseCached;

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            if (_smrs.Count == 0)
                _smrs.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));

            if (_parts == null || _parts.Length == 0)
                BuildPartsFromChildren();

            CacheDefaultPose();

            CollectRagdollRuntimeArrays();
            SetRagdollActive(false);
        }

        private void CollectRagdollRuntimeArrays()
        {
            var bodies = new List<Rigidbody>(_parts.Length);
            var cols = new List<Collider>(_parts.Length);

            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i].rigidbody) bodies.Add(_parts[i].rigidbody);
                if (_parts[i].collider) cols.Add(_parts[i].collider);
            }

            _ragdollBodies = bodies.ToArray();
            _ragdollColliders = cols.ToArray();
        }

        private void BuildPartsFromChildren()
        {
            var rigidbodies = GetComponentsInChildren<Rigidbody>(true);
            _parts = new Part[rigidbodies.Length];

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                _parts[i].rigidbody = rigidbodies[i];
                _parts[i].transform = rigidbodies[i].transform;
                _parts[i].collider = rigidbodies[i].GetComponent<Collider>();
            }
        }

        private void CacheDefaultPose()
        {
            if (_parts == null || _parts.Length == 0) return;

            _defaultLocalPos = new Vector3[_parts.Length];
            _defaultLocalRot = new Quaternion[_parts.Length];

            for (int i = 0; i < _parts.Length; i++)
            {
                if (!_parts[i].transform) continue;
                _defaultLocalPos[i] = _parts[i].transform.localPosition;
                _defaultLocalRot[i] = _parts[i].transform.localRotation;
            }

            _poseCached = true;
        }

        private void RestoreDefaultPose()
        {
            if (!_poseCached) return;
            if (_parts == null || _parts.Length == 0) return;

            for (int i = 0; i < _parts.Length; i++)
            {
                var t = _parts[i].transform;
                if (!t) continue;
                t.localPosition = _defaultLocalPos[i];
                t.localRotation = _defaultLocalRot[i];
            }
        }

        public void SetRagdollActive(bool active)
        {
            if (_ragdollBodies == null || _ragdollBodies.Length == 0)
                CollectRagdollRuntimeArrays();

            if (!active)
            {
                if (_ragdollBodies != null)
                {
                    for (int i = 0; i < _ragdollBodies.Length; i++)
                    {
                        var rb = _ragdollBodies[i];
                        if (!rb) continue;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                        rb.interpolation = RigidbodyInterpolation.None;
                    }
                }

                if (_ragdollColliders != null)
                {
                    for (int i = 0; i < _ragdollColliders.Length; i++)
                    {
                        var c = _ragdollColliders[i];
                        if (c) c.enabled = false;
                    }
                }

                RestoreDefaultPose();

                if (_anim)
                {
                    _anim.enabled = true;
                    _anim.Update(0f);
                }

                return;
            }

            if (_anim) _anim.enabled = false;

            if (_ragdollColliders != null)
            {
                for (int i = 0; i < _ragdollColliders.Length; i++)
                {
                    var c = _ragdollColliders[i];
                    if (c) c.enabled = true;
                }
            }

            if (_ragdollBodies != null)
            {
                for (int i = 0; i < _ragdollBodies.Length; i++)
                {
                    var rb = _ragdollBodies[i];
                    if (!rb) continue;
                    rb.isKinematic = false;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
        }

        public void ActivateRagdoll(Vector3 hitForce, Vector3 hitPosition)
        {
            SetRagdollActive(true);

            if (_ragdollBodies == null) return;

            for (int i = 0; i < _ragdollBodies.Length; i++)
            {
                var rb = _ragdollBodies[i];
                if (!rb) continue;
                rb.AddForceAtPosition(hitForce, hitPosition, ForceMode.Impulse);
            }
        }

        public Vector3 GetRagdollAnchorWorld()
        {
            if (_parts == null || _parts.Length == 0) return transform.position;

            Rigidbody best = null;
            float bestMass = -1f;

            for (int i = 0; i < _parts.Length; i++)
            {
                var rb = _parts[i].rigidbody;
                if (!rb) continue;
                if (rb.mass > bestMass)
                {
                    bestMass = rb.mass;
                    best = rb;
                }
            }

            if (best) return best.worldCenterOfMass;

            for (int i = 0; i < _parts.Length; i++)
                if (_parts[i].transform) return _parts[i].transform.position;

            return transform.position;
        }

        public void SnapMotorToRagdollAnchor(Character character)
        {
            if (character == null || character.motor == null) return;

            Vector3 anchor = GetRagdollAnchorWorld();
            Vector3 cur = character.transform.position;
            anchor.y = cur.y;
            character.motor.SetPosition(anchor);
        }

        [Button]
        public void Explode()
        {
            if (_anim) _anim.enabled = false;
            for (int i = 0; i < _smrs.Count; i++)
                if (_smrs[i]) _smrs[i].updateWhenOffscreen = true;

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

            for (int i = 0; i < _explodedClones.Count; i++)
            {
                var go = _explodedClones[i];
                if (!go) continue;

                var rb = go.GetComponent<Rigidbody>();
                if (!rb) continue;

                rb.useGravity = _useGravityOnClones;
                if (_explodeForce > 0f)
                    rb.AddExplosionForce(_explodeForce, transform.position, _explodeRadius, _upwardsModifier, ForceMode.Impulse);
            }

            for (int i = 0; i < _smrs.Count; i++)
                if (_smrs[i]) _smrs[i].enabled = false;
        }

        private void Explode_RagdollParts()
        {
            for (int i = 0; i < _smrs.Count; i++)
                if (_smrs[i]) _smrs[i].enabled = false;

            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i].collider) _parts[i].collider.enabled = true;

                if (_parts[i].rigidbody)
                {
                    _parts[i].rigidbody.isKinematic = false;
                    _parts[i].rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                    if (_explodeForce > 0f)
                        _parts[i].rigidbody.AddExplosionForce(_explodeForce, transform.position, _explodeRadius, _upwardsModifier, ForceMode.Impulse);
                }
            }
        }

        private void CreateStaticClonesFromSmrs()
        {
            if (_explodedClones.Count > 0)
            {
                for (int i = 0; i < _explodedClones.Count; i++)
                    if (_explodedClones[i]) Destroy(_explodedClones[i]);
                _explodedClones.Clear();
            }

            for (int i = 0; i < _smrs.Count; i++)
            {
                var smr = _smrs[i];
                if (!smr || !smr.sharedMesh) continue;

                var baked = new Mesh();
                smr.BakeMesh(baked, true);
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
            for (int i = 0; i < _explodedClones.Count; i++)
                if (_explodedClones[i]) DestroyImmediate(_explodedClones[i]);
            _explodedClones.Clear();

            for (int i = 0; i < _smrs.Count; i++)
                if (_smrs[i]) _smrs[i].enabled = true;

            SetRagdollActive(false);
        }

        [Button]
        private void GetParts()
        {
            BuildPartsFromChildren();
            CacheDefaultPose();
            CollectRagdollRuntimeArrays();
            SetRagdollActive(false);

            _smrs.Clear();
            _smrs.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        public void ResetCanmove()
        {
            PlayerControl pControl = GetComponentInParent<PlayerControl>();
            if (pControl == null) return;
            pControl.canMove = true;
        }
    }
}
