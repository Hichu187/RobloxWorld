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
        public int exp;
        public int damage = 0;
        public int health;

    }
}

