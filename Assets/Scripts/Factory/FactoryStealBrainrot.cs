using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryStealBrainrot : ScriptableObjectSingleton<FactoryStealBrainrot>
    {
        [SerializeField] private List<StealBrainrot_BrainrotConfig> _brainrotConfigs;

        public static List<StealBrainrot_BrainrotConfig> brainrotConfigs => instance._brainrotConfigs;
    }
}
