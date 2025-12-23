using System.Collections.Generic;
using UnityEngine;
using Easypapa;

namespace Game
{
    public static class LogHelper
    {
        public static bool LogToUnity = true;

        public static void Event(string eventName)
        {
            if (LogToUnity)
                Debug.Log($"[Evt] {eventName}");

            FirebaseLogger.Log(eventName);
        }

        public static void Event(string eventName, params Firebase.Analytics.Parameter[] parameters)
        {
            if (LogToUnity)
            {
                Debug.Log($"[Evt] {eventName}");
            }

            FirebaseLogger.Log(eventName, parameters);
        }

        public static void Event(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (LogToUnity)
            {
                Debug.Log($"[Evt] {eventName}");
            }

            FirebaseLogger.Log(eventName, parameters);
        }

        public static void Warn(string message)
        {
            if (LogToUnity)
                Debug.LogWarning(message);
        }

        public static void Error(string message)
        {
            if (LogToUnity)
                Debug.LogError(message);
        }
    }
}
