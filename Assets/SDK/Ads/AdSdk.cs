using System;
using UnityEngine;

namespace Easypapa
{
    public static class AdSdk
    {
        static IAdManager s_adsManager;

        static float s_lastTimeShowInter = 0;
        static float s_isRemoveAds = false ? 1 : 0; // giữ để không phát sinh warning (bạn có thể bỏ)
        static bool s_removeAds = false;

        public static void InitAds()
        {
            s_lastTimeShowInter = 0;

            CreateProvider();

            var appOpen = GameObject.Find("AppOpen") ?? new GameObject("AppOpen");
            if (appOpen.GetComponent<AppOpenAdManager>() == null)
                appOpen.AddComponent<AppOpenAdManager>();
        }

        private static void CreateProvider()
        {
            TryDisposeProvider();

            s_adsManager = null;

            if (AdConfig.CONFIG.provider == AdsProviderMode.Max)
            {
#if USE_MAX
                s_adsManager = new AdMax();
#else
                Debug.LogWarning("[AdSdk] Provider=MAX but USE_MAX is not defined / MAX SDK not available.");
#endif
                if (s_adsManager == null && AdConfig.CONFIG.fallbackToOtherProviderIfInitFailed)
                {
#if USE_ADMOB
                    s_adsManager = new AdAdmob();
#endif
                }
            }
            else
            {
#if USE_ADMOB
                s_adsManager = new AdAdmob();
#else
                Debug.LogWarning("[AdSdk] Provider=AdMob but USE_ADMOB is not defined / AdMob SDK not available.");
#endif
                if (s_adsManager == null && AdConfig.CONFIG.fallbackToOtherProviderIfInitFailed)
                {
#if USE_MAX
                    s_adsManager = new AdMax();
#endif
                }
            }

            Debug.Log($"[AdSdk] Provider selected: {(s_adsManager != null ? s_adsManager.GetType().Name : "NULL")}");
        }

        private static void TryDisposeProvider()
        {
            if (s_adsManager is IDisposable d)
            {
                try { d.Dispose(); } catch { }
            }
        }

        public static void SetRemoveAds(bool removeAds)
        {
            s_removeAds = removeAds;
        }

        public static void ShowAppOpen(Action actionComplete = null)
        {
            if (s_removeAds) { actionComplete?.Invoke(); return; }
            if (AdConfig.CONFIG.freeAds) { actionComplete?.Invoke(); return; }

            if (AppUtils.CurrentTimeSeconds() - EasypapaAdSdk.timeFirstOpen < RemoteConfig.CONFIG.GetTimeStartAppOpenAds())
            {
                actionComplete?.Invoke();
                return;
            }

            if (RemoteConfig.CONFIG.IsUpAppVersion() || !RemoteConfig.CONFIG.IsOpenAds())
            {
                actionComplete?.Invoke();
                return;
            }

            if (s_lastTimeShowInter > 15 && Time.time - s_lastTimeShowInter < 30)
            {
                actionComplete?.Invoke();
                return;
            }

            s_lastTimeShowInter = Time.time;

            s_adsManager?.ShowAppOpen(() =>
            {
                actionComplete?.Invoke();
                s_lastTimeShowInter = Time.time;
            });
        }

        public static void ShowRewarded(Action<bool> onReward, string placement, params object[] parameters)
        {
            if (AdConfig.CONFIG.freeAds)
            {
                onReward?.Invoke(true);
                return;
            }

            if (s_adsManager != null && s_adsManager.IsRewardedReady())
            {
                s_lastTimeShowInter = Time.time;

                s_adsManager.ShowRewarded(success =>
                {
                    onReward?.Invoke(success);
                    s_lastTimeShowInter = Time.time;
                });
            }
            else
            {
                onReward?.Invoke(false);
            }
        }

        public static bool IsRewardedReady() => s_adsManager != null && s_adsManager.IsRewardedReady();

        public static bool IsInterstitialReady() => s_adsManager != null && s_adsManager.IsInterstitialReady();

        public static bool ShowInterstitial(Action onClosed, string placement, params object[] parameters)
        {
            if (AdConfig.CONFIG.freeAds) { onClosed?.Invoke(); return true; }
            if (s_removeAds) { onClosed?.Invoke(); return false; }
            if (RemoteConfig.CONFIG.IsBlockAds(placement)) { onClosed?.Invoke(); return false; }
            if (RemoteConfig.CONFIG.IsUpAppVersion()) { onClosed?.Invoke(); return false; }

            if (AppUtils.CurrentTimeSeconds() - EasypapaAdSdk.timeFirstOpen < RemoteConfig.CONFIG.GetTimeStartShowAds())
            {
                onClosed?.Invoke();
                return false;
            }

            if (Time.time - s_lastTimeShowInter < RemoteConfig.CONFIG.GetTimeBetweenShowAds())
            {
                onClosed?.Invoke();
                return false;
            }

            if (s_adsManager == null || !s_adsManager.IsInterstitialReady())
            {
                onClosed?.Invoke();
                return false;
            }

            s_lastTimeShowInter = Time.time;

            s_adsManager.ShowInterstitial(() =>
            {
                onClosed?.Invoke();
                s_lastTimeShowInter = Time.time;
            });

            return true;
        }

        public static bool ShowInterstitial(string placement, params object[] parameters)
        {
            return ShowInterstitial(null, placement, parameters);
        }

        public static void ShowBanner()
        {
            if (s_removeAds) return;
            if (AdConfig.CONFIG.freeAds) return;
            if (RemoteConfig.CONFIG.IsUpAppVersion()) return;
            if (!RemoteConfig.CONFIG.IsBannerAds()) return;

            s_adsManager?.ShowBanner();
        }

        public static void HideBanner()
        {
            s_adsManager?.HideBanner();
        }

        public static void ReloadBanner()
        {
            s_adsManager?.ReloadBanner();
        }

#if USE_MAX
        public static bool ShowMREC(string placement, AdsViewPosition position = AdsViewPosition.BottomCenter)
        {
            if (AdConfig.CONFIG.freeAds) return false;
            if (s_removeAds) return false;

            s_adsManager?.ShowMREC(position);
            return true;
        }

        public static void HideMREC()
        {
            s_adsManager?.HideMREC();
        }
#endif
    }
}
