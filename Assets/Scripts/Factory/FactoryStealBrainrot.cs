using Hichu;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace Game
{
    [CreateAssetMenu(fileName = "FactoryStealBrainrot", menuName = "Game/StealBrainrot/Factory")]
    public class FactoryStealBrainrot : ScriptableObjectSingleton<FactoryStealBrainrot>
    {
        [SerializeField] private List<StealBrainrot_BrainrotConfig> _brainrotConfigs;

        public static List<StealBrainrot_BrainrotConfig> brainrotConfigs => instance._brainrotConfigs;

#if UNITY_EDITOR
        [Button("Sync IDs (Index = ID)", ButtonSizes.Large)]
        private void SyncIDs()
        {
            if (_brainrotConfigs == null || _brainrotConfigs.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _brainrotConfigs.Count; i++)
            {
                var config = _brainrotConfigs[i];
                if (config == null)
                {
                    continue;
                }

                config.ID = i;
                EditorUtility.SetDirty(config);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
