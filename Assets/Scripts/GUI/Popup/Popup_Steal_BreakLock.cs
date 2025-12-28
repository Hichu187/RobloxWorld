using UnityEngine;

namespace Game
{
    public class Popup_Steal_BreakLock : PopupBase
    {
        public StealBrainrot_Base baseM;
        protected override void Start()
        {
            base.Start();

            btn_Get.onClick.AddListener(BreakTheLock);
        }

        public void BreakTheLock()
        {
            Easypapa.AdHelper.ShowRewarded(
                "button_break_the_lock",
                rewarded =>
                {
                    if (rewarded)
                    {
                        baseM.BreakLock();
                    }
                    else
                    {
                        Debug.Log("Rewarded NOT completed.");
                    }
                });

            view.Close();
        }

        public void Nothank()
        {
            Easypapa.AdHelper.ShowInterstitial("button_nothanks_steal_break_lock");
            view.Close();
        }
    }
}
