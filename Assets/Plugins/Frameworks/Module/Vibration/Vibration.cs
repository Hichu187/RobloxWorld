namespace Hichu.Vibration
{
    public static class Vibration
    {
        public static bool Enabled = true;

        public static void Init()
        {
#if UNITY_ANDROID
            VibrationAndroid.Init();
#endif
        }

        public static void Vibrate(VibrationType type)
        {
            if (Enabled)
            {
#if UNITY_IOS
            //VibrationIOS.Vibrate(type);
#elif UNITY_ANDROID
                VibrationAndroid.Vibrate(type);
#endif

            }

        }

        public static bool HasVibrator()
        {
#if UNITY_IOS
            return VibrationIOS.HasVibrator();
#elif UNITY_ANDROID
            return VibrationAndroid.HasVibrator();
#else
            return false;
#endif
        }
    }


}