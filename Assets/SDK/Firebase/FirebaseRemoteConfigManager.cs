using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;
using System;

namespace Easypapa
{
    public class FirebaseRemoteConfigManager : MonoBehaviour
    {
        public static FirebaseRemoteConfigManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitializeAsync()
        {
            var dependency = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependency != DependencyStatus.Available)
            {
                Debug.LogError("[RemoteConfig] Firebase dependency missing");
                return;
            }

            await FetchAndApplyAsync();
        }

        public async Task FetchAndApplyAsync()
        {
            // Fetch data
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

            // Activate (await)
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            // Now safe to read
            string adsConfigStr = FirebaseRemoteConfig.DefaultInstance.GetValue("adsConfigStr").StringValue;
            string blockAdsStr = FirebaseRemoteConfig.DefaultInstance.GetValue("blockAdsStr").StringValue;
            int upAppVersion = (int)FirebaseRemoteConfig.DefaultInstance.GetValue("upAppVersion").LongValue;
            bool logEnable = FirebaseRemoteConfig.DefaultInstance.GetValue("logEnable").BooleanValue;

            var rc = RemoteConfig.CONFIG;
            rc.adsConfigStr = adsConfigStr;
            rc.blockAdsStr = blockAdsStr;
            rc.upAppVersion = upAppVersion;
            rc.logEnable = logEnable;

            rc.DecodeData();

            Debug.Log("[RemoteConfig] Applied Firebase config.");
        }

    }
}
