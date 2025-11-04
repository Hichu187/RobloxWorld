#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ReplaceWithPrefabTool : EditorWindow
{
    [Serializable]
    private class ReplaceOptions
    {
        public bool useWorldTransform = true;
        public bool keepName = true;
        public bool keepLayerAndTag = true;
        public bool keepStaticFlags = true;
        public bool keepActiveSelf = true;
        public bool moveChildren = true;
        public bool includeInactiveInSelection = false;
    }

    private GameObject _prefab;
    private ReplaceOptions _opts = new ReplaceOptions();
    private string _nameFilter = "";
    private bool _filterExact = false;

    [MenuItem("Tools/Replace With Prefab")]
    public static void Open() => GetWindow<ReplaceWithPrefabTool>("Replace With Prefab");

    private void OnGUI()
    {
        EditorGUILayout.Space();
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        _opts.useWorldTransform = EditorGUILayout.ToggleLeft("Match World Transform", _opts.useWorldTransform);
        _opts.keepName = EditorGUILayout.ToggleLeft("Keep Name", _opts.keepName);
        _opts.keepLayerAndTag = EditorGUILayout.ToggleLeft("Keep Layer & Tag", _opts.keepLayerAndTag);
        _opts.keepStaticFlags = EditorGUILayout.ToggleLeft("Keep Static Flags", _opts.keepStaticFlags);
        _opts.keepActiveSelf = EditorGUILayout.ToggleLeft("Keep Active Self", _opts.keepActiveSelf);
        _opts.moveChildren = EditorGUILayout.ToggleLeft("Move Children To New", _opts.moveChildren);
        _opts.includeInactiveInSelection = EditorGUILayout.ToggleLeft("Include Inactive In Selection", _opts.includeInactiveInSelection);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Filter By Name (optional)", EditorStyles.boldLabel);
        _nameFilter = EditorGUILayout.TextField("Contains / Equals", _nameFilter);
        _filterExact = EditorGUILayout.ToggleLeft("Exact Match", _filterExact);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(_prefab == null))
        {
            if (GUILayout.Button("Replace Selected"))
            {
                var targets = GetSelection(_opts.includeInactiveInSelection);
                ReplaceMany(targets, _prefab, _opts);
            }

            if (GUILayout.Button("Replace All In Scene (by Name Filter if set)"))
            {
                var all = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(g =>
                    {
                        if (!IsValidSceneObject(g)) return false;
                        if (StageUtility.GetStageHandle(g) != StageUtility.GetMainStageHandle()) return false;
                        if (PrefabUtility.IsPartOfPrefabAsset(g)) return false;
                        if (string.IsNullOrEmpty(_nameFilter)) return true;
                        return _filterExact ? g.name == _nameFilter : g.name.IndexOf(_nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToArray();

                ReplaceMany(all, _prefab, _opts);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Chọn các object trong Hierarchy rồi bấm Replace Selected, hoặc dùng Replace All In Scene kèm filter theo tên.", MessageType.Info);
    }

    private static GameObject[] GetSelection(bool includeInactive)
    {
        var objs = includeInactive
            ? Selection.gameObjects.Concat(Selection.GetFiltered<GameObject>(SelectionMode.Deep)).Distinct().ToArray()
            : Selection.gameObjects;

        return objs.Where(IsValidSceneObject).ToArray();
    }

    private static bool IsValidSceneObject(GameObject go)
    {
        if (go == null) return false;
        if (EditorUtility.IsPersistent(go)) return false;
        if (PrefabUtility.IsPartOfPrefabAsset(go)) return false;
        return true;
    }

    private static void ReplaceMany(GameObject[] targets, GameObject prefab, ReplaceOptions opts)
    {
        if (prefab == null || targets == null || targets.Length == 0) return;

        var scene = targets[0].scene;
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Array.Sort(targets, (a, b) => TransformHierarchyIndex(a.transform).CompareTo(TransformHierarchyIndex(b.transform)));

            int total = targets.Length;
            for (int i = 0; i < total; i++)
            {
                var oldGo = targets[i];
                if (oldGo == null) continue;

                if (EditorUtility.DisplayCancelableProgressBar("Replacing...", oldGo.name, (float)i / total))
                    break;

                ReplaceOne(oldGo, prefab, opts);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    private static int TransformHierarchyIndex(Transform t)
    {
        int idx = 0;
        while (t != null)
        {
            idx = (idx * 397) ^ t.GetSiblingIndex();
            t = t.parent;
        }
        return idx;
    }

    private static void ReplaceOne(GameObject oldGo, GameObject prefab, ReplaceOptions opts)
    {
        if (oldGo == null) return;

        var parent = oldGo.transform.parent;
        int sibling = oldGo.transform.GetSiblingIndex();

        var worldPos = oldGo.transform.position;
        var worldRot = oldGo.transform.rotation;
        var worldScale = oldGo.transform.lossyScale;

        var localPos = oldGo.transform.localPosition;
        var localRot = oldGo.transform.localRotation;
        var localScale = oldGo.transform.localScale;

        string name = oldGo.name;
        int layer = oldGo.layer;
        string tag = oldGo.tag;
        var flags = GameObjectUtility.GetStaticEditorFlags(oldGo);
        bool activeSelf = oldGo.activeSelf;

        // ✅ KHÔNG dùng StageHandle nữa
        GameObject newObjObj = null;
        if (parent != null)
        {
            // Overload theo parent Transform (ổn định trên mọi version)
            newObjObj = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        }
        else
        {
            // Overload theo Scene khi không có parent
            newObjObj = PrefabUtility.InstantiatePrefab(prefab, oldGo.scene) as GameObject;
        }

        if (newObjObj == null) return;

        Undo.RegisterCreatedObjectUndo(newObjObj, "Instantiate Prefab");

        // Nếu dùng overload theo Scene, cần set parent sau khi tạo
        if (parent == null)
        {
            newObjObj.transform.SetSiblingIndex(0); // tạm
        }
        else
        {
            // đã được parent lúc instantiate
            newObjObj.transform.SetSiblingIndex(
                Mathf.Clamp(sibling, 0, (newObjObj.transform.parent?.childCount ?? 1) - 1)
            );
        }

        if (opts.useWorldTransform)
        {
            newObjObj.transform.SetPositionAndRotation(worldPos, worldRot);
            newObjObj.transform.localScale = ComputeLocalScaleToMatchWorld(newObjObj.transform.parent, worldScale);
        }
        else
        {
            if (parent != null)
            {
                newObjObj.transform.localPosition = localPos;
                newObjObj.transform.localRotation = localRot;
                newObjObj.transform.localScale = localScale;
                newObjObj.transform.SetSiblingIndex(sibling);
            }
            else
            {
                // không có parent thì local == world
                newObjObj.transform.SetPositionAndRotation(worldPos, worldRot);
                newObjObj.transform.localScale = worldScale;
            }
        }

        if (opts.keepName) newObjObj.name = name;
        if (opts.keepLayerAndTag)
        {
            newObjObj.layer = layer;
            try { newObjObj.tag = tag; } catch { }
        }
        if (opts.keepStaticFlags) GameObjectUtility.SetStaticEditorFlags(newObjObj, flags);
        if (opts.keepActiveSelf) newObjObj.SetActive(activeSelf);

        if (opts.moveChildren)
        {
            var buffer = oldGo.transform.Cast<Transform>().ToArray();
            foreach (var child in buffer)
                Undo.SetTransformParent(child, newObjObj.transform, "Move Children");
        }

        Undo.DestroyObjectImmediate(oldGo);
        EditorUtility.SetDirty(newObjObj);
    }



    private static Vector3 ComputeLocalScaleToMatchWorld(Transform parent, Vector3 desiredWorld)
    {
        if (parent == null) return desiredWorld;

        Vector3 pls = parent.lossyScale;
        return new Vector3(
            SafeDiv(desiredWorld.x, pls.x),
            SafeDiv(desiredWorld.y, pls.y),
            SafeDiv(desiredWorld.z, pls.z)
        );
    }

    private static float SafeDiv(float a, float b) => Mathf.Approximately(b, 0f) ? a : a / b;

    [MenuItem("GameObject/Replace With Prefab...", false, 0)]
    private static void ContextMenuReplace()
    {
        var win = GetWindow<ReplaceWithPrefabTool>("Replace With Prefab");
        win.Show();
    }
}
#endif
