using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DataItem : LDataBlock<DataItem>
    {
        [SerializeField] private Dictionary<string, ItemData> _datas;

        [SerializeField] private string _current;
        [SerializeField] private int _currentIndex;

        public static string current { get { return instance._current; } set { instance._current = value; } }
        public static int currentIndex { get { return instance._currentIndex; } set { instance._currentIndex = value; } }

        public static ItemData Get(string key)
        {
            if (!instance._datas.ContainsKey(key))
                instance._datas.Add(key, new ItemData());

            return instance._datas[key];
        }

        protected override void Init()
        {
            base.Init();

            _datas = _datas ?? new Dictionary<string, ItemData>();

            if (string.IsNullOrEmpty(_current))
            {
                _currentIndex = 0;
                _current = FactoryItem.items[_currentIndex].itemName;
                FactoryItem.items[_currentIndex].data.Unlock();
            }
        }

        public static void SetCurrentItem(string itemName)
        {
            if (Get(itemName).isUnlocked)
            {
                current = itemName;
            }

            Save();
        }
    }
}
