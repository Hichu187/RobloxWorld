using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FactoryAudio : ScriptableObjectSingleton<FactoryAudio>
    {
        [Title("UI - SFX")]
        [SerializeField] private AudioConfig _sfxUIButtonClick;
        [SerializeField] private AudioConfig _Countdown;
        [SerializeField] private AudioConfig _totalReward;
        [SerializeField] private AudioConfig _resultWin;
        [SerializeField] private AudioConfig _resultLose;

        [Title("GAMEPLAY - BOT")]

        [SerializeField] private List<AudioConfig> _botHorn;

        public static AudioConfig sfxUIButtonClick => instance._sfxUIButtonClick;
        public static AudioConfig sfxCountdown => instance._Countdown;
        public static AudioConfig totalReward => instance._totalReward;
        public static AudioConfig resultWin => instance._resultWin;
        public static AudioConfig resultLose => instance._resultLose;
        public static List<AudioConfig> botHorn => instance._botHorn;
    }
}
