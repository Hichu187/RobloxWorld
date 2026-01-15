using System;
using UnityEngine;

namespace Easypapa
{
    public class EasypapaAdSdk
    {
        public static long timeFirstOpen = 0;
        public static bool isInitialized = false;
        public static int abTestId = -1;

        public static int session = 1;
        public static int countPlay = 0;

        private static float lastTimeLogAds = 0;
        private static float lastTimeLogPlay = 0;

        public static void InitOnStartup()
        {
            Debug.Log("Bounce SDK RuntimeInit");

            timeFirstOpen = long.Parse(PlayerPrefs.GetString("pref_time_first_open", "0"));
            if (timeFirstOpen == 0)
            {
                timeFirstOpen = AppUtils.CurrentTimeSeconds();
                PlayerPrefs.SetString("pref_time_first_open", "" + timeFirstOpen);
            }
            countPlay = PlayerPrefs.GetInt("pref_count_play", 0);
            session = PlayerPrefs.GetInt("pref_session", 0);
            session++;
            PlayerPrefs.SetInt("pref_session", session);
            Debug.Log("Game Start Session " + session);

            AdConfig.Init();
            AdjustManager.Init();
            RemoteConfig.Init();
            FirebaseManager.Init();
            AdSdk.InitAds();

            isInitialized = true;
        }

        public static void LogAds(GameLoggerAdsType adsType, GameLoggerAdsState state, string placement, params object[] keyValues)
        {
            if (!isInitialized) return;
            if (RemoteConfig.CONFIG != null && !RemoteConfig.CONFIG.logEnable) return;
            if (Time.unscaledTime - lastTimeLogAds < 0.3f) return;
            lastTimeLogAds = Time.unscaledTime;
            string adsTypeStr = char.ToUpper(adsType.ToString()[0]) + adsType.ToString().Substring(1);
            string stateStr = char.ToUpper(state.ToString()[0]) + state.ToString().Substring(1);

            if (keyValues == null) keyValues = new object[0];
            object[] keyValues2 = new object[keyValues.Length + 6];
            Array.Copy(keyValues, keyValues2, keyValues.Length);
            keyValues2[keyValues2.Length - 6] = "type";
            keyValues2[keyValues2.Length - 5] = adsTypeStr;
            keyValues2[keyValues2.Length - 4] = "state";
            keyValues2[keyValues2.Length - 3] = stateStr;
            keyValues2[keyValues2.Length - 2] = "placement";
            keyValues2[keyValues2.Length - 1] = placement;

            if (adsType == GameLoggerAdsType.INTERSTITIAL && state == GameLoggerAdsState.SHOW)
            {
                FirebaseManager.Log($"ad_inter_place_{placement}");
            }
            else if (adsType == GameLoggerAdsType.INTERSTITIAL && state == GameLoggerAdsState.COMPLETE)
            {
                FirebaseManager.Log($"ad_inter_complete");
            }
            else if (adsType == GameLoggerAdsType.REWARDED && state == GameLoggerAdsState.SHOW)
            {
                FirebaseManager.Log($"ad_reward_place_{placement}");
            }
            else if (adsType == GameLoggerAdsType.REWARDED && state == GameLoggerAdsState.COMPLETE)
            {
                FirebaseManager.Log($"ad_reward_complete");
            }
        }
        public static void LogEvent(string eventName, params object[] keyValues)
        {
            if (!isInitialized) return;
            if (RemoteConfig.CONFIG != null && !RemoteConfig.CONFIG.logEnable) return;
            object[] keyValues2 = new object[keyValues.Length + 2];
            Array.Copy(keyValues, keyValues2, keyValues.Length);
            keyValues2[keyValues2.Length - 2] = "eventName";
            keyValues2[keyValues2.Length - 1] = eventName;

            FirebaseManager.Log(eventName);
        }
        public static void Log(string logName, params object[] keyValues)
        {
            if (!isInitialized) return;
            if (RemoteConfig.CONFIG != null && !RemoteConfig.CONFIG.logEnable) return;

            //todo log firebase
            FirebaseManager.Log(logName);
        }
    }

    public enum GameLoggerAdsType
    {
        INTERSTITIAL = 0,
        REWARDED = 1,
        REWARDED_INTER = 2,
        OPEN_ADS = 3,
    }

    public enum GameLoggerAdsState
    {
        SHOW = 0,
        COMPLETE = 1,
        FAIL = 2,
    }
}
