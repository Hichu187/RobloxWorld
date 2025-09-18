using UnityEngine;
using Hichu;
using Sirenix.OdinInspector;

namespace Game
{


    public class BrainrotEvoConfig : ScriptableObject
    {
        public string brainrotName;
        public GameObject model;

        [Title("Config")]
        public float exp;
        public float damage = 0;
        public float health;

    }
}

