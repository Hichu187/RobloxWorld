using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using UnityEngine;

namespace Easypapa
{
    public class EasypapaSDK : MonoBehaviour
    {
        public static EasypapaSDK Instance { get; private set; }

        [SerializeField] private bool initFirebase = true;
        [SerializeField] private bool initRemoteConfig = true;

        public bool FirebaseReady { get; private set; }
        public bool RemoteConfigReady { get; private set; }

        private async void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            FirebaseReady = false;
            RemoteConfigReady = false;

            if (initFirebase)
                FirebaseReady = await InitFirebaseAsync();

            if (initRemoteConfig && FirebaseReady)
                RemoteConfigReady = await FetchAndApplyRemoteConfigAsync();

            Debug.Log($"[EasypapaSDK] Init done. FirebaseReady={FirebaseReady}, RemoteConfigReady={RemoteConfigReady}");

            if (RemoteConfigReady)
            {
                Debug.Log($"[EasypapaSDK] modeSort(raw) = {RemoteConfig.CONFIG.modeSort}");
                Debug.Log($"[EasypapaSDK] modeSort(list) = {string.Join(",", RemoteConfig.CONFIG.GetModeSortList())}");
            }
        }

        private static async Task<bool> InitFirebaseAsync()
        {
            try
            {
                var task = FirebaseApp.CheckAndFixDependenciesAsync();
                await task;

                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError("[Firebase] Dependency not available: " + task.Result);
                    return false;
                }

                _ = FirebaseApp.DefaultInstance;

                Debug.Log("[Firebase] Initialized");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static async Task<bool> FetchAndApplyRemoteConfigAsync()
        {
            try
            {
                var rc = FirebaseRemoteConfig.DefaultInstance;

                await rc.FetchAsync(TimeSpan.Zero);
                await rc.ActivateAsync();

                RemoteConfig.CONFIG.ApplyFromFirebase(rc);

                Debug.Log("[RemoteConfig] Fetch+Activate success");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}
