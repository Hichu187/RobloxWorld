using Hichu;
using UnityEngine;

namespace Game
{
    public class FactoryPrefab : ScriptableObjectSingleton<FactoryPrefab>
    {
        [SerializeField] private GameObject _gameInit;

        public static GameObject gameInit => instance._gameInit;
    }
}
