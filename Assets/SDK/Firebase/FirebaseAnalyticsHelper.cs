using Firebase.Analytics;
using UnityEngine;

namespace Easypapa
{
    public static class FirebaseAnalyticsHelper
    {
        public static void LogEvent(string eventName)
        {
            if (!FirebaseInitializer.IsInitialized) return;
            if (string.IsNullOrEmpty(eventName)) return;

            FirebaseAnalytics.LogEvent(eventName);
        }

        public static void LogEvent(string eventName, params Parameter[] parameters)
        {
            if (!FirebaseInitializer.IsInitialized) return;
            if (string.IsNullOrEmpty(eventName)) return;

            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        public static void SetUserId(string userId)
        {
            if (!FirebaseInitializer.IsInitialized) return;
            FirebaseAnalytics.SetUserId(userId);
        }

        public static void SetUserProperty(string name, string value)
        {
            if (!FirebaseInitializer.IsInitialized) return;
            FirebaseAnalytics.SetUserProperty(name, value);
        }
    }
}
