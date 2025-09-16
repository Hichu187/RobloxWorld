using Hichu;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using Vertx.Debugging;

namespace Game
{
    [SelectionBase]
    public class Ladder : MonoBase
    {
        [Title("Reference")]
        [SerializeField] private Transform _modelTransform;

        [Title("Config")]
        [SerializeField] private float _height;

        [Space]

        [SerializeField] private Material _modelMaterial;
        [SerializeField] private float _modelHeight;
        [SerializeField] private GameObject _modelPrefab;

        public float height { get { return _height; } }

        public Vector3 TopPosition { get { return TransformCached.TransformPoint(Vector3.up * _height); } }
        public Vector3 BottomPosition { get { return TransformCached.position; } }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            D.raw(new Shape.Line(TransformCached.position, TransformCached.position + Vector3.up * _height));
        }

        private void OnValidate()
        {
            if (Application.isPlaying || UnityEditor.Selection.objects == null || !UnityEditor.Selection.objects.Contains(gameObject))
                return;

            UpdateHeight();
        }

        [Button]
        private void UpdateHeight()
        {
            if (_modelTransform == null || _modelPrefab == null)
                return;

            BoxCollider collider = GetComponent<BoxCollider>();

            var size = collider.size;
            var center = collider.center;

            size.y = _height;
            center.y = _height * 0.5f;

            collider.size = size;
            collider.center = center;

            UpdateModel();

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void UpdateModel()
        {
            if (_modelHeight <= 0f)
                return;

            int modelCount = Mathf.CeilToInt(_height / _modelHeight);

            _modelTransform.DestroyChildrenImmediate();

            for (int i = 0; i < modelCount; i++)
            {
                GameObject model = UnityEditor.PrefabUtility.InstantiatePrefab(_modelPrefab, _modelTransform) as GameObject;

                if (_modelMaterial != null)
                    model.GetComponentInChildren<MeshRenderer>().material = _modelMaterial;
            }

            for (int i = 0; i < modelCount; i++)
            {
                _modelTransform.GetChild(i).localPosition = Vector3.up * i * _modelHeight;

                if (i == modelCount - 1)
                    _modelTransform.GetChild(i).localScale = new Vector3(1f, (_height - _modelHeight * (modelCount - 1)) / _modelHeight, 1f);
                else
                    _modelTransform.GetChild(i).localScale = Vector3.one;
            }
        }

#endif
    }
}
