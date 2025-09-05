using Hichu;
using UnityEngine;

namespace Game
{
    public class DataSettings : LDataBlock<DataSettings>
    {
        [SerializeField] LValue<float> _soundVolume;
        [SerializeField] LValue<float> _musicVolume;
        [SerializeField] LValue<bool> _vibration;

        public static LValue<float> soundVolume { get { return instance._soundVolume; } }
        public static LValue<float> musicVolume { get { return instance._musicVolume; } }
        public static LValue<bool> vibration { get { return instance._vibration; } }

        protected override void Init()
        {
            base.Init();

            _soundVolume = _soundVolume ?? new LValue<float>(1.0f);
            _musicVolume = _musicVolume ?? new LValue<float>(1.0f);
            _vibration = _vibration ?? new LValue<bool>(true);
        }
    }
}
