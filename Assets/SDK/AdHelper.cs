using System;
using UnityEngine;

namespace Easypapa
{
    public static class AdHelper
    {
        public static bool ForceDisableAds { get; set; }

        private static bool _initialized;

        private static RemoteConfig RC => RemoteConfig.CONFIG;

        private static bool AdsEnabled => !ForceDisableAds;

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            AdConfig.Init();
            if (!EasypapaAdSdk.isInitialized)
            {
                // Nếu project của bạn luôn gọi EasypapaAdSdk.InitOnStartup() trước thì đoạn này hiếm khi chạy.
            }

            AdSdk.InitAds();
            _initialized = true;
        }

        public static void ShowBanner()
        {
            EnsureInitialized();
            if (!AdsEnabled) return;
            AdSdk.ShowBanner();
        }

        public static void HideBanner()
        {
            EnsureInitialized();
            AdSdk.HideBanner();
        }

        public static void ShowInterstitial(string placement)
        {
            EnsureInitialized();
            if (!AdsEnabled) return;
            if (AdConfig.CONFIG.freeAds) return;

            AdSdk.ShowInterstitial(placement);
        }

        public static void ShowRewarded(string placement, Action<bool> onReward)
        {
            EnsureInitialized();
            onReward ??= _ => { };

            if (!AdsEnabled) { onReward(false); return; }

            if (AdConfig.CONFIG.freeAds)
            {
                onReward(true);
                return;
            }

            if (!AdSdk.IsRewardedReady())
            {
                onReward(false);
                return;
            }

            AdSdk.ShowRewarded(onReward, placement?.ToLower());
        }

        public static void ShowAppOpen(string placement = "appopen_default")
        {
            EnsureInitialized();
            if (!AdsEnabled) return;
            AdSdk.ShowAppOpen();
        }
    }
}
