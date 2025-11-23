using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "AdConfig", menuName = "Game/Ads/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("=== ANDROID IDS ===")]
        [Space(5)]
        public string androidBannerId;
        public string androidInterstitialId;
        public string androidRewardedId;
        public string androidAppOpenId;

        [Header("=== IOS IDS ===")]
        [Space(5)]
        public string iosBannerId;
        public string iosInterstitialId;
        public string iosRewardedId;
        public string iosAppOpenId;
    }
}
