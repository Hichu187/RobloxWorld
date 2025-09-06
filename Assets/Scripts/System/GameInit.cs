using Hichu;
using UnityEngine;

namespace Game
{
    public class GameInit : MonoSingleton<GameInit>
    {
        [RuntimeInitializeOnLoadMethod]
        private static void StartupInit()
        {
            //FactoryPrefab.gameInit.Create();
        }

        private void Start()
        {
            InitSettings();
        }

        private void InitSettings()
        {
            AudioManager.volumeMusic.value = DataSettings.musicVolume.value;
            AudioManager.volumeSound.value = DataSettings.soundVolume.value;

            DataSettings.musicVolume.eventValueChanged += (volume) => { AudioManager.volumeMusic.value = volume; };
            DataSettings.soundVolume.eventValueChanged += (volume) => { AudioManager.volumeSound.value = volume; };

#if USE_VIBRATION
            //Taptic.Taptic.tapticOn = DataSettings.vibration.value;

            DataSettings.vibration.eventValueChanged += SettingsVibrationValue_EventValueChanged;
#endif
        }
    }
}
