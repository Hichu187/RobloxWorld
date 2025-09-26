using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryBrainrotEvo : ScriptableObjectSingleton<FactoryBrainrotEvo>
    {
        [SerializeField] private List<BrainrotEvoConfig> _brainrotConfigs;
        [SerializeField] private List<GameObject> _maps;
        [SerializeField] private List<BrainrotEvoPetConfig> _pets;

        [Title("DATA BY MAP")]
        [SerializeField] private List<MapData> _mapDatas;

        public static List<BrainrotEvoConfig> brainrotConfigs => instance._brainrotConfigs;
        public static List<GameObject> maps => instance._maps;
        public static List<BrainrotEvoPetConfig> pets => instance._pets;

        public static List<MapData> mapDatas => instance._mapDatas;

        [System.Serializable]
        public class MapData
        {
            public List<BrainrotEvoPetConfig> petMap = new List<BrainrotEvoPetConfig>();
        }
    }
}
