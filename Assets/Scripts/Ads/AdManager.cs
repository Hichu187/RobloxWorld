using System;
using UnityEngine;
//using GoogleMobileAds;
//using GoogleMobileAds.Api;

namespace Game
{
    public class AdManager : MonoBehaviour
    {
/*        public static AdManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private AdConfig adConfig;
        [SerializeField] private bool useTestIds = true;

        // Test IDs của Google (dùng khi dev)
        private const string TEST_ANDROID_BANNER = "ca-app-pub-3940256099942544/6300978111";
        private const string TEST_IOS_BANNER = "ca-app-pub-3940256099942544/2934735716";

        private const string TEST_ANDROID_INTER = "ca-app-pub-3940256099942544/1033173712";
        private const string TEST_IOS_INTER = "ca-app-pub-3940256099942544/4411468910";

        private const string TEST_ANDROID_REWARDED = "ca-app-pub-3940256099942544/5224354917";
        private const string TEST_IOS_REWARDED = "ca-app-pub-3940256099942544/1712485313";

        private const string TEST_ANDROID_APPOPEN = "ca-app-pub-3940256099942544/9257395921";
        private const string TEST_IOS_APPOPEN = "ca-app-pub-3940256099942544/5575463023";

        private bool _initialized;

        // Banner
        private BannerView _bannerView;

        // Interstitial
        private InterstitialAd _interstitialAd;

        // Rewarded
        private RewardedAd _rewardedAd;
        private Action _onRewarded;

        // App Open
        private AppOpenAd _appOpenAd;
        private DateTime _appOpenExpireTime;

        #region Unity

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAdMob();
        }

        private void OnDestroy()
        {
            DestroyBanner();
            DestroyInterstitial();
            DestroyRewarded();
            DestroyAppOpen();
        }

        #endregion

        #region Init

        private void InitializeAdMob()
        {
            Debug.Log("[AdManager] Initializing Google Mobile Ads...");

            MobileAds.Initialize(status =>
            {
                _initialized = true;
                Debug.Log("[AdManager] Google Mobile Ads initialized.");
            });
        }

        private AdRequest CreateRequest()
        {
            return new AdRequest();
        }

        private bool CheckConfig()
        {
            if (adConfig != null) return true;

            Debug.LogError("[AdManager] AdConfig is NULL – please assign it in Inspector.");
            return false;
        }

        private string GetBannerId()
        {
            if (useTestIds)
            {
#if UNITY_ANDROID
                return TEST_ANDROID_BANNER;
#elif UNITY_IOS
                return TEST_IOS_BANNER;
#else
                return "unused";
#endif
            }

            if (!CheckConfig()) return string.Empty;

#if UNITY_ANDROID
            return adConfig.androidBannerId;
#elif UNITY_IOS
            return adConfig.iosBannerId;
#else
            return "unused";
#endif
        }

        private string GetInterstitialId()
        {
            if (useTestIds)
            {
#if UNITY_ANDROID
                return TEST_ANDROID_INTER;
#elif UNITY_IOS
                return TEST_IOS_INTER;
#else
                return "unused";
#endif
            }

            if (!CheckConfig()) return string.Empty;

#if UNITY_ANDROID
            return adConfig.androidInterstitialId;
#elif UNITY_IOS
            return adConfig.iosInterstitialId;
#else
            return "unused";
#endif
        }

        private string GetRewardedId()
        {
            if (useTestIds)
            {
#if UNITY_ANDROID
                return TEST_ANDROID_REWARDED;
#elif UNITY_IOS
                return TEST_IOS_REWARDED;
#else
                return "unused";
#endif
            }

            if (!CheckConfig()) return string.Empty;

#if UNITY_ANDROID
            return adConfig.androidRewardedId;
#elif UNITY_IOS
            return adConfig.iosRewardedId;
#else
            return "unused";
#endif
        }

        private string GetAppOpenId()
        {
            if (useTestIds)
            {
#if UNITY_ANDROID
                return TEST_ANDROID_APPOPEN;
#elif UNITY_IOS
                return TEST_IOS_APPOPEN;
#else
                return "unused";
#endif
            }

            if (!CheckConfig()) return string.Empty;

#if UNITY_ANDROID
            return adConfig.androidAppOpenId;
#elif UNITY_IOS
            return adConfig.iosAppOpenId;
#else
            return "unused";
#endif
        }

        #endregion

        #region Banner

        public void LoadBanner(AdPosition position = AdPosition.Bottom)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AdManager] LoadBanner called before SDK initialized.");
                return;
            }

            DestroyBanner();

            string adUnitId = GetBannerId();
            if (string.IsNullOrEmpty(adUnitId))
                return;

            Debug.Log($"[AdManager] Loading Banner: {adUnitId}");

            _bannerView = new BannerView(adUnitId, AdSize.Banner, position);

            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("[AdManager] Banner loaded.");
                _bannerView.Show();
            };

            _bannerView.OnBannerAdLoadFailed += error =>
            {
                Debug.LogError($"[AdManager] Banner failed to load: {error}");
            };

            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("[AdManager] Banner clicked.");
            };

            _bannerView.LoadAd(CreateRequest());
        }

        public void ShowBanner()
        {
            _bannerView?.Show();
        }

        public void HideBanner()
        {
            _bannerView?.Hide();
        }

        public void DestroyBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        #endregion

        #region Interstitial

        public void LoadInterstitial()
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AdManager] LoadInterstitial called before SDK initialized.");
                return;
            }

            DestroyInterstitial();

            string adUnitId = GetInterstitialId();
            if (string.IsNullOrEmpty(adUnitId))
                return;

            Debug.Log($"[AdManager] Loading Interstitial: {adUnitId}");

            InterstitialAd.Load(adUnitId, CreateRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] Interstitial failed to load: {error}");
                    return;
                }

                Debug.Log("[AdManager] Interstitial loaded.");
                _interstitialAd = ad;
                RegisterInterstitialEvents(ad);
            });
        }

        private void RegisterInterstitialEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdManager] Interstitial opened.");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] Interstitial closed. Preloading next...");
                DestroyInterstitial();
                LoadInterstitial();
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError($"[AdManager] Interstitial failed to show: {error}");
                DestroyInterstitial();
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("[AdManager] Interstitial clicked.");
            };
        }

        public bool IsInterstitialReady()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public void ShowInterstitial()
        {
            if (IsInterstitialReady())
            {
                _interstitialAd.Show();
            }
            else
            {
                Debug.Log("[AdManager] Interstitial not ready, loading...");
                LoadInterstitial();
            }
        }

        private void DestroyInterstitial()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        #endregion

        #region Rewarded

        public void LoadRewarded()
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AdManager] LoadRewarded called before SDK initialized.");
                return;
            }

            DestroyRewarded();

            string adUnitId = GetRewardedId();
            if (string.IsNullOrEmpty(adUnitId))
                return;

            Debug.Log($"[AdManager] Loading Rewarded: {adUnitId}");

            RewardedAd.Load(adUnitId, CreateRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] Rewarded failed to load: {error}");
                    return;
                }

                Debug.Log("[AdManager] Rewarded loaded.");
                _rewardedAd = ad;
                RegisterRewardedEvents(ad);
            });
        }

        private void RegisterRewardedEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdManager] Rewarded opened.");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] Rewarded closed. Preloading next...");
                DestroyRewarded();
                LoadRewarded();
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError($"[AdManager] Rewarded failed to show: {error}");
                DestroyRewarded();
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("[AdManager] Rewarded clicked.");
            };
        }

        public bool IsRewardedReady()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public void ShowRewarded(Action onRewarded)
        {
            if (!IsRewardedReady())
            {
                Debug.Log("[AdManager] Rewarded not ready, loading...");
                LoadRewarded();
                return;
            }

            _onRewarded = onRewarded;

            _rewardedAd.Show(reward =>
            {
                Debug.Log($"[AdManager] User rewarded: {reward.Amount} {reward.Type}");
                _onRewarded?.Invoke();
                _onRewarded = null;
            });
        }

        private void DestroyRewarded()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }

        #endregion

        #region App Open

        public void LoadAppOpen()
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AdManager] LoadAppOpen called before SDK initialized.");
                return;
            }

            DestroyAppOpen();

            string adUnitId = GetAppOpenId();
            if (string.IsNullOrEmpty(adUnitId))
                return;

            Debug.Log($"[AdManager] Loading AppOpen: {adUnitId}");

            AppOpenAd.Load(adUnitId, CreateRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] AppOpen failed to load: {error}");
                    return;
                }

                Debug.Log("[AdManager] AppOpen loaded.");
                _appOpenAd = ad;
                _appOpenExpireTime = DateTime.Now + TimeSpan.FromHours(4);
                RegisterAppOpenEvents(ad);
            });
        }

        private void RegisterAppOpenEvents(AppOpenAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdManager] AppOpen opened.");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] AppOpen closed. Preloading next...");
                DestroyAppOpen();
                LoadAppOpen();
            };

            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError($"[AdManager] AppOpen failed to show: {error}");
                DestroyAppOpen();
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("[AdManager] AppOpen clicked.");
            };
        }

        public bool IsAppOpenReady()
        {
            return _appOpenAd != null && DateTime.Now < _appOpenExpireTime;
        }

        public void ShowAppOpen()
        {
            if (IsAppOpenReady())
            {
                _appOpenAd.Show();
            }
            else
            {
                Debug.Log("[AdManager] AppOpen not ready, loading...");
                LoadAppOpen();
            }
        }

        private void DestroyAppOpen()
        {
            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }
        }

        #endregion*/
    }
}
