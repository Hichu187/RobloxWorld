using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CharacterItemManager : MonoBehaviour
    {
        [Title("Items")]
        public List<Item> items;

        [Title("Runtime")]
        [ReadOnly] public Item currentItem;   // item đang active hiện tại

        private Character character;
        [Button]
        public void ActiveItem(int id)
        {
            if (items == null || items.Count == 0) return;
            if (id < 0 || id >= items.Count) return;

            foreach (var i in items)
            {
                i.InActive();
            }

            items[id].ActiveItem();
            currentItem = items[id];

            if (character == null) character = GetComponentInParent<Character>();
            character.fov.radius = currentItem.config.hitRange;
            character.cCombat._knockbackForce = currentItem.config.hitForce;
        }

        [Button]
        public void UnlockItem(int id)
        {
            if (FactoryItem.items == null || id < 0 || id >= FactoryItem.items.Count)
                return;

            if (FactoryItem.items[id].data.isUnlocked) return;

            FactoryItem.items[id].data.Unlock();
        }
    }
}
