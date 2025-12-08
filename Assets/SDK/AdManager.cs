using System;
using UnityEngine;

namespace Easypapa
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [Header("MAX SDK")]
        [SerializeField] private string maxSdkKey;

        [Header("Ad Unit Ids - Android")]
        [SerializeField] private string androidBannerAdUnitId;
        [SerializeField] private string androidInterstitialAdUnitId;
        [SerializeField] private string androidRewardedAdUnitId;

        [Header("Ad Unit Ids - iOS")]
        [SerializeField] private string iosBannerAdUnitId;
        [SerializeField] private string iosInterstitialAdUnitId;
        [SerializeField] private string iosRewardedAdUnitId;

        [Header("Banner Settings")]
        [SerializeField] private MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;

        [SerializeField] private bool logDebug = true;

        public bool IsSdkInitialized { get; private set; }

        private string BannerUnitId =>
#if UNITY_ANDROID
            androidBannerAdUnitId;
#elif UNITY_IOS
            iosBannerAdUnitId;
#else
            string.Empty;
#endif

        private string InterstitialUnitId =>
#if UNITY_ANDROID
            androidInterstitialAdUnitId;
#elif UNITY_IOS
            iosInterstitialAdUnitId;
#else
            string.Empty;
#endif

        private string RewardedUnitId =>
#if UNITY_ANDROID
            androidRewardedAdUnitId;
#elif UNITY_IOS
            iosRewardedAdUnitId;
#else
            string.Empty;
#endif

        private int _interstitialRetryAttempt;
        private int _rewardedRetryAttempt;

        private Action<bool> _onRewardedComplete;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID || UNITY_IOS
            InitializeMaxSdk();
#endif
        }

        private void InitializeMaxSdk()
        {
            if (string.IsNullOrEmpty(maxSdkKey))
            {
                Debug.LogError("[AdManager] MAX SDK key is empty!");
                return;
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
            {
                IsSdkInitialized = true;

                if (logDebug)
                    Debug.Log("[AdManager] MAX SDK initialized.");

                InitializeBanner();
                InitializeInterstitial();
                InitializeRewarded();
            };

            MaxSdk.InitializeSdk();
        }

        #region Banner

        private void InitializeBanner()
        {
            if (string.IsNullOrEmpty(BannerUnitId)) return;

            MaxSdk.CreateBanner(BannerUnitId, bannerPosition);
            MaxSdk.SetBannerBackgroundColor(BannerUnitId, Color.clear);
            MaxSdk.HideBanner(BannerUnitId);
        }

        public void ShowBanner()
        {
            if (!IsSdkInitialized || string.IsNullOrEmpty(BannerUnitId)) return;
            MaxSdk.ShowBanner(BannerUnitId);
        }

        public void HideBanner()
        {
            if (!IsSdkInitialized || string.IsNullOrEmpty(BannerUnitId)) return;
            MaxSdk.HideBanner(BannerUnitId);
        }

        #endregion

        #region Interstitial

        private void InitializeInterstitial()
        {
            if (string.IsNullOrEmpty(InterstitialUnitId)) return;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedLoad;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedDisplay;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHidden;

            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            if (!IsSdkInitialized || string.IsNullOrEmpty(InterstitialUnitId)) return;

            if (logDebug)
                Debug.Log("[AdManager] Load interstitial");

            MaxSdk.LoadInterstitial(InterstitialUnitId);
        }

        public bool IsInterstitialReady =>
            IsSdkInitialized &&
            !string.IsNullOrEmpty(InterstitialUnitId) &&
            MaxSdk.IsInterstitialReady(InterstitialUnitId);

        public void ShowInterstitial(string placement = null)
        {
            if (!IsInterstitialReady) return;

            if (string.IsNullOrEmpty(placement))
                MaxSdk.ShowInterstitial(InterstitialUnitId);
            else
                MaxSdk.ShowInterstitial(InterstitialUnitId, placement);
        }

        private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialRetryAttempt = 0;

            if (logDebug)
                Debug.Log("[AdManager] Interstitial loaded");
        }

        private void OnInterstitialFailedLoad(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _interstitialRetryAttempt));

            if (logDebug)
                Debug.LogWarning($"[AdManager] Interstitial failed load: {errorInfo.Message}, retry in {retryDelay}s");

            Invoke(nameof(LoadInterstitial), (float)retryDelay);
        }

        private void OnInterstitialFailedDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (logDebug)
                Debug.LogWarning($"[AdManager] Interstitial failed display: {errorInfo.Message}");

            LoadInterstitial();
        }

        private void OnInterstitialHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (logDebug)
                Debug.Log("[AdManager] Interstitial hidden, preload next");

            LoadInterstitial();
        }

        #endregion

        #region Rewarded

        private void InitializeRewarded()
        {
            if (string.IsNullOrEmpty(RewardedUnitId)) return;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedFailedLoad;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedFailedDisplay;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedReceivedReward;

            LoadRewarded();
        }

        private void LoadRewarded()
        {
            if (!IsSdkInitialized || string.IsNullOrEmpty(RewardedUnitId)) return;

            if (logDebug)
                Debug.Log("[AdManager] Load rewarded");

            MaxSdk.LoadRewardedAd(RewardedUnitId);
        }

        public bool IsRewardedReady =>
            IsSdkInitialized &&
            !string.IsNullOrEmpty(RewardedUnitId) &&
            MaxSdk.IsRewardedAdReady(RewardedUnitId);

        public void ShowRewarded(string placement, Action<bool> onComplete)
        {
            if (!IsRewardedReady)
            {
                onComplete?.Invoke(false);
                return;
            }

            _onRewardedComplete = onComplete;

            if (string.IsNullOrEmpty(placement))
                MaxSdk.ShowRewardedAd(RewardedUnitId);
            else
                MaxSdk.ShowRewardedAd(RewardedUnitId, placement);
        }

        private void OnRewardedLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedRetryAttempt = 0;

            if (logDebug)
                Debug.Log("[AdManager] Rewarded loaded");
        }

        private void OnRewardedFailedLoad(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempt));

            if (logDebug)
                Debug.LogWarning($"[AdManager] Rewarded failed load: {errorInfo.Message}, retry in {retryDelay}s");

            Invoke(nameof(LoadRewarded), (float)retryDelay);
        }

        private void OnRewardedFailedDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (logDebug)
                Debug.LogWarning($"[AdManager] Rewarded failed display: {errorInfo.Message}");

            _onRewardedComplete?.Invoke(false);
            _onRewardedComplete = null;

            LoadRewarded();
        }

        private void OnRewardedHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (logDebug)
                Debug.Log("[AdManager] Rewarded hidden, preload next");

            LoadRewarded();
        }

        private void OnRewardedReceivedReward(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            _onRewardedComplete?.Invoke(true);
            _onRewardedComplete = null;
        }

        #endregion
    }
}
