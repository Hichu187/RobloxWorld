using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

namespace Easypapa
{
    public static class FirebaseInitializer
    {
        public static bool IsInitialized { get; private set; }
        public static FirebaseApp App { get; private set; }

        public static async Task InitializeAsync()
        {
            if (IsInitialized) return;

            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[FirebaseInitializer] Could not resolve all Firebase dependencies: {dependencyStatus}");
                return;
            }

            App = FirebaseApp.DefaultInstance;

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            IsInitialized = true;
            Debug.Log("[FirebaseInitializer] Firebase initialized.");
        }
    }
}
