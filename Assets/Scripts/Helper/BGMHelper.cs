
using Hichu;

namespace Game
{
    public static class BGMHelper
    {
        static AudioScript _asBGM;

        public static void Play(AudioConfig config)
        {
            if (_asBGM != null && _asBGM.audioSource.clip == config.clip)
                return;

            _asBGM?.Stop();

            _asBGM = AudioManager.Play(config, true);
        }

        public static void Stop()
        {
            _asBGM?.Stop();

            _asBGM = null;
        }
    }
}
