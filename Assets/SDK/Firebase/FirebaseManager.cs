#define USE_FIREBASE

using UnityEngine;
using System;
using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using Newtonsoft.Json;
#if USE_FIREBASE
using Firebase.Analytics;
#endif

namespace Easypapa
{
    public class FirebaseManager
    {
        public static event Action eventRemoteConfigLoaded;

        private static bool firebaseInitialized = false;

        public static void Init()
        {
#if USE_FIREBASE
#if UNITY_ANDROID
            _InitFirebase();
#elif UNITY_IOS
        _InitFirebase();
#else
            _InitFirebase();
#endif
#endif
        }

        private static void _InitFirebase()
        {
#if USE_FIREBASE
            Debug.Log("FirebaseManager Init");
            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    var dependencyStatus = task.Result;
                    Debug.Log("InitFirebase Success: " + dependencyStatus);
                    if (dependencyStatus == global::Firebase.DependencyStatus.Available)
                    {
                        firebaseInitialized = true;
                        EventLogin();
                        InitRemoteConfig();
                    }
                    else
                    {
                        firebaseInitialized = false;
                        Debug.Log("Could not resolve all Firebase dependencies: " + dependencyStatus);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        #region log
        public static void EventLogin()
        {
#if USE_FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
#endif
        }
        public static void Log(string key)
        {
#if USE_FIREBASE
            if (!firebaseInitialized) return;
            FirebaseAnalytics.LogEvent(key, new Parameter("success", 1));
#endif
        }

        public static void Log(string eventName, params object[] parameters)
        {
            Parameter[] pr = null;

            if (parameters != null && parameters.Length > 0 && parameters.Length % 2 == 0)
            {
                pr = new Parameter[parameters.Length / 2];

                for (int i = 0; i < pr.Length; i++)
                {
                    string key = parameters[i * 2].ToString();
                    object val = parameters[i * 2 + 1];

                    if (val is int)
                        pr[i] = new Parameter(key, (int)val);
                    else if (val is long)
                        pr[i] = new Parameter(key, (long)val);
                    else if (val is float)
                        pr[i] = new Parameter(key, (double)(float)val);
                    else if (val is double)
                        pr[i] = new Parameter(key, (double)val);
                    else if (val is bool)
                        pr[i] = new Parameter(key, (bool)val ? 1 : 0);
                    else
                        pr[i] = new Parameter(key, val.ToString());
                }
            }

            if (pr != null)
                FirebaseAnalytics.LogEvent(eventName, pr);
            else
                FirebaseAnalytics.LogEvent(eventName);
        }
        #endregion

        #region remote config
        static System.Collections.Generic.Dictionary<string, object> defaultConfig = new System.Collections.Generic.Dictionary<string, object>();
        public static void InitRemoteConfig()
        {
            Debug.Log("InitRemoteConfig");
            FetchDataAsync();
        }

        public static void FetchDataAsync()
        {
            Debug.Log("Firebase Fetching Data...");
            ConfigSettings configSettings = FirebaseRemoteConfig.DefaultInstance.ConfigSettings;
            configSettings.MinimumFetchIntervalInMilliseconds = 1000;
            configSettings.FetchTimeoutInMilliseconds = 10000;
            Dictionary<string, object> defaults = new Dictionary<string, object>();
            FirebaseRemoteConfig.DefaultInstance.SetConfigSettingsAsync(configSettings).ContinueWith((action3) =>
            {
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWith((action4) =>
                {
                    FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread((action) =>
                    {
                        LoadRemoteConfig();
                    });
                });
            });
        }

        public static void LoadRemoteConfig()
        {
            Debug.Log("LoadRemoteConfig");
            if (FirebaseRemoteConfig.DefaultInstance == null || FirebaseRemoteConfig.DefaultInstance.Keys == null) return;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (string key in FirebaseRemoteConfig.DefaultInstance.Keys)
            {
                dic.Add(key, FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue);
            }
            RemoteConfig.CONFIG = JsonConvert.DeserializeObject<RemoteConfig>(JsonConvert.SerializeObject(dic, Newtonsoft.Json.Formatting.Indented));
            RemoteConfig.CONFIG.DecodeData();
            Debug.Log("LoadRemoteConfig Complete: " + JsonConvert.SerializeObject(RemoteConfig.CONFIG));

            eventRemoteConfigLoaded?.Invoke();
        }

        public static string GetStringValueConfig(string key, string def = null)
        {
            try
            {
                ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                if (value.StringValue == null || value.StringValue.Length == 0) return def;
                return value.StringValue;
            }
            catch (Exception e)
            {
                //Debug.LogError(e.Message);
            }
            try
            {
                if (defaultConfig.ContainsKey(key)) return defaultConfig[key].ToString();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return def;
        }

        public static int GetIntValueConfig(string key, int def = -1)
        {
            try
            {
                ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                return (int)value.LongValue;
            }
            catch (Exception e)
            {
                //Debug.LogError(e.Message);
            }
            return def;
        }
        public static bool GetBoolValueConfig(string key, bool def = false)
        {
            try
            {
                ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                return value.BooleanValue;
            }
            catch (Exception e)
            {
                //Debug.LogError(e.Message);
            }
            return def;
        }
        public static float GetFloatValueConfig(string key, float def = -1)
        {
            try
            {
                ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                return (float)value.DoubleValue;
            }
            catch (Exception e)
            {
                //Debug.LogError(e.Message);
            }
            return def;
        }
        #endregion remote config
    }
}
