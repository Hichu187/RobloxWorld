using Hichu;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PopupObbyNextStage : MonoBehaviour
    {
        [SerializeField] Button btn_Get;
        [SerializeField] Button btn_nothnanks;

        private Coroutine nothankCoroutine;
        private View view;
        void Start()
        {
            StartCoroutine(NothankCoroutine());

            btn_Get.onClick.AddListener(ActiveBoost);

            view = GetComponent<View>();

        }

        private IEnumerator NothankCoroutine()
        {
            btn_nothnanks.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            btn_nothnanks.gameObject.SetActive(true);
        }

        private void ActiveBoost()
        {
            Easypapa.AdHelper.ShowRewarded(
                "button_next_Stage",
                rewarded =>
                {
                    if (rewarded)
                    {
                        NextStage();
                    }
                    else
                    {
                        Debug.Log("Rewarded NOT completed.");
                    }
                });

            view.Close();
        }

        private void NextStage()
        {
            BaseGameplay easyObby = FindAnyObjectByType<BaseGameplay>();

            easyObby.GotoNextCheckPoint();
        }
    }
}
