using Hichu;
using UnityEngine;

namespace Game
{
    public class DataAchievement : LDataBlock<DataAchievement>
    {
        [SerializeField] private float _endlessHighScore = 0;

        public static float endlessHighScore { get { return instance._endlessHighScore; } set { instance._endlessHighScore = value; } }

        public void SetHighScore(float highScore)
        {
            if (highScore < _endlessHighScore) return;

            _endlessHighScore = highScore;

            Save();
        }
    }
}
