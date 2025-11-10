using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class SlapBagOption : MonoBehaviour
    {
        public ItemConfig data;
        public Image image;
        public GameObject status;

        public void InitData(ItemConfig config)
        {
            data = config;

            image.sprite = config.sprite;
            image.SetNativeSize();

            if (config.IsCurrent())
            {
                status.gameObject.SetActive(true);
            }
            else
            {
                status.gameObject.SetActive(false);
            }
        }
    }
}
