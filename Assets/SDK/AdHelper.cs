using System;
using UnityEngine;
using static MaxSdkBase;

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

        }

        #region Banner

        public static void ShowBanner()
        {
            AdSdk.ShowBanner();
        }

        public static void HideBanner()
        {
            AdSdk.HideBanner();
        }

        public static void ShowBannerCollapsible()
        {

        }

        #endregion

        #region Interstitial
        public static async void ShowInterstitialBreak()
        {

            //View view = await ViewHelper.PushAsync(FactoryPrefab.adsBreakPopup);

            //await .WaitForSeconds(2f);

            //view.Close();

            ShowInterstitial("inter_break");
        }

        public static void ShowInterstitial(string placement)
        {
            if (AdConfig.CONFIG.freeAds) return;

            AdSdk.ShowInterstitial(placement);
        }

        #endregion

        #region Rewarded

        public static void ShowRewarded(string placement, Action<bool> onReward)
        {
            onReward ??= _ => { };

            if (AdConfig.CONFIG.freeAds)
            {
                onReward(true);
                return;
            }

            bool invoked = false;
            void SafeInvoke(bool rewarded)
            {
                if (invoked) return;
                invoked = true;
                onReward(rewarded);
            }

            try
            {
                if (!AdSdk.IsRewardedReady())
                {
                    //UINotificationText.Push("No ads available at the moment,\ntry again later!");
                    SafeInvoke(false);
                    return;
                }

                AdSdk.ShowRewarded(SafeInvoke, placement?.ToLower());
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                SafeInvoke(false);
            }
            ;
        }

        #endregion

        #region App Open

        public static void ShowAppOpen(string placement = "appopen_default")
        {
            AdSdk.ShowAppOpen();
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
