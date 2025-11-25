using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

namespace Game
{
    public enum ItemType { Slap, Buff, Control, Debuff }

    [CreateAssetMenu(menuName = "Game/Item Config", fileName = "ItemConfig")]
    public class ItemConfig : ScriptableObject
    {
        [Title("Settings")]
        public ItemType itemType;
        public string itemName;
        public Sprite sprite;

        [Title("Combat Config")]
        [Min(0f)] public float hitForce = 0f;
        [Min(0f)] public float hitRange = 0f;
        public GameObject hitVfx;

        [NonSerialized] private ItemData _data;
        public ItemData data
        {
            get
            {
                if (_data == null)
                    _data = DataItem.Get(itemName);
                return _data;
            }
        }

        public bool IsCurrent()
        {
            return string.CompareOrdinal(DataItem.current, this.itemName) == 0;
        }

#if UNITY_EDITOR
        [Button("Rename Asset", ButtonSizes.Large)]
        private void Rename()
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return;

            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
                return;

            string cleanType = Regex.Replace(itemType.ToString(), @"[^a-zA-Z0-9]", "");
            string cleanName = Regex.Replace(itemName, @"[^a-zA-Z0-9]", "");
            string newName = $"{cleanType}_{cleanName}";

            string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (currentName == newName)
                return;

            AssetDatabase.RenameAsset(assetPath, newName);
            AssetDatabase.SaveAssets();
        }
#endif
    }

    [Serializable]
    public class ItemData
    {
        [SerializeField] private bool _isUnlocked;
        public bool isUnlocked => _isUnlocked;

        public void Unlock()
        {
            _isUnlocked = true;
        }
    }
}
