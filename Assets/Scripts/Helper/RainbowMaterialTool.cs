#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RainbowMaterialTool : EditorWindow
{
    private enum Mode
    {
        SingleMaterial = 0,
        RainbowLoop = 1
    }

    private Mode _mode = Mode.SingleMaterial;

    private Material _singleMaterial;
    private List<Material> _rainbowMaterials = new List<Material>();

    private Vector2 _scroll;

    [MenuItem("Hichu/Tools/Rainbow Material Tool")]
    public static void Open()
    {
        var win = GetWindow<RainbowMaterialTool>("Material Tool");
        win.minSize = new Vector2(360f, 320f);
        win.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(8);

        _mode = (Mode)GUILayout.Toolbar((int)_mode, new[] { "1 Material", "Rainbow Loop" }, GUILayout.Height(24));

        GUILayout.Space(8);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        switch (_mode)
        {
            case Mode.SingleMaterial:
                DrawSingleMaterialUI();
                break;

            case Mode.RainbowLoop:
                DrawRainbowLoopUI();
                break;
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(12);

        GUI.enabled = Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        if (GUILayout.Button("Apply To Selected Objects", GUILayout.Height(32)))
        {
            ApplyToSelection();
        }
        GUI.enabled = true;

        GUILayout.Space(8);

        DrawSelectionInfo();
    }

    private void DrawSingleMaterialUI()
    {
        EditorGUILayout.LabelField("Replace all selected objects' material with this material:", EditorStyles.boldLabel);
        _singleMaterial = (Material)EditorGUILayout.ObjectField("Material", _singleMaterial, typeof(Material), false);

        EditorGUILayout.HelpBox(
            "Chọn nhiều GameObject trong Hierarchy rồi bấm Apply.\n" +
            "Tất cả Renderer / SkinnedMeshRenderer sẽ dùng cùng 1 material.",
            MessageType.Info
        );
    }

    private void DrawRainbowLoopUI()
    {
        EditorGUILayout.LabelField("Assign rainbow materials in sequence (looping):", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Ví dụ: list = [Đỏ, Cam, Vàng, Lục, Lam, Chàm, Tím].\n" +
            "Object 1 lấy Đỏ, Object 2 lấy Cam, ... Object 8 quay lại Đỏ.\n" +
            "Áp dụng vào toàn bộ Renderer / SkinnedMeshRenderer của từng object.",
            MessageType.Info
        );

        DrawRainbowList();
    }

    private void DrawRainbowList()
    {
        if (_rainbowMaterials == null)
            _rainbowMaterials = new List<Material>();

        int removeIndex = -1;

        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < _rainbowMaterials.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            _rainbowMaterials[i] = (Material)EditorGUILayout.ObjectField(
                $"Mat {i}",
                _rainbowMaterials[i],
                typeof(Material),
                false
            );

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0 && removeIndex < _rainbowMaterials.Count)
        {
            _rainbowMaterials.RemoveAt(removeIndex);
        }

        GUILayout.Space(4);

        if (GUILayout.Button("+ Add Material Slot", GUILayout.Height(22)))
        {
            _rainbowMaterials.Add(null);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSelectionInfo()
    {
        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            EditorGUILayout.HelpBox("No objects selected in Hierarchy.", MessageType.None);
            return;
        }

        EditorGUILayout.HelpBox($"Selected objects: {selected.Length}", MessageType.Info);
    }

    private void ApplyToSelection()
    {
        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("[RainbowMaterialTool] No objects selected.");
            return;
        }

        switch (_mode)
        {
            case Mode.SingleMaterial:
                ApplySingleMaterial(selected);
                break;

            case Mode.RainbowLoop:
                ApplyRainbowLoop(selected);
                break;
        }
    }

    private void ApplySingleMaterial(GameObject[] objects)
    {
        if (_singleMaterial == null)
        {
            Debug.LogWarning("[RainbowMaterialTool] No material assigned for SingleMaterial mode.");
            return;
        }

        int count = 0;

        foreach (var go in objects)
        {
            if (go == null) continue;

            var renderers = GetAllRenderers(go);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                Undo.RecordObject(r, "Apply Single Material");

                // gán 1 material duy nhất cho renderer
                r.sharedMaterials = CreateFilledArray(_singleMaterial, r.sharedMaterials.Length);

                EditorUtility.SetDirty(r);
                count++;
            }
        }

        Debug.Log($"[RainbowMaterialTool] Applied single material to {count} renderer(s).");
    }

    private void ApplyRainbowLoop(GameObject[] objects)
    {
        if (_rainbowMaterials == null || _rainbowMaterials.Count == 0)
        {
            Debug.LogWarning("[RainbowMaterialTool] Rainbow material list is empty.");
            return;
        }

        // lọc list để bỏ slot null (nhưng vẫn giữ thứ tự vòng)
        var pool = new List<Material>();
        foreach (var m in _rainbowMaterials)
        {
            if (m != null)
                pool.Add(m);
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning("[RainbowMaterialTool] All rainbow material slots are null.");
            return;
        }

        int matIndex = 0;
        int count = 0;

        // để có kết quả ổn định, sort theo tên trong Hierarchy (optional)
        var ordered = new List<GameObject>(objects);
        ordered.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        foreach (var go in ordered)
        {
            if (go == null) continue;

            var renderers = GetAllRenderers(go);

            foreach (var r in renderers)
            {
                if (r == null) continue;

                Undo.RecordObject(r, "Apply Rainbow Materials");

                var picked = pool[matIndex % pool.Count];

                r.sharedMaterials = CreateFilledArray(picked, r.sharedMaterials.Length);

                EditorUtility.SetDirty(r);
                count++;

                matIndex++;
            }
        }

        Debug.Log($"[RainbowMaterialTool] Applied rainbow loop to {count} renderer(s).");
    }

    private static Renderer[] GetAllRenderers(GameObject root)
    {
        // include cả root và con
        return root.GetComponentsInChildren<Renderer>(true);
    }

    private static Material[] CreateFilledArray(Material m, int length)
    {
        if (length <= 0) length = 1;
        var arr = new Material[length];
        for (int i = 0; i < length; i++)
            arr[i] = m;
        return arr;
    }
}
#endif
