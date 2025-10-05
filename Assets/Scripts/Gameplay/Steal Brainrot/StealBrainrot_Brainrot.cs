using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Brainrot : MonoBehaviour
    {
        [Title("Reference")]
        public StealBrainrot_BrainrotInfor petInfo;
        public Animator animator;

        [Title("Data")]
        public StealBrainrot_BrainrotConfig bConfig;
        [SerializeField] private PetRank rank;
        public int earn;
        public int cost;

        public int indBase = -1;

        [Title("Config")]
        public Transform startPoint;
        public Transform endPoint;
        public float moveSpeed = 3f;
        public float detectRadius = 3f;

        [Title("VFX")]
        public Material outline;

        [Title("Runtime")]
        public StealBrainrot_Slot targetSlot;
        public Transform target;
        public bool isBought = false;
        public bool canMove = true;
        public bool isMovingHome = false;

        private void Start()
        {
            if (canMove && startPoint && endPoint)
            {
                transform.position = startPoint.position;
                target = endPoint;
            }
        }

        private void Update()
        {
            if (canMove && target != null) MoveToTarget(target);
        }

        // ===== API dành cho Spawner/Pool =====

        public void SetPosition(Transform startP, Transform endP)
        {
            startPoint = startP;
            endPoint = endP;
        }

        public void Setup(PetRank rank, bool isRun)
        {
            SetSkinBrainrot(rank, isRun);
        }

        public void SpawnFromPool(Transform startP, Transform endP, PetRank rank, bool isRun)
        {
            // Reset trạng thái khi lấy từ pool
            transform.DOKill();
            targetSlot = null;
            isBought = false;
            canMove = true;
            isMovingHome = false;

            SetPosition(startP, endP);
            Setup(rank, isRun);

            if (startPoint) transform.position = startPoint.position;
            target = endPoint;
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(true);
        }

        public void DespawnToPool()
        {
            // Reset tối thiểu trước khi trả về pool
            transform.DOKill();
            target = null;
            targetSlot = null;
            canMove = false;
            isMovingHome = false;

            // Trả về pool
            StealBrainrotPool.instance.Release(this);
        }

        // ===== Internal =====

        private void SetSkinBrainrot(PetRank rank, bool isRun)
        {
            this.rank = rank;
            // TODO: áp skin/anim theo rank/isRun nếu cần
        }

        private void MoveToTarget(Transform target)
        {
            if (target == null) return;

            Vector3 to = target.position - transform.position;
            to.y = 0f;
            Vector3 dir = to.normalized;

            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.0001f)
            {
                var look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 10f);
            }

            if (to.magnitude < 0.1f)
            {
                if (!isBought)
                {
                    // CHƯA MUA -> trả về pool
                    DespawnToPool();
                    return;
                }

                // ĐÃ MUA -> cập bến slot
                if (targetSlot != null)
                {
                    targetSlot.isEmpty = false;
                    isMovingHome = false;

                    var pos = targetSlot.transform.position;
                    pos.y += 1f;
                    transform.position = pos;

                    var dirT = (targetSlot.buttonCollect.transform.position - targetSlot.stayPosition.position).normalized;
                    dirT.y = 0;
                    transform.rotation = Quaternion.LookRotation(dirT);

                    canMove = false;
                    if (indBase == 0) targetSlot.StartGenerating();
                }

                target = null;
            }
        }

        private void BuyPet()
        {
            Debug.Log($"Bought {bConfig.brainrotName}");
            isBought = true;
        }
    }
}
