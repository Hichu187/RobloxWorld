using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hichu.Editor
{
    public static class EditorHelper
    {
        [MenuItem("Hichu/Clear Device Data")]
        private static void ClearDeviceData()
        {
            LDataBlockHelper.ClearDeviceData();
            PlayerPrefs.DeleteAll();
        }
        [MenuItem("Hichu/Create Audio Config File")]
        public static void CreateAudioConfig()
        {
            foreach (var s in Selection.objects)
            {
                var assetPath = AssetDatabase.GetAssetPath(s);

                if (s.GetType() != typeof(AudioClip))
                    continue;

                AudioClip clip = (AudioClip)s;

                AudioConfig config = ScriptableObjectHelper.CreateAsset<AudioConfig>(Path.GetDirectoryName(assetPath), clip.name);

                if (config == null)
                    continue;

                config.Construct(clip);

                ScriptableObjectHelper.SaveAsset(config);
            }
        }
    }
}
