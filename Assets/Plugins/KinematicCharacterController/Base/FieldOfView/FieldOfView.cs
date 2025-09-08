using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kcc.Base
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("FOV")]
        public float radius = 12f;
        [Range(0, 360)] public float angle = 100f;

        [Header("Layers")]
        public LayerMask targetMask;
        public LayerMask obstructionMask;

        [Header("Query Options")]
        [SerializeField] private float eyeHeight = 1.5f;
        [SerializeField] private float targetHeightFallback = 1.0f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("NonAlloc Buffer")]
        [Tooltip("Kích thước buffer cho OverlapSphereNonAlloc. Tự tăng khi đầy.")]
        [SerializeField] private int initialBufferSize = 64;

        public readonly List<Transform> visibleTargets = new();
        public readonly List<Transform> combatables = new();
        public readonly List<Transform> interactables = new();

        public Transform nearestInteractable { get; private set; }

        private readonly Dictionary<Collider, TargetTraits> _traitCache = new();

        private Collider[] _hits;

        private void Awake()
        {
            _hits = new Collider[Mathf.Max(8, initialBufferSize)];
        }

        private void Update()
        {
            FieldOfViewCheck();
        }
        private void FieldOfViewCheck()
        {
            visibleTargets.Clear();
            combatables.Clear();
            interactables.Clear();
            nearestInteractable = null;

            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            int count = Physics.OverlapSphereNonAlloc(
                eyePos,
                radius,
                _hits,
                targetMask,
                triggerInteraction
            );

            if (count >= _hits.Length)
            {
                int newSize = _hits.Length * 2;
                Array.Resize(ref _hits, newSize);
                count = Physics.OverlapSphereNonAlloc(
                    eyePos, radius, _hits, targetMask, triggerInteraction
                );
            }

            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _hits[i];
                if (col == null) continue;

                if (col.attachedRigidbody && col.attachedRigidbody.gameObject == gameObject) continue;
                if (col.gameObject == gameObject) continue;

                if (IsTargetVisible(col, eyePos))
                {
                    Transform tr = col.transform;
                    visibleTargets.Add(tr);

                    TargetTraits traits = GetTraits(col);
                    if ((traits & TargetTraits.Combatable) != 0) combatables.Add(tr);
                    if ((traits & TargetTraits.Interactable) != 0)
                    {
                        interactables.Add(tr);

                        float d = Vector3.SqrMagnitude(tr.position - eyePos);
                        if (d < nearestDist)
                        {
                            nearestDist = d;
                            nearestInteractable = tr;
                        }
                    }
                }
            }
        }

        private bool IsTargetVisible(Collider targetCol, Vector3 eyePos)
        {
            Vector3 targetPos = targetCol.bounds.size.sqrMagnitude > 0.0001f
                ? targetCol.bounds.center
                : targetCol.transform.position + Vector3.up * targetHeightFallback;

            Vector3 dir = (targetPos - eyePos);

            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return false;
            dir.Normalize();

            if (Vector3.Angle(transform.forward, dir) > angle * 0.5f)
                return false;

            float dist = Vector3.Distance(eyePos, targetPos);

            if (Physics.Raycast(eyePos, dir, dist, obstructionMask, triggerInteraction))
                return false;

            return true;
        }

        private TargetTraits GetTraits(Collider col)
        {
            if (_traitCache.TryGetValue(col, out var cached))
                return cached;

            var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
            TargetTraits traits = TargetTraits.None;

            if (go.TryGetComponent(out TargetTrait tt))
                traits |= tt.traits;

            if (HasComponentInParents<ICombatable>(go)) traits |= TargetTraits.Combatable;
            if (HasComponentInParents<IInteractable>(go)) traits |= TargetTraits.Interactable;

            _traitCache[col] = traits;
            return traits;
        }

        private bool HasComponentInParents<T>(GameObject go)
        {
            return go.GetComponentInParent(typeof(T)) != null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            Handles.color = Color.yellow;
            Handles.DrawWireDisc(eyePos, Vector3.up, radius);

            Handles.color = new Color(1f, 1f, 0f, 0.2f);
            Handles.DrawSolidArc(
                eyePos,
                Vector3.up,
                Quaternion.Euler(0, -angle * 0.5f, 0) * transform.forward,
                angle,
                radius
            );

            if (combatables.Count > 0 || interactables.Count > 0)
            {
                var both = new HashSet<Transform>(combatables);
                both.IntersectWith(interactables);

                Handles.color = Color.magenta;
                foreach (var t in both)
                    if (t) Handles.DrawLine(eyePos, t.position);

                Handles.color = Color.red;
                foreach (var t in combatables)
                    if (t && !both.Contains(t)) Handles.DrawLine(eyePos, t.position);

                Handles.color = Color.cyan;
                foreach (var t in interactables)
                    if (t && !both.Contains(t)) Handles.DrawLine(eyePos, t.position);
            }
        }
#endif
    }
}
