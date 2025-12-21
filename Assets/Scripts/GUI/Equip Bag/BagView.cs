using Hichu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BagView : MonoBehaviour
    {
        public SlapBagOption optionPrefab;
        public List<SlapBagOption> option = new List<SlapBagOption>();
        public Transform content;

        private void Start()
        {
            option.Clear();

            foreach (var pair in DataItem.datas)
            {
                string itemName = pair.Key;
                ItemData data = pair.Value;

                if (data.isUnlocked)
                {
                    SlapBagOption opt = optionPrefab.Create(content);
                    option.Add(opt);
                }
            }

            int index = 0;
            foreach (var pair in DataItem.datas)
            {
                if (!pair.Value.isUnlocked) continue;

                // Lấy config tương ứng từ FactoryItem
                ItemConfig config = FactoryItem.items.Find(x => x.itemName == pair.Key);
                if (config == null) continue;

                option[index].InitData(config);

                int id = index;
                option[id].GetComponent<Button>().onClick.AddListener(() => SelectOption(id));

                index++;
            }
        }

        public void SelectOption(int id)
        {
            foreach (var op in option)
                op.status.SetActive(false);

            option[id].status.SetActive(true);

            string itemName = option[id].data.itemName;

            // Lưu item đang chọn
            DataItem.SetCurrentItem(itemName);

            // Lấy config từ DataItem → ItemConfig
            ItemConfig config = FactoryItem.items.Find(x => x.itemName == itemName);

            if (config != null)
            {
                Player.Instance.character.itemManager.ActiveItem(config);
            }
        }
    }
}
