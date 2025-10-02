using UnityEngine;

namespace Game
{
    public class StealBrainrot_Manager : MonoBehaviour
    {
        public static StealBrainrot_Manager instance;

        public Transform targetPoint;
        public Transform startPoint;

        public static string FormatMoney(long value)
        {
            if (value >= 1_000_000_000_000) // ngàn tỷ
                return (value / 1_000_000_000_000f).ToString("0.#") + "T";

            if (value >= 1_000_000_000) // tỷ
                return (value / 1_000_000_000f).ToString("0.#") + "B";

            if (value >= 1_000_000) // triệu
                return (value / 1_000_000f).ToString("0.#") + "M";

            if (value >= 1_000) // ngàn
                return (value / 1_000f).ToString("0.#") + "K";

            return value.ToString(); // nhỏ hơn 1000 thì giữ nguyên
        }
    }
}
