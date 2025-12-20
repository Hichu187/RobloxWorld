using System;
using UnityEngine;

namespace Easypapa
{
    public static class AdHelper
    {
        public static bool ForceDisableAds { get; set; }

        private static double _lastInterstitialTime;
        private static double _lastBannerCollapsibleTime;
        private static bool _initialized;

        private static RemoteConfig RC => RemoteConfig.CONFIG;
        private static AdManager Manager => AdManager.Instance;

        private static bool AdsEnabled => !ForceDisableAds;

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            _lastInterstitialTime = -1;
            _lastBannerCollapsibleTime = -1;
            _initialized = true;
        }

        #region Banner

        public static void ShowBanner(string placement = "banner_default")
        {
            EnsureInitialized();

            if (!AdsEnabled) return;
            if (Manager == null || !Manager.IsSdkInitialized) return;
            if (RC.IsBlockAds(placement)) return;

            double now = AppUtils.CurrentTimeSeconds();
            float timeStart = RC.GetTimeStartToShowBanner();
            if (now < timeStart) return;

            Manager.ShowBanner();

            if (RC.logEnable)
                Debug.Log($"[AdHelper] Show banner: {placement}");
        }

        public static void HideBanner()
        {
            Manager?.HideBanner();
        }

        public static void ShowBannerCollapsible(string placement = "banner_collapsible")
        {
            EnsureInitialized();

            if (!AdsEnabled) return;
            if (Manager == null || !Manager.IsSdkInitialized) return;
            if (RC.IsBlockAds(placement)) return;

            double now = AppUtils.CurrentTimeSeconds();
            float timeStart = RC.GetTimeStartToShowBanner();
            if (now < timeStart) return;

            float timeBetween = RC.GetTimeBetweenShowBannerCollapsible();

            if (_lastBannerCollapsibleTime > 0)
            {
                double sinceLast = now - _lastBannerCollapsibleTime;
                if (sinceLast < timeBetween) return;
            }

            _lastBannerCollapsibleTime = now;

            Manager.ShowBanner();

            if (RC.logEnable)
                Debug.Log($"[AdHelper] Show collapsible banner: {placement}");
        }

        #endregion

        #region Interstitial

        private static bool CanShowInterstitial(string placement)
        {
            EnsureInitialized();

            if (!AdsEnabled) return false;
            if (Manager == null || !Manager.IsSdkInitialized) return false;
            if (RC.IsBlockAds(placement)) return false;

            double now = AppUtils.CurrentTimeSeconds();
            float timeStart = RC.GetTimeStartToShowInterstitial();
            if (now < timeStart) return false;

            float timeBetween = RC.GetTimeBetweenShowInterstitial();

            if (_lastInterstitialTime > 0)
            {
                double sinceLast = now - _lastInterstitialTime;
                if (sinceLast < timeBetween) return false;
            }

            return true;
        }

        public static void ShowInterstitial(string placement = "interstitial_default")
        {
            if (!CanShowInterstitial(placement)) return;
            if (!Manager.IsInterstitialReady) return;

            _lastInterstitialTime = AppUtils.CurrentTimeSeconds();

            Manager.ShowInterstitial(placement);

            if (RC.logEnable)
                Debug.Log($"[AdHelper] Show interstitial: {placement}");
        }

        #endregion

        #region Rewarded

        public static void ShowRewarded(string placement, Action<bool> onRewarded)
        {
            EnsureInitialized();

            if (!AdsEnabled)
            {
                onRewarded?.Invoke(true);
                return;
            }

            if (Manager == null || !Manager.IsSdkInitialized)
            {
                onRewarded?.Invoke(false);
                return;
            }

            if (RC.IsBlockAds(placement))
            {
                onRewarded?.Invoke(false);
                return;
            }

            if (!Manager.IsRewardedReady)
            {
                onRewarded?.Invoke(false);
                return;
            }

            Manager.ShowRewarded(placement, rewarded =>
            {
                onRewarded?.Invoke(rewarded);

                if (RC.logEnable)
                    Debug.Log($"[AdHelper] Rewarded complete: {placement}, rewarded = {rewarded}");
            });
        }

        #endregion

        #region App Open

        public static void ShowAppOpen(string placement = "appopen_default")
        {
            EnsureInitialized();

            if (!AdsEnabled) return;
            if (Manager == null || !Manager.IsSdkInitialized) return;
            if (RC.IsBlockAds(placement)) return;
            if (!RC.IsShowAppOpenFirst()) return;

            double now = AppUtils.CurrentTimeSeconds();
            float timeStart = RC.GetTimeStartToShowAppOpen();
            if (now < timeStart) return;

            if (!Manager.IsAppOpenReady) return;

            Manager.ShowAppOpen(placement);

            if (RC.logEnable)
                Debug.Log($"[AdHelper] Show app open: {placement}");
        }

        #endregion

        #region Native

        public static void ShowNative(string placement = "native_default")
        {
            EnsureInitialized();

            if (!AdsEnabled) return;
            if (RC.IsBlockAds(placement)) return;

            Debug.LogWarning("[AdHelper] ShowNative được gọi nhưng hiện tại chưa implement SDK cụ thể cho Native. Bạn cần tự gắn logic vào đây theo SDK bạn dùng.");
        }

        public static void HideNative()
        {
            Debug.LogWarning("[AdHelper] HideNative được gọi nhưng hiện tại chưa implement SDK cụ thể cho Native.");
        }

        #endregion
    }
}
