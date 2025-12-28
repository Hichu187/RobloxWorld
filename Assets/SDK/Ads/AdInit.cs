using UnityEngine;
using System.Collections;

namespace Easypapa
{
    public class AdInit : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DelayShowBanner());
        }

        private IEnumerator DelayShowBanner()
        {
            EasypapaAdSdk.InitOnStartup();

            yield return new WaitForSeconds(3f);

            AdHelper.ShowBanner();
            AdHelper.ShowAppOpen();
            Debug.Log(Easypapa.RemoteConfig.CONFIG.modeSort);
;
        }
    }
}
