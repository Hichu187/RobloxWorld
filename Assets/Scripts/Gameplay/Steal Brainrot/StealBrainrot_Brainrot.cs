using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private PetRank rankPet;
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

        void Awake()
        {
            startPoint = StealBrainrot_Manager.instance.startPoint;
            endPoint = StealBrainrot_Manager.instance.targetPoint;
        }
        private void Start()
        {
            if (canMove)
            {
                transform.position = startPoint.position;
                target = endPoint;
            }
        }
        private void Update()
        {
            if (canMove) MoveToTarget(target);
;
        }

        //INIT DATA
        public void Setup(PetRank rank , bool isRun)
        {
            SetSkinBrainrot(rank, isRun);
        }
        private void SetSkinBrainrot(PetRank rank, bool isRun)
        {
            rankPet = rank;
        }

        // MOVING
        private void MoveToTarget(Transform target)
        {
            if (target == null) return;

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (direction.magnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                if (!isBought)
                {
                    this.gameObject.SetActive(false);
                }
                else
                {
                    targetSlot.isEmpty = false;
                    isMovingHome = false;
                    Vector3 pos = targetSlot.transform.position;
                    pos.y += 1f;
                    this.transform.position = pos;

                    Vector3 directionT = (targetSlot.buttonCollect.transform.position - targetSlot.stayPosition.position).normalized;
                    directionT.y = 0;
                    Quaternion lookRotationT = Quaternion.LookRotation(directionT);
                    transform.rotation = lookRotationT;

                    canMove = false;
                    if (indBase == 0) targetSlot.StartGenerating();
                }
                if (!isBought)
                {
                    target = (target == endPoint) ? startPoint : endPoint;
                }
                else
                {
                    target = null;
                }

            }
        }

        private void BuyPet()
        {
            Debug.Log($"Bought {bConfig.brainrotName}");
            isBought = true;
        }
    }
}
