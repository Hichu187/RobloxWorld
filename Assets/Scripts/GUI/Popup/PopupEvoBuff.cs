using DG.Tweening;
using Hichu;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PopupEvoBuff : MonoBehaviour
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

            if (DataBrainrotEvo.boostFree)
            {
                btn_Get.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                btn_Get.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        private IEnumerator NothankCoroutine()
        {
            btn_nothnanks.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            btn_nothnanks.gameObject.SetActive(true);
        }

        private void ActiveBoost()
        {
            if (DataBrainrotEvo.boostFree)
            {
                BuffEffectiveStart();
                view.Close();
            }
            else
            {
                Easypapa.AdHelper.ShowRewarded(
                    "brainrot_evo_power",
                    rewarded =>
                    {
                        if (rewarded)
                        {
                            BuffEffectiveStart();
                            view.Close();
                        }
                        else
                        {
                            Debug.Log("Rewarded NOT completed.");
                        }
                    });
            }      
        }

        private void BuffEffectiveStart()
        {
            BrainrotEvoView brainrotView = FindAnyObjectByType<BrainrotEvoView>();
            brainrotView.BuffActive();
        }
    }
}
