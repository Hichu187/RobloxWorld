using System.Collections;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Popup_Tower_Drop : PopupBase
    {
        [SerializeField] TextMeshProUGUI countDown;

        private Coroutine _countDownCoroutine;

        private void OnEnable()
        {
            StartCountDown(10);
        }

        private void OnDisable()
        {
            if (_countDownCoroutine != null)
            {
                StopCoroutine(_countDownCoroutine);
                _countDownCoroutine = null;
            }
        }

        protected override void Start()
        {
            base.Start();
        }


        public void StartCountDown(int startValue)
        {
            if (_countDownCoroutine != null)
                StopCoroutine(_countDownCoroutine);

            _countDownCoroutine = StartCoroutine(CountDownCoroutine(startValue));
        }

        private IEnumerator CountDownCoroutine(int value)
        {
            int current = value;

            while (current >= 0)
            {
                if (countDown != null)
                    countDown.text = current.ToString();

                yield return new WaitForSeconds(1f);
                current--;
            }

            _countDownCoroutine = null;
            OnCountDownFinished();
        }

        protected virtual void OnCountDownFinished()
        {
            // TODO: xử lý khi đếm ngược kết thúc
        }

        public void GetReturnFloor()
        {
            Easypapa.AdHelper.ShowRewarded(
                "button_return_tower",
                rewarded =>
                {
                    if (rewarded)
                    {
                        TowerGameplay gameplay = FindAnyObjectByType<TowerGameplay>();
                        gameplay.ReturnCheckPoint();
                    }
                    else
                    {
                        Debug.Log("Rewarded NOT completed.");
                    }
                });

            view.Close();
        }

        public void NothanksClick()
        {
            Easypapa.AdHelper.ShowInterstitial("button_nothank_popup_tower");
            TowerGameplay gameplay = FindAnyObjectByType<TowerGameplay>();
            gameplay.ResetCurrent();
            view.Close();
        }
    }
}
