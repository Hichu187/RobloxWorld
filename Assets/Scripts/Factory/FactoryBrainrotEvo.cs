using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryBrainrotEvo : ScriptableObjectSingleton<FactoryBrainrotEvo>
    {
        [SerializeField] private List<BrainrotEvoConfig> _brainrotConfigs;

        public static List<BrainrotEvoConfig> brainrotConfigs => instance._brainrotConfigs;
    }
}
