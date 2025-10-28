using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryMinigame : ScriptableObjectSingleton<FactoryMinigame>
    {
        [SerializeField] private List<MinigameConfig> _minigames;

        public static List<MinigameConfig> minigames => instance._minigames;
    }
}
