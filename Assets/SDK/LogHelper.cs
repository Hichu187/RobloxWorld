using System.Collections.Generic;

namespace Easypapa
{
    public readonly struct EPParam
    {
        public readonly string Key;
        public readonly object Value;

        public EPParam(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    public static class LogHelper
    {
        public static EPParam P(string key, object value) => new EPParam(key, value);

        public static void Log(string eventName)
        {
            FirebaseLogger.LogEvent(eventName);
        }

        public static void Log(string eventName, params EPParam[] ps)
        {
            if (ps == null || ps.Length == 0)
            {
                FirebaseLogger.LogEvent(eventName);
                return;
            }

            var dict = new Dictionary<string, object>(ps.Length);
            for (int i = 0; i < ps.Length; i++)
            {
                var k = ps[i].Key;
                if (string.IsNullOrEmpty(k)) continue;
                dict[k] = ps[i].Value;
            }

            FirebaseLogger.LogEvent(eventName, dict);
        }

        public static void LogString(string eventName, string key, string value)
        {
            FirebaseLogger.LogEvent(eventName, new Dictionary<string, object>(1)
            {
                { key, value }
            });
        }

        public static void LogInt(string eventName, string key, int value)
        {
            FirebaseLogger.LogEvent(eventName, new Dictionary<string, object>(1)
            {
                { key, value }
            });
        }

        public static void LogBool(string eventName, string key, bool value)
        {
            FirebaseLogger.LogEvent(eventName, new Dictionary<string, object>(1)
            {
                { key, value }
            });
        }
    }
}
