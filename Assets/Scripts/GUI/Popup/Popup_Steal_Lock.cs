using UnityEngine;

namespace Game
{
    public class Popup_Steal_Lock : PopupBase
    {
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
                        StealBrainrot_Manager manager = FindAnyObjectByType<StealBrainrot_Manager>();
                        manager.baseLists[0].LockSpecial5Minutes();
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
