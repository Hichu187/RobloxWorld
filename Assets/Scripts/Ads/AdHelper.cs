using System;
using UnityEngine;

namespace Game
{
    public static class AdHelper
    {
        private static AdManager M
        {
            get
            {
                if (AdManager.Instance == null)
                {
                    Debug.LogError("[AdHelper] Không tìm thấy AdManager.Instance. Hãy chắc chắn có AdManager trong scene!");
                }
                return AdManager.Instance;
            }
        }

        // ==========================
        // BANNER
        // ==========================
        public static void LoadBanner()
        {
            if (M == null) return;
            M.LoadBanner();
        }

        public static void ShowBanner()
        {
            if (M == null) return;
            M.ShowBanner();
        }

        public static void HideBanner()
        {
            if (M == null) return;
            M.HideBanner();
        }

        public static void DestroyBanner()
        {
            if (M == null) return;
            M.DestroyBanner();
        }

        // ==========================
        // INTERSTITIAL
        // ==========================
        public static void LoadInterstitial()
        {
            if (M == null) return;
            M.LoadInterstitial();
        }

        public static bool IsInterstitialReady()
        {
            if (M == null) return false;
            return M.IsInterstitialReady();
        }

        public static void ShowInterstitial()
        {
            if (M == null) return;

            if (!M.IsInterstitialReady())
            {
                Debug.Log("[AdHelper] Interstitial chưa sẵn sàng → Load lại");
                M.LoadInterstitial();
                return;
            }

            M.ShowInterstitial();
        }

        /// <summary>
        /// Dùng khi bạn muốn logic an toàn: chỉ show nếu chắc chắn ready.
        /// </summary>
        public static bool TryShowInterstitial()
        {
            if (M == null) return false;

            if (M.IsInterstitialReady())
            {
                M.ShowInterstitial();
                return true;
            }

            Debug.Log("[AdHelper] Interstitial chưa sẵn sàng → Load lại");
            M.LoadInterstitial();
            return false;
        }

        // ==========================
        // REWARDED
        // ==========================
        public static void LoadRewarded()
        {
            if (M == null) return;
            M.LoadRewarded();
        }

        public static bool IsRewardedReady()
        {
            if (M == null) return false;
            return M.IsRewardedReady();
        }

        /// <summary>
        /// Show rewarded bắt buộc có kiểm tra.
        /// </summary>
        public static void ShowRewarded(Action onRewarded)
        {
            if (M == null) return;

            if (!M.IsRewardedReady())
            {
                Debug.Log("[AdHelper] Rewarded chưa sẵn sàng → Load lại");
                M.LoadRewarded();
                return;
            }

            M.ShowRewarded(onRewarded);
        }

        /// <summary>
        /// Chỉ show rewarded nếu chắc chắn ready. 
        /// Trả về true nếu show được, false nếu chưa sẵn sàng.
        /// </summary>
        public static bool TryShowRewarded(Action onReward)
        {
            if (M == null) return false;

            if (M.IsRewardedReady())
            {
                M.ShowRewarded(onReward);
                return true;
            }

            Debug.Log("[AdHelper] Rewarded chưa sẵn sàng → Load lại");
            M.LoadRewarded();
            return false;
        }

        // ==========================
        // APP OPEN
        // ==========================
        public static void LoadAppOpen()
        {
            if (M == null) return;
            M.LoadAppOpen();
        }

        public static bool IsAppOpenReady()
        {
            if (M == null) return false;
            return M.IsAppOpenReady();
        }

        public static void ShowAppOpen()
        {
            if (M == null) return;

            if (!M.IsAppOpenReady())
            {
                Debug.Log("[AdHelper] AppOpen chưa sẵn sàng → Load lại");
                M.LoadAppOpen();
                return;
            }

            M.ShowAppOpen();
        }

        public static bool TryShowAppOpen()
        {
            if (M == null) return false;

            if (M.IsAppOpenReady())
            {
                M.ShowAppOpen();
                return true;
            }

            Debug.Log("[AdHelper] AppOpen chưa sẵn sàng → Load lại");
            M.LoadAppOpen();
            return false;
        }
    }
}
