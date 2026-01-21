#if USE_ADMOB
using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace Easypapa
{
    public sealed class AdAdmob : IAdManager
    {
        public bool IsInitialized { get; private set; }

        private BannerView _banner;
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private AppOpenAd _appOpen;

        private Action _onInterstitialClosed;
        private Action<bool> _onRewarded;

        public AdAdmob()
        {
            IsInitialized = false;
            Initialize();
        }

        private void Initialize()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobAppId))
            {
                Debug.LogWarning("[AdAdmob] AdMob App ID is empty. Please set AdConfig.admobAppId/admobAppIdIos.");
            }

            MobileAds.Initialize(status =>
            {
                IsInitialized = true;
                Debug.Log("[AdAdmob] Initialized");

                LoadInterstitial();
                LoadRewarded();
                LoadAppOpen();
                CreateBanner(); // create first then show/hide via API
            });
        }

        #region Banner

        private void CreateBanner()
        {
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobBanner)) return;

            DestroyBanner();

            _banner = new BannerView(AdConfig.CONFIG.admobBanner, AdSize.Banner, AdPosition.Bottom);
            _banner.LoadAd(new AdRequest());
        }

        private void DestroyBanner()
        {
            try { _banner?.Destroy(); } catch { }
            _banner = null;
        }

        public void ShowBanner()
        {
            if (!IsInitialized) return;
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobBanner)) return;

            if (_banner == null) CreateBanner();
        }

        public void HideBanner()
        {
            DestroyBanner();
        }

        public void ReloadBanner()
        {
            if (!IsInitialized) return;
            CreateBanner();
        }

        #endregion

        #region Interstitial

        public bool IsInterstitialReady()
        {
            return _interstitial != null;
        }

        private void LoadInterstitial()
        {
            if (!IsInitialized) return;
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobInterstitial)) return;

            InterstitialAd.Load(AdConfig.CONFIG.admobInterstitial, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdAdmob] Interstitial load failed: {error}");
                    _interstitial = null;
                    return;
                }

                _interstitial = ad;
                _interstitial.OnAdFullScreenContentClosed += () =>
                {
                    _onInterstitialClosed?.Invoke();
                    _onInterstitialClosed = null;

                    _interstitial?.Destroy();
                    _interstitial = null;
                    LoadInterstitial();
                };

                _interstitial.OnAdFullScreenContentFailed += e =>
                {
                    Debug.LogWarning($"[AdAdmob] Interstitial show failed: {e}");
                    _onInterstitialClosed?.Invoke();
                    _onInterstitialClosed = null;

                    _interstitial?.Destroy();
                    _interstitial = null;
                    LoadInterstitial();
                };
            });
        }

        public void ShowInterstitial(Action onClosed)
        {
            _onInterstitialClosed = onClosed;

            if (!IsInterstitialReady())
            {
                onClosed?.Invoke();
                LoadInterstitial();
                return;
            }

            _interstitial.Show();
        }

        #endregion

        #region Rewarded

        public bool IsRewardedReady()
        {
            return _rewarded != null;
        }

        private void LoadRewarded()
        {
            if (!IsInitialized) return;
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobRewarded)) return;

            RewardedAd.Load(AdConfig.CONFIG.admobRewarded, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdAdmob] Rewarded load failed: {error}");
                    _rewarded = null;
                    return;
                }

                _rewarded = ad;
                _rewarded.OnAdFullScreenContentClosed += () =>
                {
                    _rewarded?.Destroy();
                    _rewarded = null;
                    LoadRewarded();
                };

                _rewarded.OnAdFullScreenContentFailed += e =>
                {
                    Debug.LogWarning($"[AdAdmob] Rewarded show failed: {e}");
                    _onRewarded?.Invoke(false);
                    _onRewarded = null;

                    _rewarded?.Destroy();
                    _rewarded = null;
                    LoadRewarded();
                };
            });
        }

        public void ShowRewarded(Action<bool> onRewarded)
        {
            _onRewarded = onRewarded;

            if (!IsRewardedReady())
            {
                onRewarded?.Invoke(false);
                LoadRewarded();
                return;
            }

            _rewarded.Show(reward =>
            {
                _onRewarded?.Invoke(true);
                _onRewarded = null;
            });
        }

        #endregion

        #region App Open

        private void LoadAppOpen()
        {
            if (!IsInitialized) return;
            if (string.IsNullOrEmpty(AdConfig.CONFIG.admobAppOpen)) return;

            var request = new AdRequest();

            AppOpenAd.Load(AdConfig.CONFIG.admobAppOpen, request, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdAdmob] AppOpen load failed: {error}");
                    _appOpen = null;
                    return;
                }

                _appOpen = ad;
                _appOpen.OnAdFullScreenContentClosed += () =>
                {
                    _appOpen?.Destroy();
                    _appOpen = null;
                    LoadAppOpen();
                };

                _appOpen.OnAdFullScreenContentFailed += e =>
                {
                    Debug.LogWarning($"[AdAdmob] AppOpen show failed: {e}");
                    _appOpen?.Destroy();
                    _appOpen = null;
                    LoadAppOpen();
                };
            });
        }

        public void ShowAppOpen(Action onClosed)
        {
            if (_appOpen == null)
            {
                onClosed?.Invoke();
                LoadAppOpen();
                return;
            }

            _appOpen.OnAdFullScreenContentClosed += () => onClosed?.Invoke();
            _appOpen.Show();
        }

        #endregion

        public void Dispose()
        {
            try { _interstitial?.Destroy(); } catch { }
            try { _rewarded?.Destroy(); } catch { }
            try { _appOpen?.Destroy(); } catch { }
            DestroyBanner();
        }

#if USE_ADMOB || MAX_USE_ADMOB_COLLAP
        public void ShowBannerCollapsible() { }
        public void HideBannerCollapsible() { }
        public void ReloadBannerCollapsible() { }
#endif

#if USE_ADMOB || MAX_USE_ADMOB_NATIVE
        public void ShowNative(AdSize size, AdPosition position) { }
        public bool IsNativeReady() => false;
        public void HideNative() { }
#endif

#if USE_MAX
        public void ShowMREC(AdsViewPosition position = AdsViewPosition.BottomCenter) { }
        public void HideMREC() { }
#endif
    }
}
#endif
