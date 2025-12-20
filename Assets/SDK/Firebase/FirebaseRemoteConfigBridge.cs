using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine;

namespace Easypapa
{
    public static class FirebaseRemoteConfigBridge
    {
        public static bool IsFetched { get; private set; }

        public static async Task FetchAndApplyAsync()
        {
            if (!FirebaseInitializer.IsInitialized)
                await FirebaseInitializer.InitializeAsync();

            if (!FirebaseInitializer.IsInitialized)
                return;

            var rc = RemoteConfig.CONFIG;

            var defaults = new Dictionary<string, object>
            {
                { "adsConfigStr", rc.adsConfigStr ?? string.Empty },
                { "blockAdsStr", rc.blockAdsStr ?? "test1,test2" },
                { "upAppVersion", rc.upAppVersion },
                { "logEnable", rc.logEnable }
            };

            await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            await fetchTask;

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success)
            {
                Debug.LogWarning($"[FirebaseRemoteConfigBridge] Fetch failed: {info.LastFetchStatus}");
                return;
            }

            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            try
            {
                rc.adsConfigStr = FirebaseRemoteConfig.DefaultInstance.GetValue("adsConfigStr").StringValue;
                rc.blockAdsStr = FirebaseRemoteConfig.DefaultInstance.GetValue("blockAdsStr").StringValue;
                rc.upAppVersion = (int)FirebaseRemoteConfig.DefaultInstance.GetValue("upAppVersion").LongValue;
                rc.logEnable = FirebaseRemoteConfig.DefaultInstance.GetValue("logEnable").BooleanValue;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            rc.DecodeData();

            IsFetched = true;
            Debug.Log("[FirebaseRemoteConfigBridge] Remote config fetched and applied.");
        }
    }
}
