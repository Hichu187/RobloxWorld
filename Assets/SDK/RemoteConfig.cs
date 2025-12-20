using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Easypapa
{
    [Serializable]
    public class RemoteConfig
    {
        public static RemoteConfig CONFIG = new RemoteConfig();

        #region sdk var

        public int upAppVersion = -1;

        public string adsConfigStr;
        public bool logEnable = true;

        [JsonIgnore]
        private AdsConfig adsConfig = new AdsConfig();

        public string blockAdsStr = "test1,test2";

        [JsonIgnore]
        private HashSet<string> setBlockAds = new HashSet<string>();

        #endregion

        public static void Init() { }

        public void DecodeData()
        {
            if (!string.IsNullOrEmpty(adsConfigStr))
            {
                try
                {
                    adsConfig = JsonConvert.DeserializeObject<AdsConfig>(adsConfigStr) ?? new AdsConfig();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    adsConfig = new AdsConfig();
                }
            }

            if (!string.IsNullOrEmpty(blockAdsStr))
            {
                try
                {
                    setBlockAds = new HashSet<string>(
                        blockAdsStr
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    setBlockAds = new HashSet<string>();
                }
            }
        }

        public bool IsBlockAds(string placement)
        {
            if (string.IsNullOrEmpty(placement)) return false;
            return setBlockAds.Contains(placement);
        }

        #region ADS CONFIG ACCESSORS

        public bool IsShowAppOpenFirst()
        {
            if (adsConfig == null) return false;
            if (IsUpAppVersion()) return false;
            return adsConfig.showAppOpenFirst;
        }

        public float GetTimeStartToShowBanner()
        {
            if (adsConfig == null) return 0f;
            return adsConfig.timeStartToShowBanner;
        }

        public float GetTimeStartToShowMREC()
        {
            if (adsConfig == null) return 0f;
            return adsConfig.timeStartToShowMREC;
        }

        public float GetTimeStartToShowAppOpen()
        {
            if (adsConfig == null) return 0f;
            return adsConfig.timeStartToShowAppOpen;
        }

        public float GetTimeBetweenShowBannerCollapsible()
        {
            if (adsConfig == null) return 30f;
            return adsConfig.timeBetweenShowBannerCollapsible;
        }

        public float GetTimeStartToShowInterstitial()
        {
            if (adsConfig == null) return 60f;
            if (adsConfig.listTimeStartToShowInterstitial == null ||
                adsConfig.listTimeStartToShowInterstitial.Length < 1)
                return 60f;

            return adsConfig.listTimeStartToShowInterstitial[0];
        }

        public float GetTimeBetweenShowInterstitial()
        {
            if (adsConfig == null) return 60f;

            var startList = adsConfig.listTimeStartToShowInterstitial;
            var betweenList = adsConfig.listTimeBetweenShowInterstitial;

            if (startList == null || startList.Length < 1) return 60f;
            if (betweenList == null || betweenList.Length < 1) return 60f;

            double timeInGame = AppUtils.CurrentTimeSeconds();

            for (int i = startList.Length - 1; i >= 0; i--)
            {
                if (timeInGame > startList[i])
                {
                    if (i < betweenList.Length)
                        return betweenList[i];

                    return betweenList[betweenList.Length - 1];
                }
            }

            return 60f;
        }

        public bool IsUpAppVersion()
        {
            if (upAppVersion <= 0) return false;
            int appVersion = AppUtils.GetAppVersion();
            return appVersion >= upAppVersion;
        }

        #endregion
    }

    [Serializable]
    public class AdsConfig
    {
        public float timeStartToShowBanner = 0f;
        public float timeStartToShowMREC = 0f;
        public float timeStartToShowAppOpen = 0f;
        public bool showAppOpenFirst = true;

        public float timeBetweenShowBannerCollapsible = 30f;
        public float[] listTimeBetweenShowInterstitial = { 30f, 22f };
        public float[] listTimeStartToShowInterstitial = { 60f, 300f };
    }
}
