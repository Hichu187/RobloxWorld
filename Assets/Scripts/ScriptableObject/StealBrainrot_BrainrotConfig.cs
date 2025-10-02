using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_BrainrotConfig : ScriptableObject
    {
        public int ID;

        public string brainrotName;

        public PetRank rank;

        public int earningPerSecond;

        public int costToBuy;

        [PreviewField(100, ObjectFieldAlignment.Left)]
        public Sprite texture;

        public GameObject prefab;

        public bool reward;
    }
}
