using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

#if EASYPAPA_FIREBASE
using Firebase.Analytics;
#endif

namespace Easypapa
{
    public static class FirebaseLogger
    {
        public static bool Enabled { get; set; } = true;

        public static void LogEvent(string eventName)
        {
            if (!Enabled) return;

            eventName = SanitizeEventName(eventName);
            if (string.IsNullOrEmpty(eventName)) return;

#if EASYPAPA_FIREBASE
            try
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            Debug.Log($"[FirebaseLogger] {eventName}");
#endif
        }

        public static void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (!Enabled) return;

            eventName = SanitizeEventName(eventName);
            if (string.IsNullOrEmpty(eventName)) return;

#if EASYPAPA_FIREBASE
            try
            {
                if (parameters == null || parameters.Count == 0)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                    return;
                }

                var list = new List<Parameter>(parameters.Count);
                foreach (var kv in parameters)
                {
                    if (string.IsNullOrEmpty(kv.Key)) continue;

                    var key = SanitizeParamName(kv.Key);
                    if (string.IsNullOrEmpty(key)) continue;

                    if (TryToParameter(key, kv.Value, out var p))
                        list.Add(p);
                }

                if (list.Count == 0)
                    FirebaseAnalytics.LogEvent(eventName);
                else
                    FirebaseAnalytics.LogEvent(eventName, list.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            Debug.Log($"[FirebaseLogger] {eventName} | params={(parameters == null ? "null" : parameters.Count.ToString())}");
#endif
        }

        public static void SetUserId(string userId)
        {
            if (!Enabled) return;

#if EASYPAPA_FIREBASE
            try { FirebaseAnalytics.SetUserId(userId); }
            catch (Exception e) { Debug.LogException(e); }
#else
            Debug.Log($"[FirebaseLogger] SetUserId: {userId}");
#endif
        }

        public static void SetUserProperty(string name, string value)
        {
            if (!Enabled) return;

            name = SanitizeParamName(name);
            if (string.IsNullOrEmpty(name)) return;

#if EASYPAPA_FIREBASE
            try { FirebaseAnalytics.SetUserProperty(name, value); }
            catch (Exception e) { Debug.LogException(e); }
#else
            Debug.Log($"[FirebaseLogger] SetUserProperty: {name}={value}");
#endif
        }

        public static void SetAnalyticsCollectionEnabled(bool enabled)
        {
#if EASYPAPA_FIREBASE
            try { FirebaseAnalytics.SetAnalyticsCollectionEnabled(enabled); }
            catch (Exception e) { Debug.LogException(e); }
#else
            Debug.Log($"[FirebaseLogger] CollectionEnabled: {enabled}");
#endif
        }

#if EASYPAPA_FIREBASE
        private static bool TryToParameter(string key, object value, out Parameter parameter)
        {
            parameter = default;

            if (value == null)
            {
                parameter = new Parameter(key, string.Empty);
                return true;
            }

            switch (value)
            {
                case string s:
                    parameter = new Parameter(key, s);
                    return true;

                case bool b:
                    parameter = new Parameter(key, b ? 1L : 0L);
                    return true;

                case int i:
                    parameter = new Parameter(key, (long)i);
                    return true;

                case long l:
                    parameter = new Parameter(key, l);
                    return true;

                case float f:
                    parameter = new Parameter(key, (double)f);
                    return true;

                case double d:
                    parameter = new Parameter(key, d);
                    return true;

                case decimal m:
                    parameter = new Parameter(key, (double)m);
                    return true;

                default:
                    parameter = new Parameter(key, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                    return true;
            }
        }
#endif

        private static string SanitizeEventName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            s = s.Trim();
            var sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
                else if (c == ' ' || c == '-' || c == '.') sb.Append('_');
            }

            var r = sb.ToString();
            if (r.Length == 0) return string.Empty;
            if (char.IsDigit(r[0])) r = "_" + r;
            if (r.Length > 40) r = r.Substring(0, 40);
            return r;
        }

        private static string SanitizeParamName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            s = s.Trim();
            var sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
                else if (c == ' ' || c == '-' || c == '.') sb.Append('_');
            }

            var r = sb.ToString();
            if (r.Length == 0) return string.Empty;
            if (char.IsDigit(r[0])) r = "_" + r;
            if (r.Length > 40) r = r.Substring(0, 40);
            return r;
        }
    }
}
