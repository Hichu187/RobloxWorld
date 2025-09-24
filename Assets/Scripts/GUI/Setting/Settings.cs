
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
            //_panel.sizeDelta = new Vector2(_panel.sizeDelta.x, isHome ? 385f : 495f);
            _objHome.GetComponent<Button>().onClick.AddListener(BtnHome_OnClick);
        }

        private void BtnHome_OnClick()
        {
            SceneLoaderHelper.Load(1);

        }
    }
}
