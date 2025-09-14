using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CharacterInteract : MonoBehaviour
    {
        [Title("Clone Settings")]
        [SerializeField] private int cloneLayer = 0;
        [SerializeField] private Transform clonePos;

        [Title("Ghost Settings")]
        [SerializeField] private bool useGhost = true;
        [SerializeField, Range(0.05f, 0.95f)] private float ghostAlpha = 0.35f;
        [SerializeField] private bool includeChildren = true;
        [SerializeField] private KeyCode cancelKey = KeyCode.Space;

        // cache
        private PlayerGUI _gui;
        private FieldOfView _fov;

        // runtime
        private GameObject _clone;
        private Transform _nearestAtStart;
        private bool _ghostActive;
        private bool _btnVisible;

        // ghost state (tối giản)
        private readonly List<MatState> _saved = new(16);

        private struct MatState
        {
            public Material mat;
            public Color color;
            public int renderQueue;
            public bool alphaBlendOn;
            public bool alphaTestOn;
            public float? surface, zwrite, srcBlend, dstBlend;
        }

        private void Awake()
        {
            _gui = Player.Instance.gui;
            _fov = Player.Instance.character.fov;

            var interact = _gui?.interactiveBtn;
            if (interact != null)
            {
                interact.eventDown += OnInteractDown;
                interact.eventUp += OnInteractUp;
            }
        }

        private void OnDestroy()
        {
            var interact = _gui?.interactiveBtn;
            if (interact != null)
            {
                interact.eventDown -= OnInteractDown;
                interact.eventUp -= OnInteractUp;
            }
            if (_ghostActive) RestoreGhost();
        }

        private void Update()
        {
            bool hasTarget = _fov != null && _fov.nearestInteractable != null;
            if (hasTarget != _btnVisible)
            {
                _btnVisible = hasTarget;
                if (_gui?.interactiveBtn != null)
                    _gui.interactiveBtn.gameObject.SetActive(_btnVisible);
            }

            if (useGhost && _ghostActive && Input.GetKeyDown(cancelKey))
            {
                CancelGhostAndClone();
            }
        }

        private void OnInteractDown()
        {
            if (_fov == null || _fov.nearestInteractable == null) return;

            if (_clone != null)
            {
                Destroy(_clone);
                _clone = null;
            }

            _nearestAtStart = _fov.nearestInteractable;

            var src = _nearestAtStart.gameObject;
            _clone = Instantiate(src);
            _clone.name = src.name + "_Clone";

            if (clonePos != null)
            {
                var t = _clone.transform;
                t.SetParent(clonePos, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }

            if (useGhost && !_ghostActive)
            {
                ApplyGhost(_nearestAtStart.gameObject, ghostAlpha, includeChildren);
                _ghostActive = true;
            }

            SetLayerRecursively(_clone, cloneLayer);

            var cols = _clone.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                Destroy(cols[i]);
            }
        }

        private void OnInteractUp()
        {

        }

        private void ApplyGhost(GameObject target, float alpha, bool children)
        {
            RestoreGhost();

            if (target == null) return;
            var renderers = children
                ? target.GetComponentsInChildren<Renderer>(true)
                : target.GetComponents<Renderer>();

            for (int r = 0; r < renderers.Length; r++)
            {
                var rd = renderers[r];
                if (rd == null) continue;

                var mats = rd.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;

                    var st = new MatState
                    {
                        mat = m,
                        color = m.HasProperty("_Color") ? m.color : Color.white,
                        renderQueue = m.renderQueue,
                        alphaBlendOn = m.IsKeywordEnabled("_ALPHABLEND_ON"),
                        alphaTestOn = m.IsKeywordEnabled("_ALPHATEST_ON"),
                        surface = m.HasProperty("_Surface") ? m.GetFloat("_Surface") : null,
                        zwrite = m.HasProperty("_ZWrite") ? m.GetFloat("_ZWrite") : null,
                        srcBlend = m.HasProperty("_SrcBlend") ? m.GetFloat("_SrcBlend") : null,
                        dstBlend = m.HasProperty("_DstBlend") ? m.GetFloat("_DstBlend") : null
                    };
                    _saved.Add(st);

                    if (m.HasProperty("_Color"))
                    {
                        var c = m.color; c.a = alpha; m.color = c;
                    }
                    if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f); // URP Transparent
                    if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
                    if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }

                rd.materials = mats;
            }
        }

        private void RestoreGhost()
        {
            if (_saved.Count == 0) return;

            for (int i = 0; i < _saved.Count; i++)
            {
                var s = _saved[i];
                if (s.mat == null) continue;

                if (s.mat.HasProperty("_Color")) s.mat.color = s.color;
                if (s.surface.HasValue && s.mat.HasProperty("_Surface")) s.mat.SetFloat("_Surface", s.surface.Value);
                if (s.zwrite.HasValue && s.mat.HasProperty("_ZWrite")) s.mat.SetFloat("_ZWrite", s.zwrite.Value);
                if (s.srcBlend.HasValue && s.mat.HasProperty("_SrcBlend")) s.mat.SetFloat("_SrcBlend", s.srcBlend.Value);
                if (s.dstBlend.HasValue && s.mat.HasProperty("_DstBlend")) s.mat.SetFloat("_DstBlend", s.dstBlend.Value);

                if (!s.alphaBlendOn) s.mat.DisableKeyword("_ALPHABLEND_ON");
                if (s.alphaTestOn) s.mat.EnableKeyword("_ALPHATEST_ON");
                else s.mat.DisableKeyword("_ALPHATEST_ON");

                s.mat.renderQueue = s.renderQueue;
            }

            _saved.Clear();
        }

        private void CancelGhostAndClone()
        {
            RestoreGhost();

            if (_clone != null)
            {
                Destroy(_clone);
                _clone = null;
            }

            _ghostActive = false;
            _nearestAtStart = null;
        }


        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            var t = obj.transform;
            for (int i = 0, c = t.childCount; i < c; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }
}