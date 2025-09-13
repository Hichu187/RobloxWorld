using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class AITest : MonoBehaviour
    {
        [SerializeField] AI _ai;

        [Button]
        private void Patrolling(Transform middle)
        {
            _ai.Patrol(middle.position , 10f);
        }
    }
}
