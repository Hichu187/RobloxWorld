using Hichu;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class FactoryPrefab : ScriptableObjectSingleton<FactoryPrefab>
    {
        [SerializeField] private GameObject _gameInit;
        [SerializeField] private GameObject _uiNotificationText;

        [SerializeField] private AssetReferenceGameObject _popupEvoNow;
        [SerializeField] private AssetReferenceGameObject _popupBreakLock;
        public static GameObject gameInit => instance._gameInit;
        public static GameObject uiNotificationText => instance._uiNotificationText;
        public static AssetReferenceGameObject popupEvoNow => instance._popupEvoNow;
        public static AssetReferenceGameObject popupBreakLock => instance._popupBreakLock;
    }
}
