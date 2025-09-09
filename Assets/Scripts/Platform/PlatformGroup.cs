using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public class PlatformGroup : MonoBehaviour
    {
/*#if UNITY_EDITOR
        [Button]
        private void UpdatePlatformFade()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            int index = Random.Range(0, FactoryEditor.MaterialPlatformFade.Length);

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = FactoryEditor.MaterialPlatformFade.GetLoop(index + i);

                EditorUtility.SetDirty(meshRenderers[i]);
            }
        }

        [Button]
        private void UpdatePlatform()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            List<Material> materials = new List<Material>(FactoryEditor.MaterialPlatform);

            materials.Shuffle();

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = materials.GetLoop(i);

                EditorUtility.SetDirty(meshRenderers[i]);
            }
        }

        private static int s_colorIndex = 0;

        [Button]
        private void UpdatePlatformOneColor()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            int index = s_colorIndex;

            s_colorIndex++;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = FactoryEditor.MaterialPlatform.GetLoop(index);

                EditorUtility.SetDirty(meshRenderers[i]);
            }
        }
#endif*/
    }
}
