#define USE_ADJUST

using AdjustSdk;
using UnityEngine;

namespace Easypapa
{
    public class AdjustManager : MonoBehaviour
    {
        public static void Init()
        {
            Debug.Log("AdjustManager Init");
#if USE_ADJUST
            if (AdConfig.CONFIG == null) return;
            if (string.IsNullOrEmpty(AdConfig.CONFIG.adjustAppToken))
            {
                throw new System.Exception("You must config Adjust in BounceConfig file!");
            }
            AdjustConfig adjustConfig = new AdjustConfig(AdConfig.CONFIG.adjustAppToken, AdjustEnvironment.Production);
            adjustConfig.LogLevel = AdjustLogLevel.Suppress;
            Adjust.InitSdk(adjustConfig);
#endif
        }
        public static void ShowInterstitial()
        {
        }

        public static void ShowRewarded()
        {
        }
        public static void CompleteRewarded()
        {
        }

        public static void LogIAPRevenue(double localizedPrice, string isoCurrencyCode)
        {
        }
    }
}