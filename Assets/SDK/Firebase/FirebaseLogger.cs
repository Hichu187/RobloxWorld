using System;
using System.Collections.Generic;
using Firebase.Analytics;

namespace Easypapa
{
    public static class FirebaseLogger
    {
        public const int MaxEventNameLength = 40;
        public const int MaxParamNameLength = 40;
        public const int MaxParamStringValueLength = 100;

        public static bool Enabled
        {
            get
            {
                if (!FirebaseInitializer.IsInitialized) return false;
                return RemoteConfig.CONFIG == null || RemoteConfig.CONFIG.logEnable;
            }
        }

        public static void Log(string eventName)
        {
            if (!Enabled) return;
            eventName = NormalizeEventName(eventName);
            if (string.IsNullOrEmpty(eventName)) return;

            FirebaseAnalytics.LogEvent(eventName);
        }

        public static void Log(string eventName, params Parameter[] parameters)
        {
            if (!Enabled) return;

            eventName = NormalizeEventName(eventName);
            if (string.IsNullOrEmpty(eventName)) return;

            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, NormalizeParameters(parameters));
        }

        public static void Log(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (!Enabled) return;

            eventName = NormalizeEventName(eventName);
            if (string.IsNullOrEmpty(eventName)) return;

            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var list = new List<Parameter>(Math.Min(25, parameters.Count));
            foreach (var kv in parameters)
            {
                if (list.Count >= 25) break;

                var key = NormalizeParamName(kv.Key);
                if (string.IsNullOrEmpty(key)) continue;

                if (TryConvertToParameter(key, kv.Value, out var p))
                    list.Add(p);
            }

            if (list.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, list.ToArray());
        }

        public static Parameter P(string key, string value)
            => new Parameter(NormalizeParamName(key), NormalizeParamString(value));

        public static Parameter P(string key, int value)
            => new Parameter(NormalizeParamName(key), value);

        public static Parameter P(string key, long value)
            => new Parameter(NormalizeParamName(key), value);

        public static Parameter P(string key, double value)
            => new Parameter(NormalizeParamName(key), value);

        public static Parameter P(string key, bool value)
            => new Parameter(NormalizeParamName(key), value ? 1 : 0);

        private static Parameter[] NormalizeParameters(Parameter[] parameters)
        {
            var list = new List<Parameter>(Math.Min(25, parameters.Length));
            for (int i = 0; i < parameters.Length && list.Count < 25; i++)
            {
                var p = parameters[i];
                if (p == null) continue;

                // Firebase Parameter type is immutable; we rebuild via ToString() is not possible.
                // So we keep parameter names normalized at creation via FirebaseLogger.P(...)
                // If caller passes raw Parameter, we accept as-is.
                list.Add(p);
            }
            return list.ToArray();
        }

        private static bool TryConvertToParameter(string key, object value, out Parameter p)
        {
            if (value == null)
            {
                p = new Parameter(key, "null");
                return true;
            }

            switch (value)
            {
                case string s:
                    p = new Parameter(key, NormalizeParamString(s));
                    return true;
                case bool b:
                    p = new Parameter(key, b ? 1 : 0);
                    return true;
                case int i:
                    p = new Parameter(key, i);
                    return true;
                case long l:
                    p = new Parameter(key, l);
                    return true;
                case float f:
                    p = new Parameter(key, (double)f);
                    return true;
                case double d:
                    p = new Parameter(key, d);
                    return true;
                default:
                    p = new Parameter(key, NormalizeParamString(value.ToString()));
                    return true;
            }
        }

        private static string NormalizeEventName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            name = name.Trim().ToLowerInvariant().Replace(" ", "_");
            if (name.Length > MaxEventNameLength)
                name = name.Substring(0, MaxEventNameLength);

            return name;
        }

        private static string NormalizeParamName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            name = name.Trim().ToLowerInvariant().Replace(" ", "_");
            if (name.Length > MaxParamNameLength)
                name = name.Substring(0, MaxParamNameLength);

            return name;
        }

        private static string NormalizeParamString(string value)
        {
            if (value == null) return string.Empty;

            value = value.Trim();
            if (value.Length > MaxParamStringValueLength)
                value = value.Substring(0, MaxParamStringValueLength);

            return value;
        }
    }
}
