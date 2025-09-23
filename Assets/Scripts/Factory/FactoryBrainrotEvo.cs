using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryBrainrotEvo : ScriptableObjectSingleton<FactoryBrainrotEvo>
    {
        [SerializeField] private List<BrainrotEvoConfig> _brainrotConfigs;
        [SerializeField] private List<GameObject> _maps;
        [SerializeField] private List<BrainrotPet> _pets;

        public static List<BrainrotEvoConfig> brainrotConfigs => instance._brainrotConfigs;
        public static List<GameObject> maps => instance._maps;
        public static List<BrainrotPet> pets => instance._pets;
    }
}
