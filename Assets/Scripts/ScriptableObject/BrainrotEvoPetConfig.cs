using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Game
{
    public enum PetRank { Common = 0, Uncommon = 1, Rare = 2, Epic = 3, Legendary = 4}
    public enum MoveMode { Fly, Run, Hop }

    [CreateAssetMenu(menuName = "Game/BrainrotEvo/Pet Config", fileName = "Evo Pet")]
    public class BrainrotEvoPetConfig : ScriptableObject
    {
        [Title("Identity")]
        public string petName;

        [PreviewField(100, ObjectFieldAlignment.Left)]
        public Sprite petIcon;

        [Title("References")]
        public GameObject petModel;

        [Title("Config")]
        public PetRank petRank = PetRank.Common;
        public MoveMode petMove = MoveMode.Fly;
        public float bonusDamage = 0f;

#if UNITY_EDITOR
        private const string ICON_FOLDER = "Assets/Arts/2D/Icon Pet";

        [Button("Rename Asset to:  Evo Pet {petName}", ButtonSizes.Medium)]
        private void RenameAssetToPattern()
        {
            if (string.IsNullOrWhiteSpace(petName))
            {
                Debug.LogWarning("[BrainrotEvoPetConfig] petName is empty. Please set a name before renaming.");
                return;
            }

            string newFileName = $"Evo Pet {SanitizeFileName(petName)}.asset";
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("[BrainrotEvoPetConfig] Could not find asset path. Save the asset first.");
                return;
            }

            string dir = Path.GetDirectoryName(assetPath);
            string targetPath = Path.Combine(dir, newFileName).Replace("\\", "/");

            if (assetPath.EndsWith(newFileName))
            {
                Debug.Log($"[BrainrotEvoPetConfig] Asset already named '{newFileName}'.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(targetPath) != null)
            {
                targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
            }

            string error = AssetDatabase.RenameAsset(assetPath, Path.GetFileNameWithoutExtension(targetPath));
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[BrainrotEvoPetConfig] Rename failed: {error}");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[BrainrotEvoPetConfig] Renamed to '{Path.GetFileName(targetPath)}'");
        }

        [Button("Auto-Find Icon (by model/name in Assets/Arts/2D/Icon Pet)")]
        private void AutoFindAndAssignIcon()
        {
            if (!AssetDatabase.IsValidFolder(ICON_FOLDER))
            {
                Debug.LogWarning($"[BrainrotEvoPetConfig] Folder not found: {ICON_FOLDER}");
                return;
            }

            // Danh sách tên thử: ưu tiên tên model, sau đó petName
            var candidates = new System.Collections.Generic.List<string>(2);
            if (petModel != null) candidates.Add(CleanName(petModel.name));
            if (!string.IsNullOrWhiteSpace(petName)) candidates.Add(CleanName(petName));

            foreach (var candidate in candidates)
            {
                var sprite = FindSpriteByNameInFolder(candidate, ICON_FOLDER);
                if (sprite != null)
                {
                    petIcon = sprite;
                    EditorUtility.SetDirty(this);
                    Debug.Log($"[BrainrotEvoPetConfig] Assigned icon: '{sprite.name}' from {ICON_FOLDER}");
                    petName = petModel.name;
                    return;
                }
            }

            Debug.LogWarning($"[BrainrotEvoPetConfig] No sprite found in {ICON_FOLDER} matching: {(candidates.Count > 0 ? string.Join(", ", candidates) : "(no candidates)")}.");
        }

        private static Sprite FindSpriteByNameInFolder(string fileNameNoExt, string folder)
        {
            if (string.IsNullOrWhiteSpace(fileNameNoExt)) return null;

            // Tìm nhanh theo từ khóa (FindAssets có thể trả về nhiều kết quả gần giống)
            string[] guids = AssetDatabase.FindAssets($"t:Sprite {fileNameNoExt}", new[] { folder });
            Sprite best = null;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                // Ưu tiên khớp tên chính xác (không phân biệt hoa thường)
                string nameNoExt = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(nameNoExt, fileNameNoExt, System.StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }

                // Nếu chưa có exact match, giữ tạm cái đầu tiên (fallback)
                if (best == null)
                    best = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return best;
        }

        private static string CleanName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            // Loại "(Clone)" và khoảng trắng thừa
            string s = raw.Replace("(Clone)", "").Trim();
            return s;
        }

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid)
                name = name.Replace(c.ToString(), "_");
            return name.Trim();
        }
#endif
    }
}
