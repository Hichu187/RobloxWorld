#define USE_MAX

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Easypapa
{
    public static class AdSdk
    {
        static IAdManager s_adsManager;

        static float s_lastTimeShowInter = 0;
        static float s_lastTimeShowBannerCollapsible = 0;
        static bool s_isRemoveAds = false;

        public static void InitAds()
        {
            s_lastTimeShowInter = 0;

            s_adsManager = new AdMax();

            var appOpen = new GameObject("AppOpen");
            appOpen.AddComponent<AppOpenAdManager>();
        }
        public static void SetRemoveAds(bool removeAds)
        {
            s_isRemoveAds = removeAds;
        }

        public static void ShowAppOpen(Action actionComplete = null)
        {
            if (s_isRemoveAds)
            {
                actionComplete?.Invoke();
                return;
            }
            if (AppUtils.CurrentTimeSeconds() - EasypapaAdSdk.timeFirstOpen < RemoteConfig.CONFIG.GetTimeStartAppOpenAds())
            {
                actionComplete?.Invoke();
                return;
            }

            if (RemoteConfig.CONFIG.IsUpAppVersion())
            {
                actionComplete?.Invoke();
                return;
            }

            if (!RemoteConfig.CONFIG.IsOpenAds())
            {
                actionComplete?.Invoke();
                return;
            }

            if (AdConfig.CONFIG.freeAds)
            {
                actionComplete?.Invoke();
                return;
            }

            if (s_lastTimeShowInter > 15 && Time.time - s_lastTimeShowInter < 30)
            {
                Debug.Log("AppOpen not show: Time diff to last interstitial show time is not enough " + (Time.time - s_lastTimeShowInter) + "s");
                actionComplete?.Invoke();
                return;
            }
            s_lastTimeShowInter = Time.time;

            EasypapaAdSdk.LogAds(GameLoggerAdsType.OPEN_ADS, GameLoggerAdsState.SHOW, "AppOpen");
            s_adsManager?.ShowAppOpen(() =>
            {
                EasypapaAdSdk.LogAds(GameLoggerAdsType.OPEN_ADS, GameLoggerAdsState.COMPLETE, "AppOpen");
                actionComplete?.Invoke();
                s_lastTimeShowInter = Time.time;
            });
        }

        public static void ShowRewarded(Action<bool> onReward, string placement, params object[] parameters)
        {
            if (AdConfig.CONFIG.freeAds)
            {
                onReward(true);
                return;
            }

            if (s_adsManager != null && s_adsManager.IsRewardedReady())
            {
                EasypapaAdSdk.LogAds(GameLoggerAdsType.REWARDED, GameLoggerAdsState.SHOW, placement, parameters);
                s_lastTimeShowInter = Time.time;

                Debug.Log("Show Rewarded");

                s_adsManager?.ShowRewarded((success) =>
                {
                    if (success) EasypapaAdSdk.LogAds(GameLoggerAdsType.REWARDED, GameLoggerAdsState.COMPLETE, placement, parameters);
                    else EasypapaAdSdk.LogAds(GameLoggerAdsType.REWARDED, GameLoggerAdsState.FAIL, placement, parameters);
                    onReward?.Invoke(success);
                    s_lastTimeShowInter = Time.time;
                });
            }
            else
            {
                onReward?.Invoke(false);
            }
        }

        public static bool IsRewardedReady()
        {
            if (s_adsManager == null) return false;
            return s_adsManager.IsRewardedReady();
        }

        public static bool CheckShowInterstitialAble(string placement = null)
        {
            if (AdConfig.CONFIG.freeAds) return false;
            if (!IsInterstitialOKToShow(placement)) return false;
            if (s_isRemoveAds) return false;
            if (s_adsManager == null || !s_adsManager.IsInterstitialReady()) return false;
            return true;
        }

        public static bool ShowInterstitial(string placement, params object[] parameters)
        {
            return ShowInterstitial(null, placement, parameters);
        }

        public static bool IsBlockInterstitial(string placement)
        {
            if (RemoteConfig.CONFIG.IsBlockAds(placement))
            {
                return true;
            }
            return false;
        }

        public static bool ShowInterstitial(Action onClosed, string placement, params object[] parameters)
        {
            if (AdConfig.CONFIG.freeAds)
            {
                onClosed?.Invoke();
                return true;
            }

            if (!IsInterstitialOKToShow(placement))
            {
                onClosed?.Invoke();
                return false;
            }

            // Check if interstitial ready
            if (s_adsManager == null || !s_adsManager.IsInterstitialReady())
            {
                onClosed?.Invoke();
                return false;
            }

            EasypapaAdSdk.LogAds(GameLoggerAdsType.INTERSTITIAL, GameLoggerAdsState.SHOW, placement, parameters);
            s_lastTimeShowInter = Time.time;

            Debug.Log("Show Interstitial");
            s_adsManager?.ShowInterstitial(() =>
            {
                onClosed?.Invoke();
                EasypapaAdSdk.LogAds(GameLoggerAdsType.INTERSTITIAL, GameLoggerAdsState.COMPLETE, placement, parameters);
                s_lastTimeShowInter = Time.time;
            });

            return true;
        }

        private static bool IsInterstitialOKToShow(string placement = null)
        {
            // Check if remove ads
            if (s_isRemoveAds)
            {
                return false;
            }

            if (IsBlockInterstitial(placement))
            {
                return false;
            }

            if (RemoteConfig.CONFIG.IsUpAppVersion())
            {
                return false;
            }

            //check time start to show
            if (AppUtils.CurrentTimeSeconds() - EasypapaAdSdk.timeFirstOpen < RemoteConfig.CONFIG.GetTimeStartShowAds())
                return false;

            // Check time between time show inter
            if (Time.time - s_lastTimeShowInter < RemoteConfig.CONFIG.GetTimeBetweenShowAds())
            {
                Debug.Log("Interstitial not show: Time diff to last interstitial show time is not enough " + (Time.time - s_lastTimeShowInter) + "s");
                return false;
            }

            return true;
        }

        private static bool IsBannerOKToShow()
        {
            // Check remove ads
            if (s_isRemoveAds)
                return false;

            if (RemoteConfig.CONFIG.IsUpAppVersion())
            {
                return false;
            }

            if (!RemoteConfig.CONFIG.IsBannerAds())
            {
                return false;
            }

            return true;
        }

        public static bool IsInterstitialReady()
        {
            if (s_adsManager == null) return false;
            return s_adsManager.IsInterstitialReady();
        }

        public static void ShowBanner()
        {
            if (!IsBannerOKToShow())
                return;

            Debug.Log("Show Banner");
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
                    if (AdConfig.CONFIG.freeAds)
                        return false;

                    if (!IsMRECOKToShow())
                        return false;

                    s_adsManager?.ShowMREC(position);

                    return true;
                }

                public static void HideMREC()
                {
                    s_adsManager?.HideMREC();
                }

                public static bool IsMRECOKToShow()
                {
                    // Check remove ads
                    if (s_isRemoveAds)
                        return false;

/*                    if (AppUtils.CurrentTimeSeconds() - EasypapaAdSdk.timeFirstOpen < RemoteConfig.CONFIG.GetTimeStartToShowMREC())
                        return false;*/

                    return true;
                }
#endif
    }
}
