using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CharacterItemManager : MonoBehaviour
    {
        public List<Item> items;

        private void Start()
        {

        }

        [Button]
        public void ActiveItem(int id)
        {
            foreach(var i in items)
            {
                i.InActive();
            }

            items[id].ActiveItem();
        }
    }
}
