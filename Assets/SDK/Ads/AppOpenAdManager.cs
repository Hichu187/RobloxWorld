using UnityEngine;

namespace Easypapa
{
    public class AppOpenAdManager : MonoBehaviour
    {
        public static AppOpenAdManager INSTANCE;
        public static bool needShowAppOpenFirst = true;

        private void Awake()
        {
            INSTANCE = this;
            DontDestroyOnLoad(this);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                AdSdk.ShowAppOpen();
            }
        }

        public void CheckShowAppOpenFirst()
        {
            Debug.Log("CheckShowAppOpenFirst");
            if (needShowAppOpenFirst)
            {
                needShowAppOpenFirst = false;
                AdSdk.ShowAppOpen();
            }
        }
    }
}
