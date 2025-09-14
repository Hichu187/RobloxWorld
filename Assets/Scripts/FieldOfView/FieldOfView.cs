using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

    public class FieldOfView : MonoBehaviour
    {
        [Min(0f)] public float radius = 12f;
        [Range(0, 360)] public float angle = 100f;

        public LayerMask targetMask;
        public LayerMask obstructionMask;

        [SerializeField] private float eyeHeight = 1.5f;
        [SerializeField] private float targetHeightFallback = 1.0f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [SerializeField] private int initialBufferSize = 64;

        [SerializeField] private int maxVisibleTargets = 64;
        [SerializeField] private int maxCombatables = 8;
        [SerializeField] private int maxInteractables = 8;

        private List<Transform> visibleTargets = new();
        public readonly List<Transform> combatables = new();
        public readonly List<Transform> interactables = new();

        public Transform nearestInteractable { get; private set; }

        private readonly Dictionary<Collider, TargetTraits> _traitCache = new();
        private readonly Dictionary<Transform, float> _distCache = new();

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
            _distCache.Clear();

            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            int count = Physics.OverlapSphereNonAlloc(
                eyePos, radius, _hits, targetMask, triggerInteraction
            );

            if (count >= _hits.Length)
            {
                int newSize = _hits.Length * 2;
                System.Array.Resize(ref _hits, newSize);
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
                    float sqrDist = (tr.position - eyePos).sqrMagnitude;
                    _distCache[tr] = sqrDist;

                    visibleTargets.Add(tr);

                    TargetTraits traits = GetTraits(col);
                    if ((traits & TargetTraits.Combatable) != 0) combatables.Add(tr);
                    if ((traits & TargetTraits.Interactable) != 0)
                    {
                        interactables.Add(tr);
                        if (sqrDist < nearestDist)
                        {
                            nearestDist = sqrDist;
                            nearestInteractable = tr;
                        }
                    }
                }
            }

            SortByDistance(visibleTargets);
            SortByDistance(combatables);
            SortByDistance(interactables);

            TrimList(visibleTargets, maxVisibleTargets);
            TrimList(combatables, maxCombatables);
            TrimList(interactables, maxInteractables);
        }

        private bool IsTargetVisible(Collider targetCol, Vector3 eyePos)
        {
            Vector3 targetPos = targetCol.bounds.size.sqrMagnitude > 0.0001f
                ? targetCol.bounds.center
                : targetCol.transform.position + Vector3.up * targetHeightFallback;

            Vector3 dir = targetPos - eyePos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return false;
            dir.Normalize();

            if (Vector3.Angle(transform.forward, dir) > angle * 0.5f) return false;

            float dist = Vector3.Distance(eyePos, targetPos);
            if (Physics.Raycast(eyePos, dir, dist, obstructionMask, triggerInteraction)) return false;

            return true;
        }

        private TargetTraits GetTraits(Collider col)
        {
            if (_traitCache.TryGetValue(col, out var cached)) return cached;

            var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
            TargetTraits traits = TargetTraits.None;

            if (go.TryGetComponent(out TargetTrait tt)) traits |= tt.traits;
            if (HasComponentInParents<ICombatable>(go)) traits |= TargetTraits.Combatable;
            if (HasComponentInParents<IInteractable>(go)) traits |= TargetTraits.Interactable;

            _traitCache[col] = traits;
            return traits;
        }

        private bool HasComponentInParents<T>(GameObject go)
        {
            return go.GetComponentInParent(typeof(T)) != null;
        }

        private void SortByDistance(List<Transform> list)
        {
            list.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                float da = _distCache.TryGetValue(a, out var va) ? va : float.MaxValue;
                float db = _distCache.TryGetValue(b, out var vb) ? vb : float.MaxValue;
                return da.CompareTo(db);
            });
        }

        private void TrimList(List<Transform> list, int maxCount)
        {
            if (maxCount <= 0) { list.Clear(); return; }
            if (list.Count > maxCount) list.RemoveRange(maxCount, list.Count - maxCount);
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
                foreach (var t in both) if (t) Handles.DrawLine(eyePos, t.position);

                Handles.color = Color.red;
                foreach (var t in combatables) if (t && !both.Contains(t)) Handles.DrawLine(eyePos, t.position);

                Handles.color = Color.cyan;
                foreach (var t in interactables) if (t && !both.Contains(t)) Handles.DrawLine(eyePos, t.position);
            }
        }
#endif
    }
