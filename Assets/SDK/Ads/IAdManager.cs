#define USE_MAX

#if USE_ADMOB || MAX_USE_ADMOB_NATIVE
using GoogleMobileAds.Api;
#endif
using System;
using UnityEngine.UI;

namespace Easypapa
{
    public enum AdsViewPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        Centered,
        CenterLeft,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public interface IAdManager
    {
        void ShowInterstitial(Action onClosed);
        bool IsInterstitialReady();
        void ShowRewarded(Action<bool> onRewarded);
        bool IsRewardedReady();
        void ShowAppOpen(Action onClosed);
        void ShowBanner();
        void HideBanner();
        void ReloadBanner();

#if USE_MAX
        void ShowMREC(AdsViewPosition position = AdsViewPosition.BottomCenter);
        void HideMREC();

#endif

#if USE_ADMOB || MAX_USE_ADMOB_COLLAP
        void ShowBannerCollapsible();
        void HideBannerCollapsible();
        void ReloadBannerCollapsible();
#endif

#if USE_ADMOB || MAX_USE_ADMOB_NATIVE
        void ShowNative(AdSize size, AdPosition position);
        bool IsNativeReady();
        void HideNative();
#endif
    }
}
