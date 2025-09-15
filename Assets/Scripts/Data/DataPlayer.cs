using Hichu;
using UnityEngine;

namespace Game
{
    public class DataPlayer : LDataBlock<DataPlayer>
    {
        [SerializeField] private int _cash = 0;
        [SerializeField] private int _gem = 0;

        public static int cash { get { return instance._cash; } set { instance._cash = value; } }
        public static int gem { get { return instance._gem; } set { instance._gem = value; } }

        public void AddCash(int cash)
        {
            _cash += cash;

            Save();
        }
    }
}
