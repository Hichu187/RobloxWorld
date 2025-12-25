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


    }
}
