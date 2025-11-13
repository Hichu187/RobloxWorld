using Hichu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class FactorySkin : ScriptableObjectSingleton<FactorySkin>
    {
        [SerializeField] private List<AssetReferenceGameObject> _skin;

        public static List<AssetReferenceGameObject> skin => instance._skin;
    }
}
