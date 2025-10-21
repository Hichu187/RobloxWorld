using UnityEngine;
using Hichu;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Game
{


    public class BrainrotEvoConfig : ScriptableObject
    {
        public string brainrotName;
        public GameObject model;

        [Title("Config")]
        public int exp;
        public int damage = 0;
        public int health;

#if UNITY_EDITOR
        [Button("Sync Name From Prefab", ButtonSizes.Large)]
        private void SyncNameFromPrefab()
        {
            if (model == null)
            {
                Debug.LogWarning($"[{name}] Prefab is missing, cannot sync name.");
                return;
            }

            // Lấy tên model và xử lý
            string rawName = model.name;
            rawName = rawName.Replace("Evo", "");       // bỏ chữ Evo
            rawName = rawName.Replace(" ", "");         // bỏ dấu cách
            brainrotName = rawName;

            // Đổi tên file ScriptableObject
            string path = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                string newName = $"Brainrot_Evo_{brainrotName}";
                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"✅ Synced Brainrot name and file name → {newName}");
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}

