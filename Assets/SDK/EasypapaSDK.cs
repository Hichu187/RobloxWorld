using System.Threading.Tasks;
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
            if (initFirebase)
            {
                await FirebaseInitializer.InitializeAsync();
                FirebaseReady = FirebaseInitializer.IsInitialized;
            }

            if (initRemoteConfig && FirebaseReady)
            {
                await FirebaseRemoteConfigBridge.FetchAndApplyAsync();
                RemoteConfigReady = FirebaseRemoteConfigBridge.IsFetched;
            }

            Debug.Log($"[EasypapaSDK] Init done. FirebaseReady={FirebaseReady}, RemoteConfigReady={RemoteConfigReady}");
        }
    }
}
