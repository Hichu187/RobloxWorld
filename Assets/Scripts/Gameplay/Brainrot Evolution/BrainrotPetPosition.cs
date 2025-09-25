using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class BrainrotPetPosition : MonoBehaviour
    {
        [Title("Target")]
        public Transform target;
        [Tooltip("Offset tương đối so với target.")]
        public Vector3 followOffset = new Vector3(-0.5f, 0f, -0.5f);
        [Tooltip("Khoảng cách tối đa, vượt quá sẽ teleport về gần target để tránh tụt.")]
        [Min(0f)] public float maxTeleportDistance = 20f;

        [Title("Mode")]
        public MoveMode moveMode = MoveMode.Fly;

        [TitleGroup("Fly"), ShowIf("@moveMode == MoveMode.Fly")]
        [SerializeField, LabelText("Speed")] private float flySpeed = 6f;
        [TitleGroup("Fly"), ShowIf("@moveMode == MoveMode.Fly")]
        [SerializeField, LabelText("Rot Speed")] private float flyRotSpeed = 8f;

        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField, LabelText("Speed")] private float runSpeed = 4.5f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField, LabelText("Rotate Speed")] private float runRotSpeed = 10f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField, LabelText("Ground Mask")] private LayerMask groundMask = ~0;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField, LabelText("Ray Length")] private float groundRayLength = 3f;
        [TitleGroup("Run"), ShowIf("@moveMode == MoveMode.Run")]
        [SerializeField, LabelText("Height Offset")] private float groundHeightOffset = 0.05f;

        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Hop Power (DOTween)")] private float hopPower = 0.75f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Base Duration")] private float hopBaseDuration = 0.35f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Cooldown")] private float hopCooldown = 0.12f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Min Dist To Hop")] private float hopMinDistance = 0.75f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Ground Mask")] private LayerMask hopGroundMask = ~0;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Ray Length")] private float hopRayLength = 3f;
        [TitleGroup("Hop"), ShowIf("@moveMode == MoveMode.Hop")]
        [SerializeField, LabelText("Height Offset")] private float hopHeightOffset = 0.05f;

        private Tween _hopTween;
        private float _nextHopTime;

        void OnDisable()
        {
            _hopTween?.Kill();
            _hopTween = null;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + target.rotation * followOffset;

            // Nếu quá xa thì “teleport” về gần để đuổi kịp
            float dist = Vector3.Distance(transform.position, desired);
            if (dist > maxTeleportDistance)
            {
                Vector3 pos = desired;
                if (moveMode != MoveMode.Fly) // snap xuống đất khi chạy/hop
                    pos = SnapToGround(pos, moveMode == MoveMode.Run ? groundMask : hopGroundMask,
                                       moveMode == MoveMode.Run ? groundRayLength : hopRayLength,
                                       moveMode == MoveMode.Run ? groundHeightOffset : hopHeightOffset);
                transform.position = pos;
                transform.rotation = Quaternion.LookRotation((target.position - transform.position).OnlyXZ().normalized.ToVector3XZ(), Vector3.up);
                return;
            }

            switch (moveMode)
            {
                case MoveMode.Fly:
                    TickFly(desired);
                    break;

                case MoveMode.Run:
                    TickRun(desired);
                    break;

                case MoveMode.Hop:
                    TickHop(desired);
                    break;
            }
        }

        void TickFly(Vector3 desired)
        {
            // Mượt vị trí & xoay theo target
            transform.position = Vector3.Lerp(transform.position, desired, flySpeed * Time.deltaTime);
            Quaternion targetRot = Quaternion.LookRotation((desired - transform.position).OnlyXZ().normalized.ToVector3XZ(), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, flyRotSpeed * Time.deltaTime);
        }

        void TickRun(Vector3 desired)
        {
            // Chạy trên mặt đất (XZ), snap xuống ground mỗi frame
            Vector3 current = transform.position;
            Vector3 goalXZ = new Vector3(desired.x, current.y, desired.z);

            // Hướng di chuyển phẳng
            Vector3 dir = (goalXZ - current);
            Vector3 dirXZ = dir.OnlyXZ().normalized.ToVector3XZ();
            Vector3 step = dirXZ * runSpeed * Time.deltaTime;

            // Nếu còn xa thì tiến tới, nếu gần thì ghim vào goalXZ
            if (dir.magnitude > step.magnitude)
                current += step;
            else
                current = goalXZ;

            // Snap to ground
            current = SnapToGround(current, groundMask, groundRayLength, groundHeightOffset);
            transform.position = current;

            // Xoay theo hướng di chuyển
            if (dirXZ.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dirXZ, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, runRotSpeed * Time.deltaTime);
            }
        }

        void TickHop(Vector3 desired)
        {
            // Hop chỉ kích hoạt khi đủ khoảng cách và qua cooldown
            float distXZ = Vector3.Distance(transform.position.OnlyXZVec3(), desired.OnlyXZVec3());
            if (_hopTween == null || !_hopTween.IsActive())
            {
                if (Time.time >= _nextHopTime && distXZ >= hopMinDistance)
                {
                    Vector3 landing = desired;
                    landing = SnapToGround(landing, hopGroundMask, hopRayLength, hopHeightOffset);

                    // Duration tỷ lệ theo khoảng cách một chút để tự nhiên hơn
                    float d = Mathf.Clamp(hopBaseDuration * Mathf.Lerp(0.9f, 1.5f, Mathf.InverseLerp(hopMinDistance, 5f, distXZ)), 0.2f, 0.6f);

                    _hopTween = transform.DOJump(landing, hopPower, 1, d)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            _hopTween = null;
                            _nextHopTime = Time.time + hopCooldown;
                        });

                    // Xoay mặt theo hướng landing
                    Vector3 dirXZ = (landing - transform.position).OnlyXZ().normalized.ToVector3XZ();
                    if (dirXZ.sqrMagnitude > 0.0001f)
                    {
                        transform.rotation = Quaternion.LookRotation(dirXZ, Vector3.up);
                    }
                }
            }
            else
            {
                // Trong lúc tween, có thể cập nhật nhẹ rotation hướng target
                Vector3 dirXZ = (desired - transform.position).OnlyXZ().normalized.ToVector3XZ();
                if (dirXZ.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirXZ, Vector3.up), 8f * Time.deltaTime);
            }
        }

        Vector3 SnapToGround(Vector3 pos, LayerMask mask, float rayLen, float heightOffset)
        {
            // Ray từ trên xuống để bám mặt đất
            Vector3 origin = pos + Vector3.up * rayLen * 0.5f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLen, mask, QueryTriggerInteraction.Ignore))
            {
                return new Vector3(pos.x, hit.point.y + heightOffset, pos.z);
            }
            return pos; // nếu không có ground thì giữ nguyên (tránh giật)
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.color = Color.cyan;
            Vector3 desired = target.position + (target.rotation * followOffset);
            Gizmos.DrawWireSphere(desired, 0.08f);
            Gizmos.DrawLine(transform.position, desired);
        }
#endif
    }

    // ====== Helper nhỏ cho xử lý XZ ======
    static class VecUtil
    {
        public static Vector2 OnlyXZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector3 ToVector3XZ(this Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);
        public static Vector3 OnlyXZVec3(this Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
