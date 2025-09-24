using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class FactoryPrefab : ScriptableObjectSingleton<FactoryPrefab>
    {
        [SerializeField] private GameObject _gameInit;
        [SerializeField] private AssetReference _settingView;

        public static GameObject gameInit => instance._gameInit;
        public static AssetReference settingView => instance._settingView;
    }
}
