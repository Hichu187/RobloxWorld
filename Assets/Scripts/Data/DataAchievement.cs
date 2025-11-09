using Hichu;
using UnityEngine;

namespace Game
{
    public class DataAchievement : LDataBlock<DataAchievement>
    {
        [SerializeField] private int _easyObbyCheckpoint = 0;
        [SerializeField] private int _megaObbyCheckpoint = 0;

        public static int easyObbyCheckpoint { get { return instance._easyObbyCheckpoint; } set { instance._easyObbyCheckpoint = value; } }
        public static int megaObbyCheckpoint { get { return instance._megaObbyCheckpoint; } set { instance._megaObbyCheckpoint = value; } }

        public static void SetEasyObbyCheckpoint(int checkpoint)
        {
            if (checkpoint < easyObbyCheckpoint) return;

            easyObbyCheckpoint = checkpoint;

            Save();
        }
        public static void SetMegaObbyCheckpoint(int checkpoint)
        {
            if (checkpoint < megaObbyCheckpoint) return;

            megaObbyCheckpoint = checkpoint;

            Save();
        }
    }
}
