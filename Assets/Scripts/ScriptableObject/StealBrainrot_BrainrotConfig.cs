using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [CreateAssetMenu(fileName = "StealBrainrot_BrainrotConfig", menuName = "Game/StealBrainrot/Brainrot Config")]
    public class StealBrainrot_BrainrotConfig : ScriptableObject
    {
        public int ID;
        public string brainrotName;
        public PetRank rank;
        public int earningPerSecond;
        public int costToBuy;

        [PreviewField(100, ObjectFieldAlignment.Left)]
        public Sprite texture;

        public GameObject prefab;
        public bool reward;

#if UNITY_EDITOR
        [Button("Sync Name From Prefab", ButtonSizes.Large)]
        private void SyncNameFromPrefab()
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[{name}] Prefab is missing, cannot sync name.");
                return;
            }

            brainrotName = prefab.name.Trim();

            string path = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                string newName = $"StealBrainrot_{rank}_{brainrotName}";
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
