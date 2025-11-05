using Hichu;
using UnityEngine;

namespace Game
{
    public class DataAchievement : LDataBlock<DataAchievement>
    {
        [SerializeField] private int _easyObbyCheckpoint = 0;

        public static int easyObbyCheckpoint { get { return instance._easyObbyCheckpoint; } set { instance._easyObbyCheckpoint = value; } }

        public static void SetEasyObbyCheckpoint(int checkpoint)
        {
            if (checkpoint < easyObbyCheckpoint) return;

            easyObbyCheckpoint = checkpoint;

            Save();
        }
    }
}
