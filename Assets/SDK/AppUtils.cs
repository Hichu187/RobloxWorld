using System;
using UnityEngine;

namespace Easypapa
{
    public static class AppUtils
    {
        public static long CurrentTimeMiliseconds()
        {
            //return DateTime.Now.Millisecond;
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            return millis;
        }
        public static long CurrentTimeSeconds()
        {
            return (long)(CurrentTimeMiliseconds() / 1000);
        }

        private static int appVersion = -1;
        public static int GetAppVersion()
        {
            if (appVersion >= 0) return appVersion;
            try
            {
                string appVersion = Application.version;
                string[] arr = appVersion.Split('.');
                if (arr.Length != 2 && Int32.Parse(arr[0]) != 1)
                {
                    throw new Exception("Version Code format invalid. You murst use format: 1.1, 1.2, 1.3, ...");
                }
                int version = Int32.Parse(arr[1]);
                //if (BounceSdk.abTestId >= 0) version = version * 1000 + BounceSdk.abTestId;
                AppUtils.appVersion = version;
                return version;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return 1;
        }
    }
}