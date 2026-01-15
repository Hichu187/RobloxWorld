#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class TextureRenameTool : EditorWindow
{
    private string suffix = "new";

    [MenuItem("Tools/Texture/Rename With Suffix")]
    private static void Open()
    {
        GetWindow<TextureRenameTool>("Texture Rename");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Selected Textures", EditorStyles.boldLabel);

        suffix = EditorGUILayout.TextField("Suffix", suffix);

        GUILayout.Space(10);

        if (GUILayout.Button("Rename Selected Textures"))
        {
            RenameSelectedTextures();
        }
    }

    private void RenameSelectedTextures()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning("No textures selected.");
            return;
        }

        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path))
                continue;

            string name = tex.name;
            string newName = name.EndsWith($"_{suffix}") ? name : $"{name}_{suffix}";

            AssetDatabase.RenameAsset(path, newName);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif