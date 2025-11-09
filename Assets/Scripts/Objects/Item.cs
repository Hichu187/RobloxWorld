using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Item : MonoBehaviour
    {
        public ItemConfig config;

        public List<GameObject> models;

        public void ActiveItem()
        {
            foreach(var m in models)
            {
                m.SetActive(true);
            }
        }

        public void InActive()
        {
            foreach (var m in models)
            {
                m.SetActive(false);
            }
        }
    }
}
