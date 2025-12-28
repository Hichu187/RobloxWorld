using DG.Tweening;
//using GoogleMobileAds.Ump.Api;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Easypapa
{
    public class AdMax : IAdManager
    {
        int _interstitialRetryAttempt;
        int _rewardedRetryAttempt;

        bool _isRewarded = false;
        bool _isBannerLoaded = false;

        Tween _tweenInterstitial;
        Tween _tweenRewardedVideo;

        event Action _eventInterstitialClosed;
        event Action<bool> _eventRewarded;
        event Action _eventBannerLoaded;
        event Action _eventAppOpenClosed;

#if MAX_USE_ADMOB_NATIVE
        AdsMaxUseAdmobNative adsMaxUseAdmobNative = new AdsMaxUseAdmobNative();
#endif
#if MAX_USE_ADMOB_COLLAP
        AdsMaxUseAdmobCollap adsMaxUseAdmobCollap = new AdsMaxUseAdmobCollap();
#endif

        public AdMax()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                Log("Initialized");

                InitializeInterstitial();
                InitializeRewarded();
                InitializeBanner();
                InitializeAppOpen();
                InitializeMRec();
#if MAX_USE_ADMOB_NATIVE
                InititalzeAdmobNative();
#endif
#if MAX_USE_ADMOB_COLLAP
                InititalzeAdmobCollap();
#endif

                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += MaxSDK_OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += MaxSDK_OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += MaxSDK_OnAdRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += MaxSDK_OnAdRevenuePaidEvent;
                MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += MaxSDK_OnAdRevenuePaidEvent;

#if DEVELOPMENT_BUILD
                MaxSdk.ShowMediationDebugger();
#endif
            };

            MaxSdk.InitializeSdk();
        }

        private void MaxSDK_OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double revenue = adInfo.Revenue;

            var impressionParameters = new[] {
            new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
            new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
            new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
            new Firebase.Analytics.Parameter("ad_format", adInfo.Placement),
            new Firebase.Analytics.Parameter("value", revenue),
            new Firebase.Analytics.Parameter("currency", "USD"),
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_max", impressionParameters);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void Log(object msg)
        {
            UnityEngine.Debug.Log($"AdsMAX: {msg}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(object msg)
        {
            UnityEngine.Debug.LogWarning($"AdsMAX: {msg}");
        }

        #region AppOpen

        private void InitializeAppOpen()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxAppOpen))
            {
                LogWarning("App Open ID is empty");
                return;
            }

            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += AppOpen_OnAdHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;

            LoadAppOpen();
        }

        private void LoadAppOpen()
        {
            MaxSdk.LoadAppOpenAd(AdConfig.CONFIG.maxAppOpen);
        }

        private void AppOpen_OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Log($"App Open failed to display {errorInfo}");
        }

        private void AppOpen_OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log($"App Open loaded!");

            //BounceAppOpenManager.INSTANCE?.CheckShowAppOpenFirst();
        }

        private void AppOpen_OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log($"App Open closed!");

            _eventAppOpenClosed?.Invoke();

            LoadAppOpen();
        }

        private void ShowAppOpen(Action onClosed)
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxAppOpen))
            {
                onClosed?.Invoke();
                return;
            }

            if (MaxSdk.IsAppOpenAdReady(AdConfig.CONFIG.maxAppOpen))
            {
                _eventAppOpenClosed = onClosed;

                MaxSdk.ShowAppOpenAd(AdConfig.CONFIG.maxAppOpen);
            }
            else
            {
                LoadAppOpen();
            }
        }

        #endregion

        #region Interstitial

        private void InitializeInterstitial()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxInterstitial))
            {
                LogWarning("Interstitial ID is empty");
                return;
            }

            // Attach callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += Interstitial_OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += Interstitial_OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += Interstitial_OnAdDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += Interstitial_OnAdHidden;

            // Load the first interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            Log("Interstitial is loading...");

            MaxSdk.LoadInterstitial(AdConfig.CONFIG.maxInterstitial);
        }

        private void Interstitial_OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Log("Interstitial is loaded");

            // Reset retry attempt
            _interstitialRetryAttempt = 0;
        }

        private void Interstitial_OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            _interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _interstitialRetryAttempt));

            Log("Interstital load failed with error code " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...");

            _tweenInterstitial?.Kill();
            _tweenInterstitial = DOVirtual.DelayedCall((float)retryDelay, LoadInterstitial);
        }

        private void Interstitial_OnAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Log("Interstitial failed to display with error code " + errorInfo.Code);

            LoadInterstitial();
        }

        private void Interstitial_OnAdHidden(string adUnitId, MaxSdkBase.AdInfo info)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            Log("Interstitial dismissed");

            _eventInterstitialClosed?.Invoke();

            LoadInterstitial();
        }

        #endregion

        #region Rewarded

        private void InitializeRewarded()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxRewarded))
            {
                LogWarning("Rewarded ID is empty");

                return;
            }

            // Attach callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += Rewarded_OnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += Rewarded_OnAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += Rewarded_OnAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += Rewarded_OnAdDisplayed;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += Rewarded_OnAdClicked;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += Rewarded_OnAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += Rewarded_OnAdReceivedReward;

            // Load the first RewardedAd
            LoadRewarded();
        }

        private void LoadRewarded()
        {
            Log("Rewarded video is loading...");

            MaxSdk.LoadRewardedAd(AdConfig.CONFIG.maxRewarded);
        }

        private void Rewarded_OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
            Log("Rewarded ad is loaded");

            // Reset retry attempt
            _rewardedRetryAttempt = 0;
        }

        private void Rewarded_OnAdLoadFailed(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            _rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempt));

            Log("Rewarded ad load failed with error code " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...");

            _tweenRewardedVideo?.Kill();
            _tweenRewardedVideo = DOVirtual.DelayedCall((float)retryDelay, LoadRewarded);
        }

        private void Rewarded_OnAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            Log("Rewarded ad failed to display with error code: " + errorInfo.Code);

            LoadRewarded();
        }

        private void Rewarded_OnAdDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log("Rewarded ad displayed");
        }

        private void Rewarded_OnAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log("Rewarded ad clicked");
        }

        private void Rewarded_OnAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            Log("Rewarded ad dismissed");

            _eventRewarded?.Invoke(_isRewarded);

            LoadRewarded();
        }

        private void Rewarded_OnAdReceivedReward(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad was displayed and user should receive the reward
            Log("Rewarded ad received reward");

            _isRewarded = true;
        }

        #endregion

        #region Banner

        private void InitializeBanner()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxBanner))
            {
                LogWarning("Banner ID is empty");
                return;
            }

            // Attach Callbacks
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += Banner_OnAdLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += Banner_OnAdLoadFailed;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += Banner_OnAdClicked;

            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
            MaxSdk.CreateBanner(AdConfig.CONFIG.maxBanner, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional.
            MaxSdk.SetBannerBackgroundColor(AdConfig.CONFIG.maxBanner, Color.clear);

            MaxSdk.HideBanner(AdConfig.CONFIG.maxBanner);
        }

        private void Banner_OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Banner ad is ready to be shown.
            // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
            Log("Banner is loaded");

            _isBannerLoaded = true;

            _eventBannerLoaded?.Invoke();
        }

        private void Banner_OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Banner ad failed to load. MAX will automatically try loading a new ad internally.
            Log("Banner ad failed to load with error code " + errorInfo.Code);
        }

        private void Banner_OnAdClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log("Banner ad clicked");
        }

        #endregion

        #region MREC Ads

        private void InitializeMRec()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxMREC))
            {
                LogWarning("MREC ID is empty");

                return;
            }

            // MRECs are sized to 300x250 on phones and tablets
            MaxSdk.CreateMRec(AdConfig.CONFIG.maxMREC, MaxSdkBase.AdViewPosition.TopCenter);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += MRec_OnAdLoadedEvent; ;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += MRec_OnAdLoadFailedEvent;

            MaxSdk.HideMRec(AdConfig.CONFIG.maxMREC);
        }

        private void MRec_OnAdLoadFailedEvent(string adUnit, MaxSdkBase.ErrorInfo error)
        {
            Log($"MREC Load Failed - {error.Message}");
        }

        private void MRec_OnAdLoadedEvent(string adUnit, MaxSdkBase.AdInfo ad)
        {
            Log("MREC is Loaded");
        }

        private void ShowMREC(AdsViewPosition position = AdsViewPosition.BottomCenter)
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxMREC))
                return;

            MaxSdk.UpdateMRecPosition(AdConfig.CONFIG.maxMREC, (MaxSdkBase.AdViewPosition)position);
            MaxSdk.ShowMRec(AdConfig.CONFIG.maxMREC);
            MaxSdk.StartMRecAutoRefresh(AdConfig.CONFIG.maxMREC);
        }

        private void HideMREC()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.maxMREC))
                return;

            MaxSdk.HideMRec(AdConfig.CONFIG.maxMREC);
            MaxSdk.StopMRecAutoRefresh(AdConfig.CONFIG.maxMREC);
        }

        #endregion

        #region AdmobNative
#if MAX_USE_ADMOB_NATIVE
        private void InititalzeAdmobNative()
        {
            Log("InititalzeAdmobNative");
            adsMaxUseAdmobNative = new AdsMaxUseAdmobNative();
            adsMaxUseAdmobNative.Init();
        }
#endif
        #endregion
        #region AdmobCollap
#if MAX_USE_ADMOB_COLLAP
        private void InititalzeAdmobCollap()
        {
            Log("InititalzeAdmobCollap");
            adsMaxUseAdmobCollap = new AdsMaxUseAdmobCollap();
            adsMaxUseAdmobCollap.Init();
        }
#endif
        #endregion

        #region IAdManager

        void IAdManager.ShowInterstitial(Action onClosed)
        {
            _eventInterstitialClosed = onClosed;

            MaxSdk.ShowInterstitial(AdConfig.CONFIG.maxInterstitial);
        }

        void IAdManager.ShowRewarded(Action<bool> onRewarded)
        {
            _isRewarded = false;
            _eventRewarded = onRewarded;

            MaxSdk.ShowRewardedAd(AdConfig.CONFIG.maxRewarded);
        }

        void IAdManager.ShowBanner()
        {
            MaxSdk.ShowBanner(AdConfig.CONFIG.maxBanner);
        }

        void IAdManager.HideBanner()
        {
            MaxSdk.HideBanner(AdConfig.CONFIG.maxBanner);
        }

        bool IAdManager.IsRewardedReady()
        {
            return MaxSdk.IsRewardedAdReady(AdConfig.CONFIG.maxRewarded);
        }

        bool IAdManager.IsInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(AdConfig.CONFIG.maxInterstitial);
        }

        void IAdManager.ReloadBanner()
        {

        }

        // If got error interface not found, defind symbol "MAX" in IAdManager.cs
        void IAdManager.ShowMREC(AdsViewPosition position)
        {
            ShowMREC(position);
        }

        void IAdManager.HideMREC()
        {
            HideMREC();
        }

        void IAdManager.ShowAppOpen(Action onClosed)
        {
            ShowAppOpen(onClosed);
        }
#if MAX_USE_ADMOB_NATIVE
        void IAdManager.ShowNative(AdSize size, AdPosition position)
        {
            adsMaxUseAdmobNative.ShowNative(size, position);
        }

        void IAdManager.HideNative()
        {
            adsMaxUseAdmobNative.HideNative();
        }

        bool IAdsManager.IsNativeReady()
        {
            return adsMaxUseAdmobNative.IsNativeReady();
        }

#endif
#if MAX_USE_ADMOB_COLLAP
        public void ShowBannerCollapsible()
        {
            adsMaxUseAdmobCollap?.ShowBannerCollapsible();
        }

        public void HideBannerCollapsible()
        {
            adsMaxUseAdmobCollap?.HideBannerCollapsible();
        }

        public void ReloadBannerCollapsible()
        {
            adsMaxUseAdmobCollap?.LoadBannerCollapsible();
        }
#endif
        #endregion
    }
}
