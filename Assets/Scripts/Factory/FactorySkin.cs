using Hichu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class FactorySkin : ScriptableObjectSingleton<FactorySkin>
    {
        [SerializeField] private List<GameObject> _skin;

        public static List<GameObject> skin => instance._skin;
    }
}
