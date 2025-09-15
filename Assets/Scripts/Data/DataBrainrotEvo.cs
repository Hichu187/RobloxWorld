using Hichu;
using UnityEngine;

namespace Game
{
    public class DataBrainrotEvo : LDataBlock<DataBrainrotEvo>
    {
        [SerializeField] private int _level = 0;
        [SerializeField] private int _exp = 0;

        public static int level { get { return instance._level; } set { instance._level = value; } }
        public static int exp { get { return instance._exp; } set { instance._exp = value; } }

        public void AddExp(int amount)
        {
            _exp += amount;

            while (CanLevelUp())
            {
                LevelUp();
            }

            Save();
        }

        private bool CanLevelUp()
        {
            return _level < FactoryBrainrotEvo.brainrotConfigs.Count &&
                   _exp >= FactoryBrainrotEvo.brainrotConfigs[_level].exp;
        }

        private void LevelUp()
        {
            int requiredExp = Mathf.RoundToInt(FactoryBrainrotEvo.brainrotConfigs[_level].exp);
            _exp -= requiredExp;
            _level++;

            LDebug.Log($"Player level Up {_level}, exp : {_exp}");
        }
    }
}
