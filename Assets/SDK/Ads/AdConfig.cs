using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Easypapa
{
    public enum AdsProviderMode
    {
        Max = 0,
        AdMob = 1
    }

    [CreateAssetMenu(
        fileName = "AdConfig",
        menuName = "Easypapa/Config/Ad Config",
        order = 1)]
    public class AdConfig : ScriptableObject
    {
        private const string RootFolderName = "Easypapa";
        private static AdConfig s_config;

        public static AdConfig CONFIG
        {
            get
            {
                if (s_config == null)
                {
                    s_config = Resources.Load<AdConfig>($"{RootFolderName}/{nameof(AdConfig)}");

#if UNITY_EDITOR
                    if (s_config == null)
                    {
                        string dir = $"Assets/Resources/{RootFolderName}";
                        if (!System.IO.Directory.Exists(dir))
                            System.IO.Directory.CreateDirectory(dir);

                        s_config = CreateInstance<AdConfig>();
                        AssetDatabase.CreateAsset(
                            s_config,
                            $"{dir}/{nameof(AdConfig)}.asset"
                        );
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
#endif
                    Init();
                }

                return s_config;
            }
        }

        public static void Init()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_IPHONE)
            CONFIG.admobAppId = CONFIG.admobAppIdIos;
            CONFIG.admobBanner = CONFIG.admobBannerIos;
            CONFIG.admobInterstitial = CONFIG.admobInterstitialIos;
            CONFIG.admobRewarded = CONFIG.admobRewardedIos;
            CONFIG.admobAppOpen = CONFIG.admobAppOpenIos;

            CONFIG.maxBanner = CONFIG.maxBannerIos;
            CONFIG.maxInterstitial = CONFIG.maxInterstitialIos;
            CONFIG.maxRewarded = CONFIG.maxRewardedIos;
            CONFIG.maxAppOpen = CONFIG.maxAppOpenIos;
            CONFIG.maxMREC = CONFIG.maxMRECIos;
#endif

#if DEVELOPMENT_BUILD
            if (CONFIG.admobTestAdsOnDevelopmentBuild)
            {
                CONFIG.admobBanner = "ca-app-pub-3940256099942544/6300978111";
                CONFIG.admobInterstitial = "ca-app-pub-3940256099942544/1033173712";
                CONFIG.admobRewarded = "ca-app-pub-3940256099942544/5224354917";
                CONFIG.admobAppOpen = "ca-app-pub-3940256099942544/9257395921";
            }
#endif
        }

        [Header("Common")]
        public bool freeAds;

        [Header("Provider")]
        public AdsProviderMode provider = AdsProviderMode.Max;
        public bool fallbackToOtherProviderIfInitFailed = true;

        [Header("AdMob App ID (Required for Google Mobile Ads SDK)")]
        public string admobAppId;       // ca-app-pub-xxxxx~yyyyy (Android by default)
        public string admobAppIdIos;    // iOS App ID

        [Header("Admob AdUnit")]
        public string admobBanner;
        public string admobInterstitial;
        public string admobRewarded;
        public string admobAppOpen;

        public string admobBannerIos;
        public string admobInterstitialIos;
        public string admobRewardedIos;
        public string admobAppOpenIos;

        [Space]
        public bool admobTestAdsOnDevelopmentBuild;

        [Header("AppLovin MAX")]
        public string maxSdkKey;
        public string maxBanner;
        public string maxInterstitial;
        public string maxRewarded;
        public string maxMREC;
        public string maxAppOpen;

        public string maxBannerIos;
        public string maxInterstitialIos;
        public string maxRewardedIos;
        public string maxMRECIos;
        public string maxAppOpenIos;

        [Header("Adjust")]
        public string adjustAppToken = "";
    }
}
