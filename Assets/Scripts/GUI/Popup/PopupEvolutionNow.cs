using Hichu;
using UnityEngine;
using UnityEngine.UI;
using Easypapa;

namespace Game
{
    public class PopupEvolutionNow : MonoBehaviour
    {
        [SerializeField] Image currentIcon;
        [SerializeField] Image nextIcon;

        [SerializeField] Button btn_Evo;
        [SerializeField] Button btn_nothanks;

        private View view;

        private void Start()
        {
            currentIcon.sprite = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level].texture;
            nextIcon.sprite = FactoryBrainrotEvo.brainrotConfigs[DataBrainrotEvo.level + 1].texture;

            currentIcon.SetNativeSize();
            nextIcon.SetNativeSize();

            view = GetComponent<View>();

            btn_Evo.onClick.AddListener(EvoNow);
        }

        private void EvoNow()
        {
            Easypapa.AdHelper.ShowRewarded(
                "brainrot_Evo_now",
                rewarded =>
                {
                    if (rewarded)
                    {
                        DataBrainrotEvo.instance.LevelUpBoost();
                        view.Close();
                    }
                    else
                    {
                        Debug.Log("Rewarded NOT completed.");
                    }
                });

        }

        public void Nothank()
        {
            Easypapa.AdHelper.ShowInterstitial("button_nothanks_evo_now");
            view.Close();
        }
    }
}
