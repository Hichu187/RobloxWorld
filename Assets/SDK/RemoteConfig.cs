using Firebase.RemoteConfig;
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

        public const string KEY_UP_APP_VERSION = "upAppVersion";
        public const string KEY_ADS_CONFIG_STR = "adsConfigStr";
        public const string KEY_LOG_ENABLE = "logEnable";
        public const string KEY_BLOCK_ADS_STR = "blockAdsStr";
        public const string KEY_ADS_TIME = "adsTime";
        public const string KEY_MODE_SORT = "modeSort";

        public int upAppVersion = -1;

        public string adsConfigStr;
        public bool logEnable = true;

        public string blockAdsStr = "test1,test2";

        public string adsTimeStr = "";
        public string modeSort = "";

        [JsonIgnore] private HashSet<string> setBlockAds = new HashSet<string>();
        [JsonIgnore] private AdsTimeConfig adsTime = new AdsTimeConfig();
        [JsonIgnore] private List<string> modeSortList = new List<string>();

        public static void Init() { }

        public void ApplyFromFirebase(FirebaseRemoteConfig rc)
        {
            if (rc == null) return;

            upAppVersion = GetInt(rc, KEY_UP_APP_VERSION, upAppVersion);
            adsConfigStr = GetString(rc, KEY_ADS_CONFIG_STR, adsConfigStr);
            logEnable = GetBool(rc, KEY_LOG_ENABLE, logEnable);
            blockAdsStr = GetString(rc, KEY_BLOCK_ADS_STR, blockAdsStr);

            adsTimeStr = GetString(rc, KEY_ADS_TIME, adsTimeStr);
            modeSort = GetString(rc, KEY_MODE_SORT, modeSort);

            DecodeData();
        }

        public void DecodeData()
        {
            DecodeBlockAds();
            DecodeAdsTime();
            DecodeModeSort();
        }

        private void DecodeBlockAds()
        {
            if (!string.IsNullOrEmpty(blockAdsStr))
            {
                try
                {
                    setBlockAds = new HashSet<string>(
                        blockAdsStr
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLower())
                            .Where(x => !string.IsNullOrEmpty(x))
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    setBlockAds = new HashSet<string>();
                }
            }
            else
            {
                setBlockAds = new HashSet<string>();
            }
        }

        private void DecodeAdsTime()
        {
            if (string.IsNullOrEmpty(adsTimeStr))
            {
                adsTime = new AdsTimeConfig();
                adsTime.BuildCache();
                return;
            }

            try
            {
                adsTime = JsonConvert.DeserializeObject<AdsTimeConfig>(adsTimeStr) ?? new AdsTimeConfig();
                adsTime.BuildCache();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                adsTime = new AdsTimeConfig();
                adsTime.BuildCache();
            }
        }

        private void DecodeModeSort()
        {
            if (string.IsNullOrEmpty(modeSort))
            {
                modeSortList = new List<string>();
                return;
            }

            try
            {
                modeSortList = modeSort
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().ToLower())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                modeSortList = new List<string>();
            }
        }

        public bool IsBlockAds(string placement)
        {
            if (string.IsNullOrEmpty(placement)) return false;

            var key = placement.Trim().ToLower();
            if (adsTime != null && adsTime.IsBlocked(key)) return true;
            return setBlockAds != null && setBlockAds.Contains(key);
        }

        public bool IsOpenAds()
        {
            if (adsTime == null) return true;
            return adsTime.openAds;
        }

        public float GetTimeStartAppOpenAds()
        {
            if (adsTime == null) return 0f;
            return adsTime.timeStartShowAds;
        }

        public bool IsBannerAds()
        {
            if (adsTime == null) return true;
            return adsTime.bannerAds;
        }

        public float GetTimeStartShowAds()
        {
            if (adsTime == null) return 0f;
            return adsTime.timeStartShowAds;
        }

        public float GetTimeBetweenShowAds()
        {
            if (adsTime == null) return 60f;
            return adsTime.timeBetweenShowAds;
        }

        public IReadOnlyList<string> GetModeSortList()
        {
            return modeSortList;
        }

        public bool IsUpAppVersion()
        {
            if (upAppVersion <= 0) return false;
            int appVersion = AppUtils.GetAppVersion();
            return appVersion >= upAppVersion;
        }

        private static int GetInt(FirebaseRemoteConfig rc, string key, int fallback)
        {
            try { return (int)rc.GetValue(key).LongValue; }
            catch { return fallback; }
        }

        private static bool GetBool(FirebaseRemoteConfig rc, string key, bool fallback)
        {
            try { return rc.GetValue(key).BooleanValue; }
            catch { return fallback; }
        }

        private static string GetString(FirebaseRemoteConfig rc, string key, string fallback)
        {
            try { return rc.GetValue(key).StringValue ?? fallback; }
            catch { return fallback; }
        }
    }

    [Serializable]
    public class AdsTimeConfig
    {
        public bool openAds = true;
        public bool bannerAds = true;
        public string blockAds = "";
        public float timeStartShowAds = 0f;
        public float timeBetweenShowAds = 30f;

        [JsonIgnore] private HashSet<string> _blockSet = new HashSet<string>();

        public void BuildCache()
        {
            try
            {
                _blockSet = new HashSet<string>();

                if (string.IsNullOrEmpty(blockAds))
                    return;

                var parts = blockAds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    var k = parts[i].Trim().ToLower();
                    if (!string.IsNullOrEmpty(k))
                        _blockSet.Add(k);
                }
            }
            catch
            {
                _blockSet = new HashSet<string>();
            }
        }

        public bool IsBlocked(string placement)
        {
            if (string.IsNullOrEmpty(placement)) return false;
            if (_blockSet == null) return false;
            return _blockSet.Contains(placement.Trim().ToLower());
        }
    }
}
