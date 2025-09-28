using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class FactoryPrefab : ScriptableObjectSingleton<FactoryPrefab>
    {
        [SerializeField] private GameObject _gameInit;
        [SerializeField] private GameObject _uiNotificationText;

        public static GameObject gameInit => instance._gameInit;
        public static GameObject uiNotificationText => instance._uiNotificationText;
    }
}
