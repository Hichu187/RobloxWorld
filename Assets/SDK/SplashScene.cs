using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Easypapa
{
    public class SplashScene : MonoBehaviour
    {
        [SerializeField] private float _delayToHome = 2f;
        [SerializeField] private string _homeSceneName = "Home";

        private void Start()
        {
            StartCoroutine(DelayToHome());
        }

        private IEnumerator DelayToHome()
        {
            EasypapaAdSdk.InitOnStartup();
            yield return new WaitForSeconds(_delayToHome);
            ChangeScene();
        }

        private void ChangeScene()
        {
            SceneManager.LoadScene(_homeSceneName);
            Easypapa.AdHelper.ShowAppOpen();
            Easypapa.AdHelper.ShowBanner();
        }
    }
}
