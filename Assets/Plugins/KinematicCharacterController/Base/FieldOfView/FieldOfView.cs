using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kcc.Base
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("FOV")]
        public float radius = 10f;
        [Range(0, 360)] public float angle = 90f;

        [Header("Layers")]
        public LayerMask targetMask;
        public LayerMask obstructionMask;

        [Header("Query Options")]
        [SerializeField] private float eyeHeight = 1.5f;
        [SerializeField] private float targetHeightFallback = 1.0f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public List<Transform> visibleTargets = new List<Transform>();

        public bool seeingTarget = false;
        private void Update()
        {
            FieldOfViewCheck();
        }

        private void FieldOfViewCheck()
        {
            visibleTargets.Clear();

            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            Collider[] rangeChecks = Physics.OverlapSphere(
                eyePos,
                radius,
                targetMask,
                triggerInteraction
            );

            if (rangeChecks.Length > 0)
            {
                foreach (var col in rangeChecks)
                {

                    if (col.attachedRigidbody != null && col.attachedRigidbody.gameObject == gameObject) continue;
                    if (col.gameObject == gameObject) continue;

                    if (IsTargetVisible(col))
                        visibleTargets.Add(col.transform);
                }
            }
        }

        private bool IsTargetVisible(Collider targetCol)
        {
            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            Vector3 targetPos = targetCol.bounds.size.sqrMagnitude > 0.0001f
                ? targetCol.bounds.center
                : targetCol.transform.position + Vector3.up * targetHeightFallback;

            Vector3 dir = (targetPos - eyePos).normalized;

            dir.y = 0; dir.Normalize();

            if (Vector3.Angle(transform.forward, dir) <= angle * 0.5f)
            {
                float dist = Vector3.Distance(eyePos, targetPos);

                if (!Physics.Raycast(
                        eyePos,
                        dir,
                        dist,
                        obstructionMask,
                        triggerInteraction))
                {
                    return true;
                }
            }
            return false;
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

            if (visibleTargets != null && visibleTargets.Count > 0)
            {
                Handles.color = Color.red;
                foreach (var t in visibleTargets)
                {
                    if (t != null)
                    {
                        Handles.DrawLine(eyePos, t.position);
                    }
                }
            }
        }
#endif
    }
}
