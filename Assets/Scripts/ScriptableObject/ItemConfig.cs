using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Game
{
    public enum ItemType { Slap, Buff, Control, Debuff }

    [CreateAssetMenu(menuName = "Game/Item Config", fileName = "ItemConfig")]
    public class ItemConfig : ScriptableObject
    {
        public ItemType itemType;
        public string itemName;

#if UNITY_EDITOR
        [Button("Rename Asset", ButtonSizes.Large)]
        private void Rename()
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            string cleanType = Regex.Replace(itemType.ToString(), @"[^a-zA-Z0-9]", "");
            string cleanName = Regex.Replace(itemName, @"[^a-zA-Z0-9]", "");
            string newName = $"{cleanType}_{cleanName}";

            string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (currentName == newName)
            {
                return;
            }

            AssetDatabase.RenameAsset(assetPath, newName);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
