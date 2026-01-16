using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class TowerFloor : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private TowerWall _wall;
        [SerializeField] private TowerGameplay gameplay;
        public List<PlatformCheckpoint> checkpoints;
        [Title("Config")]
        public int floorId;
        [SerializeField] private float _height;

        public float height { get { return _height; } }

        public TowerWall wall { get { return _wall; } }


        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.GetComponent<Character>()) return;
            if (!collision.collider.GetComponent<Character>().isPlayer) return;

            if(gameplay.curFloorID > floorId)
            {
                if (gameplay.curCheckpoint == null) return;

                StaticBus<Event_DropFloor>.Post(null);
            }
        }

#if UNITY_EDITOR

        [Button]
        private void UpdateFloor()
        {
            MeshRenderer[] meshRenderer = GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshRenderer.Length; i++)
            {
                meshRenderer[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        [Button("Get Checkpoints")]
        private void GetCheckpoint()
        {
            if (checkpoints == null)
                checkpoints = new List<PlatformCheckpoint>();
            else
                checkpoints.Clear();

            checkpoints.AddRange(
                GetComponentsInChildren<PlatformCheckpoint>(true)
            );

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnValidate()
        {
            if (Application.isPlaying || UnityEditor.Selection.objects == null || !UnityEditor.Selection.objects.Contains(gameObject))
                return;

            _wall.SetHeight(_height);
        }
#endif


    }
}
