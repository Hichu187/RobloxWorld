using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ObbyCanvas : MonoBehaviour
    {
        public Minigame game;
        public Slider progress;
        public TextMeshProUGUI progressTxt;

        private void Start()
        {
            switch (game)
            {
                case Minigame.EaseObby:
                    EasyObbyGameplay gameplay = FindAnyObjectByType<EasyObbyGameplay>();
                    InitProgress(DataAchievement.easyObbyCheckpoint + 1, gameplay.checkpoints.Count);
                    break;
                case Minigame.MegaObby:
                    MegaObbyGameplay megameplay = FindAnyObjectByType<MegaObbyGameplay>();
                    InitProgress(DataAchievement.megaObbyCheckpoint + 1, megameplay.checkpoints.Count);
                    break;
            }
        }

        public void InitProgress(int prog, int maxCheckpoint)
        {
            float v = (float)prog/(float)maxCheckpoint;
            progress.maxValue = maxCheckpoint;
            progress.value = Mathf.Max(prog, 10);
            progressTxt.text = $"Stage {prog} ({(v*100).ToString("F1")} %)";
        }
    }
}
