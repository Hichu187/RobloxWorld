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
        [SerializeField] private List<PetRateByEgg> _petRate = new List<PetRateByEgg>();
        [Title("DATA BY MAP")]
        [SerializeField] private List<MapData> _mapDatas;


        public static List<BrainrotEvoConfig> brainrotConfigs => instance._brainrotConfigs;
        public static List<GameObject> maps => instance._maps;
        public static List<BrainrotEvoPetConfig> pets => instance._pets;

        public static List<MapData> mapDatas => instance._mapDatas;
        public static List<PetRateByEgg> petRate => instance._petRate;

        [System.Serializable]
        public class MapData
        {
            public int price = 100;
            public List<BrainrotEvoPetConfig> petMap = new List<BrainrotEvoPetConfig>();
        }
        [System.Serializable]
        public class PetRateByEgg
        {
            public List<int> rate;
        }
    }
}
