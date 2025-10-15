using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class StealBrainrot_Manager : MonoBehaviour
    {
        public static StealBrainrot_Manager instance;

        public Transform startPoint;
        public Transform endPoint;

        public List<StealBrainrot_Base> baseLists;
        private void Awake()
        {
            instance = this;
        }
        public static string FormatMoney(long value)
        {
            if (value >= 1_000_000_000_000)
                return (value / 1_000_000_000_000f).ToString("0.#") + "T";

            if (value >= 1_000_000_000)
                return (value / 1_000_000_000f).ToString("0.#") + "B";

            if (value >= 1_000_000)
                return (value / 1_000_000f).ToString("0.#") + "M";

            if (value >= 1_000)
                return (value / 1_000f).ToString("0.#") + "K";

            return value.ToString();
        }
    }
}
