using Hichu;
using UnityEngine;

namespace Game
{
    public class DataPlayer : LDataBlock<DataPlayer>
    {
        [SerializeField] private int _currentMotor = 0;

        [SerializeField] private int _level = 0;
        [SerializeField] private int _experience = 0;

        [SerializeField] private int _cash = 0;
        [SerializeField] private int _gem = 0;

        public static int currentMotor { get { return instance._currentMotor; } set { instance._currentMotor = value; } }
        public static int level { get { return instance._level; } set { instance._level = value; } }
        public static int experience { get { return instance._experience; } set { instance._experience = value; } }
        public static int cash { get { return instance._cash; } set { instance._cash = value; } }
        public static int gem { get { return instance._gem; } set { instance._gem = value; } }

        public void SelectMotor(int motorIndex)
        {
            _currentMotor = motorIndex;

            Save(); 
        }

        public void AddCash(int cash)
        {
            _cash += cash;

            Save();
        }
    }
}
