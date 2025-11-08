using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public class MinigameConfig : ScriptableObject
    {
        public string gameTitle;
        public Sprite gameIcon;
        [Range(0, 100)] public float like;
        public float user;
        public bool commingSoon;
        public string gameSceneName;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(gameTitle)) return;

            string assetPath = AssetDatabase.GetAssetPath(this);
            string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (!string.Equals(currentName, gameTitle, System.StringComparison.Ordinal))
            {
                AssetDatabase.RenameAsset(assetPath, gameTitle);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
#endif
    }
}
