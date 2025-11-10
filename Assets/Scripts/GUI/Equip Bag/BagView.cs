using Hichu;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class BagView : MonoBehaviour
    {
        public SlapBagOption optionPrefab;
        public List<SlapBagOption> option;
        public Transform content;

        private void Start()
        {
            for (int i = 0; i < FactoryItem.items.Count; i++)
            {
                int index = i;

                if (FactoryItem.items[index].data.isUnlocked)
                {
                    SlapBagOption opt = optionPrefab.Create(content);
                    option.Add(opt);
                }
            }

            for (int i = 0; i < option.Count; i++)
            {
                int index = i;
                option[index].InitData(FactoryItem.items[index]);

                option[index].GetComponent<Button>().onClick.AddListener(() => { SelectOption(index); });
            }
        }

        public void SelectOption(int id)
        {
            foreach( var op in option)
            {
                op.status.SetActive(false);
            }

            option[id].status.SetActive(true);

            DataItem.SetCurrentItem(option[id].data.itemName);

            Player.Instance.character.itemManager.ActiveItem(FactoryItem.items.IndexOf(option[id].data));
        }
    }
}
