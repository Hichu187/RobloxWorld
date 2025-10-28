using UnityEngine;

namespace Game
{
    public class MinigameConfig : ScriptableObject
    {
        public string gameTitle;
        public Sprite gameIcon;
        [Range(0,100)]
        public float like;
        public float user;

        public string gameSceneName;
    }
}
