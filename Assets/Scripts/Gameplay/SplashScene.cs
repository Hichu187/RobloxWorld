using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
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
            yield return new WaitForSeconds(_delayToHome);
            ChangeScene();
        }

        private void ChangeScene()
        {
            SceneManager.LoadScene(_homeSceneName);
            Easypapa.AdHelper.ShowAppOpen();
        }
    }
}
