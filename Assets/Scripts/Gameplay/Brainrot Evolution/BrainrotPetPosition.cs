using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class BrainrotPetPosition : MonoBehaviour
    {
        [Title("Target")]
        public Transform target;
        public Vector3 followOffset = new Vector3(-0.5f, 0f, -0.5f);
        [Min(0f)] public float maxTeleportDistance = 20f;

        [Title("Mode")]
        public MoveMode moveMode = MoveMode.Fly;

        [TitleGroup("Fly"), ShowIf("@moveMode == MoveMode.Fly")]
        [SerializeField] private float flySpeed = 6f;
        [TitleGroup("Fly"), ShowIf("@moveMode == MoveMode.Fly")]
        [SerializeField] private float flyRotSpeed = 8f;

        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField] private float runSpeed = 4.5f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField] private float runRotSpeed = 10f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField] private LayerMask groundMask = ~0;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField] private float groundRayLength = 3f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField] private float groundHeightOffset = 0.05f;

        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopPower = 0.75f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopBaseDuration = 0.35f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopCooldown = 0.12f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopMinDistance = 0.75f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private LayerMask hopGroundMask = ~0;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopRayLength = 3f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField] private float hopHeightOffset = 0.05f;

        [Title("De-overlap")]
        [Min(0f)] public float randomRadius = 0.6f;
        [Min(0f)] public float wanderAmplitude = 0.25f;
        [Min(0f)] public float wanderFrequency = 0.8f;
        public bool rotateTowardMove = true;

        private Tween _hopTween;
        private float _nextHopTime;
        private Vector3 _instanceOffsetXZ;
        private int _seedX, _seedZ;

        void Awake()
        {
            var r = Random.insideUnitCircle * randomRadius;
            _instanceOffsetXZ = new Vector3(r.x, 0f, r.y);
            int baseSeed = GetInstanceID();
            _seedX = baseSeed * 73856093;
            _seedZ = baseSeed * 19349663;
        }

        void OnDisable()
        {
            _hopTween?.Kill();
            _hopTween = null;
        }

        void LateUpdate()
        {
            if (!target) return;

            float t = Time.time * wanderFrequency;
            float nx = Mathf.PerlinNoise(_seedX * 0.0001f, t) * 2f - 1f;
            float nz = Mathf.PerlinNoise(_seedZ * 0.0001f, t + 123.456f) * 2f - 1f;
            Vector3 wander = new Vector3(nx, 0f, nz) * wanderAmplitude;

            Vector3 desired = target.position + target.rotation * (followOffset + _instanceOffsetXZ + wander);

            bool tooFar;
            if (moveMode == MoveMode.Hop)
            {
                float distXZ = Vector3.Distance(transform.position.OnlyXZVec3(), desired.OnlyXZVec3());
                tooFar = distXZ > maxTeleportDistance;
            }
            else
            {
                tooFar = Vector3.Distance(transform.position, desired) > maxTeleportDistance;
            }

            if (tooFar)
            {
                Vector3 pos = desired;
                if (moveMode != MoveMode.Fly)
                    pos = SnapToGround(pos,
                        moveMode == MoveMode.Run ? groundMask : hopGroundMask,
                        moveMode == MoveMode.Run ? groundRayLength : hopRayLength,
                        moveMode == MoveMode.Run ? groundHeightOffset : hopHeightOffset);

                transform.position = pos;

                if (rotateTowardMove)
                    transform.rotation = Quaternion.LookRotation((target.position - transform.position).OnlyXZ().normalized.ToVector3XZ(), Vector3.up);
                return;
            }

            switch (moveMode)
            {
                case MoveMode.Fly: TickFly(desired); break;
                case MoveMode.Run: TickRun(desired); break;
                case MoveMode.Hop: TickHop(desired); break;
            }
        }

        void TickFly(Vector3 desired)
        {
            Vector3 prev = transform.position;
            transform.position = Vector3.Lerp(prev, desired, flySpeed * Time.deltaTime);

            if (!rotateTowardMove) return;
            Vector3 dirXZ = (desired - prev).OnlyXZ().normalized.ToVector3XZ();
            if (dirXZ.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirXZ, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, flyRotSpeed * Time.deltaTime);
            }
        }

        void TickRun(Vector3 desired)
        {
            Vector3 current = transform.position;
            Vector3 goalXZ = new Vector3(desired.x, current.y, desired.z);

            Vector3 dir = (goalXZ - current);
            Vector3 dirXZ = dir.OnlyXZ().normalized.ToVector3XZ();
            Vector3 step = dirXZ * runSpeed * Time.deltaTime;

            if (dir.magnitude > step.magnitude) current += step;
            else current = goalXZ;

            current = SnapToGround(current, groundMask, groundRayLength, groundHeightOffset);
            transform.position = current;

            if (rotateTowardMove && dirXZ.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dirXZ, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, runRotSpeed * Time.deltaTime);
            }
        }

        void TickHop(Vector3 desired)
        {
            Vector3 desiredGround = SnapToGround(desired, hopGroundMask, hopRayLength, hopHeightOffset);

            float distXZ = Vector3.Distance(transform.position.OnlyXZVec3(), desiredGround.OnlyXZVec3());
            float verticalDelta = Mathf.Abs(desired.y - transform.position.y);

            if (_hopTween == null || !_hopTween.IsActive())
            {
                transform.position = SnapToGround(transform.position, hopGroundMask, hopRayLength, hopHeightOffset);

                bool forceByHeight = verticalDelta > 0.35f;
                bool farEnoughXZ = distXZ >= hopMinDistance;

                if (Time.time >= _nextHopTime && (farEnoughXZ || forceByHeight))
                {
                    float d = Mathf.Clamp(
                        hopBaseDuration * Mathf.Lerp(0.85f, 1.4f, Mathf.InverseLerp(hopMinDistance, 5f, Mathf.Max(distXZ, 0.05f))),
                        0.18f, 0.6f);

                    _hopTween = transform.DOJump(desiredGround, hopPower, 1, d)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            _hopTween = null;
                            _nextHopTime = Time.time + hopCooldown;
                            transform.position = SnapToGround(transform.position, hopGroundMask, hopRayLength, hopHeightOffset);
                        });

                    if (rotateTowardMove)
                    {
                        Vector3 dirXZ = (desiredGround - transform.position).OnlyXZ().normalized.ToVector3XZ();
                        if (dirXZ.sqrMagnitude > 0.0001f)
                            transform.rotation = Quaternion.LookRotation(dirXZ, Vector3.up);
                    }
                    return;
                }

                if (rotateTowardMove)
                {
                    Vector3 dirXZ = (desiredGround - transform.position).OnlyXZ().normalized.ToVector3XZ();
                    if (dirXZ.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirXZ, Vector3.up), 8f * Time.deltaTime);
                }
            }
            else
            {
                if (rotateTowardMove)
                {
                    Vector3 dirXZ = (desiredGround - transform.position).OnlyXZ().normalized.ToVector3XZ();
                    if (dirXZ.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirXZ, Vector3.up), 8f * Time.deltaTime);
                }
            }
        }

        Vector3 SnapToGround(Vector3 pos, LayerMask mask, float rayLen, float heightOffset)
        {
            Vector3 origin = pos + Vector3.up * rayLen * 0.5f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLen, mask, QueryTriggerInteraction.Ignore))
                return new Vector3(pos.x, hit.point.y + heightOffset, pos.z);
            return pos;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!target) return;
            Gizmos.color = Color.cyan;
            Vector3 desired = target.position + target.rotation * (followOffset + _instanceOffsetXZ);
            Gizmos.DrawWireSphere(desired, 0.08f);
            Gizmos.DrawLine(transform.position, desired);
        }
#endif
    }

    static class VecUtil
    {
        public static Vector2 OnlyXZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector3 ToVector3XZ(this Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);
        public static Vector3 OnlyXZVec3(this Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
