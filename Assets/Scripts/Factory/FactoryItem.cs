using Hichu;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Game
{
    public class FactoryItem : ScriptableObjectSingleton<FactoryItem>
    {
        [SerializeField] private List<ItemConfig> _items;

        public static List<ItemConfig> items => instance._items;
    }
}
