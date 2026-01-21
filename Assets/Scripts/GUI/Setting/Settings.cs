
using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class Settings : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private GameObject _objHome;

        private void Start()
        {
            bool isHome = SceneManager.GetActiveScene().buildIndex == 1;

            _objHome.SetActive(!isHome);
            _objHome.GetComponent<Button>().onClick.AddListener(BtnHome_OnClick);
        }

        private void BtnHome_OnClick()
        {
            Easypapa.AdHelper.ShowInterstitial("btn_home");

            SceneLoaderHelper.Load(1);

        }
    }
}
