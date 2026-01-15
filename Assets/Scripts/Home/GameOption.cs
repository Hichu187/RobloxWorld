using Easypapa;
using Hichu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameOption : MonoBehaviour
    {
        [SerializeField] Image _image;
        [SerializeField] TextMeshProUGUI _gametitle;
        [SerializeField] TextMeshProUGUI _like;
        [SerializeField] TextMeshProUGUI _user;
        [SerializeField] GameObject _commingSoon;

        private MinigameConfig data;
        public void InitData(MinigameConfig config)
        {
            data = config;

            _commingSoon.SetActive(config.commingSoon);
            _image.sprite = config.gameIcon;
            _gametitle.text = config.gameTitle;
            _like.text = $"{config.like} %";
            _user.text = $"{config.user} K";
        }

        public void SelectMinigame()
        {
            if (data.gameSceneName == "") return;
            SceneLoaderHelper.Load(data.gameSceneName);

            Easypapa.AdHelper.ShowBanner();
            //Easypapa.LogHelper.P("mode_start", data.gameSceneName.Trim());
            Easypapa.EasypapaAdSdk.LogEvent($"mode_start_{data.gameSceneName.Trim()}","mode", data.gameSceneName.Trim());
        }
    }
}
