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
        public float exp;
        public float damage = 0;
        public float health;

        [Button]
        private void SetName()
        {
            if (string.IsNullOrWhiteSpace(brainrotName)) return;

            string path = AssetDatabase.GetAssetPath(this);
            string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(path) && currentName != brainrotName)
            {
                AssetDatabase.RenameAsset(path, brainrotName);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

