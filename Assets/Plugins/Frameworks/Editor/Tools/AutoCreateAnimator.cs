#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hichu.Editor
{
    // =========================================================
    // =============== PROJECT SETTINGS (trong 1 file) =========
    // =========================================================
    public class HichuAnimatorSettings : ScriptableObject
    {
        [Header("Output")]
        [Tooltip("Folder để lưu .controller (phải nằm trong Assets/)")]
        public string targetFolder = "Assets/Animators";

        [Header("Options")]
        [Tooltip("Giữ cấu trúc thư mục của Prefab bên dưới targetFolder.\nVD: Prefab ở Assets/Characters/Zombie/Z1.prefab => Controller ở Assets/Animators/Characters/Zombie/Z1.controller")]
        public bool mirrorPrefabFolders = true;

        public const string AssetPath = "Assets/Hichu/Settings/HichuAnimatorSettings.asset";

        public static HichuAnimatorSettings LoadOrCreate()
        {
            var asset = AssetDatabase.LoadAssetAtPath<HichuAnimatorSettings>(AssetPath);
            if (asset == null)
            {
                EnsureFolderRecursive(Path.GetDirectoryName(AssetPath).Replace("\\", "/"));
                asset = CreateInstance<HichuAnimatorSettings>();
                AssetDatabase.CreateAsset(asset, AssetPath);
                AssetDatabase.SaveAssets();
            }
            return asset;
        }

        internal static bool EnsureFolderRecursive(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;
            if (AssetDatabase.IsValidFolder(folderPath)) return true;

            if (!folderPath.StartsWith("Assets/") && folderPath != "Assets")
            {
                Debug.LogError("Folder phải nằm bên trong 'Assets/'.");
                return false;
            }

            var parts = folderPath.Split('/');
            var cur = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    var created = AssetDatabase.CreateFolder(cur, parts[i]);
                    if (string.IsNullOrEmpty(created)) return false;
                }
                cur = next;
            }
            return true;
        }
    }

    public static class HichuAnimatorSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Hichu/Animator", SettingsScope.Project)
            {
                label = "Animator",
                guiHandler = _ =>
                {
                    var s = HichuAnimatorSettings.LoadOrCreate();

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Hichu Animator Settings", EditorStyles.boldLabel);

                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        var curObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(s.targetFolder);
                        var newObj = (DefaultAsset)EditorGUILayout.ObjectField(
                            new GUIContent("Target Folder", "Folder để lưu AnimatorController .controller"),
                            curObj, typeof(DefaultAsset), false);

                        if (newObj != curObj)
                        {
                            var newPath = AssetDatabase.GetAssetPath(newObj);
                            if (string.IsNullOrEmpty(newPath) || !AssetDatabase.IsValidFolder(newPath))
                            {
                                EditorGUILayout.HelpBox("Vui lòng chọn 1 folder hợp lệ trong Project (nằm trong Assets/).", MessageType.Warning);
                            }
                            else
                            {
                                s.targetFolder = newPath;
                                EditorUtility.SetDirty(s);
                            }
                        }

                        var newPathStr = EditorGUILayout.TextField("Folder Path", s.targetFolder);
                        if (newPathStr != s.targetFolder)
                        {
                            if (string.IsNullOrEmpty(newPathStr) || AssetDatabase.IsValidFolder(newPathStr))
                            {
                                s.targetFolder = newPathStr;
                                EditorUtility.SetDirty(s);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Path không hợp lệ. Phải là folder nằm trong 'Assets/'.", MessageType.Warning);
                            }
                        }

                        s.mirrorPrefabFolders = EditorGUILayout.ToggleLeft(
                            new GUIContent("Mirror cấu trúc thư mục của Prefab", "Giữ cấu trúc thư mục Prefab bên dưới targetFolder"),
                            s.mirrorPrefabFolders);
                    }

                    EditorGUILayout.HelpBox("Thiết lập này sẽ dùng chung cho toàn dự án (file .asset được commit).", MessageType.Info);
                }
            };
            return provider;
        }
    }

    // =========================================================
    // ====================== MAIN TOOL ========================
    // =========================================================
    public class AutoCreateAnimator : EditorWindow
    {
        private const string Title = "Animator Auto Creator";
        [SerializeField] private DefaultAsset overrideFolderObj;
        [SerializeField] private bool overwriteIfExists = false;

        // MENU
        [MenuItem("Hichu/Animator/Animator Auto Creator %#A")]
        public static void Open()
        {
            var win = GetWindow<AutoCreateAnimator>(true, Title);
            win.minSize = new Vector2(460, 220);
            win.Show();
        }

        // CONTEXT MENU
        [MenuItem("Hichu/Create/Animator Controller for Prefab(s) and Assign", priority = 2010)]
        private static void ContextCreateAndAssign()
        {
            RunForSelection(GetEffectiveOutputFolder(), GetSettings().mirrorPrefabFolders, AskOverwriteIfNeeded());
        }

        [MenuItem("Hichu/Create/Animator Controller for Prefab(s) and Assign", true)]
        private static bool ValidateContextCreateAndAssign() => HasAnyPrefabAssetInSelection();

        [MenuItem("Assets/Create/Animator Controller for Prefab(s) and Assign", priority = 2010)]
        private static void ContextCreateAndAssign_Assets()
        {
            RunForSelection(GetEffectiveOutputFolder(), GetSettings().mirrorPrefabFolders, AskOverwriteIfNeeded());
        }

        [MenuItem("Assets/Create/Animator Controller for Prefab(s) and Assign", true)]
        private static bool ValidateContextCreateAndAssign_Assets() => HasAnyPrefabAssetInSelection();

        private static bool HasAnyPrefabAssetInSelection()
        {
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0) return false;
            foreach (var o in objs)
                if (o is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go))
                    return true;
            return false;
        }

        private void OnGUI()
        {
            var settings = GetSettings();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tạo Animator trùng tên Prefab và gắn lại vào Prefab", EditorStyles.boldLabel);

            string overridePath = GetFolderPathFromObject(overrideFolderObj);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Project Setting Folder:", settings.targetFolder, EditorStyles.miniLabel);
                settings.mirrorPrefabFolders = EditorGUILayout.ToggleLeft(
                    new GUIContent("Mirror cấu trúc thư mục của Prefab (global)", "Bật/tắt trong phiên này (sẽ lưu vào asset settings)"),
                    settings.mirrorPrefabFolders);

                if (GUI.changed)
                    EditorUtility.SetDirty(settings);

                overrideFolderObj = (DefaultAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Override Folder (tuỳ chọn - local)", "Nếu để trống, sẽ dùng Project Settings."),
                    overrideFolderObj, typeof(DefaultAsset), false);

                if (!string.IsNullOrEmpty(overridePath))
                    EditorGUILayout.LabelField("Save to (Override):", overridePath, EditorStyles.miniLabel);

                overwriteIfExists = EditorGUILayout.ToggleLeft("Overwrite nếu Animator đã tồn tại", overwriteIfExists);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Mở Project Settings…"))
                    SettingsService.OpenProjectSettings("Project/Hichu/Animator");

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(Selection.objects.Length == 0))
                {
                    if (GUILayout.Button($"Tạo & Gắn cho {Selection.objects.Length} prefab đã chọn"))
                    {
                        var baseFolder = !string.IsNullOrEmpty(overridePath) ? overridePath : settings.targetFolder;
                        if (!EnsureFolder(baseFolder))
                        {
                            EditorUtility.DisplayDialog(Title, "Folder không hợp lệ hoặc không tạo được.", "OK");
                        }
                        else
                        {
                            RunForSelection(baseFolder, settings.mirrorPrefabFolders, overwriteIfExists);
                        }
                    }
                }
            }

            if (GUILayout.Button("Mở folder xuất hiện tại trong Explorer/Finder"))
            {
                var path = !string.IsNullOrEmpty(overridePath) ? overridePath : settings.targetFolder;
                if (!string.IsNullOrEmpty(path))
                    EditorUtility.RevealInFinder(Path.GetFullPath(path));
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Chuột phải vào Prefab → Hichu → Create → Animator Controller for Prefab(s) and Assign.", MessageType.Info);
        }

        // ---------------- CORE ----------------
        private static void RunForSelection(string baseOutputFolder, bool mirrorStructure, bool overwrite)
        {
            if (!EnsureFolder(baseOutputFolder))
            {
                EditorUtility.DisplayDialog(Title, "Folder không hợp lệ hoặc không tạo được.", "OK");
                return;
            }

            var objs = Selection.objects;
            int done = 0, skipped = 0, errors = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var obj in objs)
                {
                    if (obj is not GameObject go || !PrefabUtility.IsPartOfPrefabAsset(go))
                    {
                        skipped++;
                        continue;
                    }

                    if (ProcessPrefab(go, baseOutputFolder, mirrorStructure, overwrite))
                        done++;
                    else
                        errors++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog(Title,
                $"Hoàn tất.\nTạo & gắn thành công: {done}\nBỏ qua: {skipped}\nLỗi: {errors}",
                "OK");
        }

        private static bool ProcessPrefab(GameObject prefabAsset, string baseOutputFolder, bool mirrorStructure, bool overwrite)
        {
            try
            {
                var prefabName = prefabAsset.name;
                var prefabAssetPath = AssetDatabase.GetAssetPath(prefabAsset);
                var controllerPath = BuildControllerPath(prefabAssetPath, baseOutputFolder, prefabName, mirrorStructure);

                var controllerDir = Path.GetDirectoryName(controllerPath).Replace("\\", "/");
                if (!EnsureFolder(controllerDir))
                {
                    Debug.LogError($"[AnimatorAuto] Không tạo được folder đích: {controllerDir}");
                    return false;
                }

                AnimatorController controller;

                if (File.Exists(controllerPath))
                {
                    if (!overwrite)
                    {
                        controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                        if (controller == null)
                        {
                            Debug.LogError($"[AnimatorAuto] Không load được Animator tại: {controllerPath}");
                            return false;
                        }
                    }
                    else
                    {
                        AssetDatabase.DeleteAsset(controllerPath);
                        controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                    }
                }
                else
                {
                    controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                }

                if (controller == null)
                {
                    Debug.LogError($"[AnimatorAuto] Tạo Animator thất bại: {controllerPath}");
                    return false;
                }

                var contents = PrefabUtility.LoadPrefabContents(prefabAssetPath);
                if (contents == null)
                {
                    Debug.LogError($"[AnimatorAuto] LoadPrefabContents thất bại: {prefabAssetPath}");
                    return false;
                }

                bool success = true;
                try
                {
                    var animator = contents.GetComponent<Animator>() ?? contents.AddComponent<Animator>();
                    animator.runtimeAnimatorController = controller;

                    if (controller.layers.Length > 0 && controller.layers[0].stateMachine.states.Length == 0)
                    {
                        var state = controller.layers[0].stateMachine.AddState("Idle");
                        controller.layers[0].stateMachine.defaultState = state;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    success = false;
                }
                finally
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabAssetPath);
                    PrefabUtility.UnloadPrefabContents(contents);
                }

                if (success)
                    Debug.Log($"[AnimatorAuto] ✅ {prefabName}: tạo & gắn '{controllerPath}'");

                return success;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AnimatorAuto] Lỗi với '{prefabAsset?.name}': {ex.Message}");
                return false;
            }
        }

        // ---------------- UTILS ----------------
        private static string BuildControllerPath(string prefabPath, string baseFolder, string name, bool mirror)
        {
            baseFolder = baseFolder.Replace("\\", "/");
            var relDir = string.Empty;

            if (mirror)
            {
                var prefabDir = Path.GetDirectoryName(prefabPath).Replace("\\", "/");
                if (prefabDir.StartsWith("Assets/"))
                    relDir = prefabDir.Substring("Assets/".Length);
            }

            var outDir = string.IsNullOrEmpty(relDir) ? baseFolder : $"{baseFolder}/{relDir}";
            return $"{outDir}/{name}.controller".Replace("\\", "/");
        }

        private static HichuAnimatorSettings GetSettings() => HichuAnimatorSettings.LoadOrCreate();

        private static string GetEffectiveOutputFolder()
        {
            var s = GetSettings();
            return string.IsNullOrEmpty(s.targetFolder) ? "Assets/Animators" : s.targetFolder;
        }

        private static string GetFolderPathFromObject(DefaultAsset folderObj)
        {
            if (folderObj == null) return null;
            var path = AssetDatabase.GetAssetPath(folderObj);
            return AssetDatabase.IsValidFolder(path) ? path : null;
        }

        private static bool EnsureFolder(string folderPath)
        {
            return HichuAnimatorSettings.EnsureFolderRecursive(folderPath);
        }

        private static bool AskOverwriteIfNeeded()
        {
            return EditorUtility.DisplayDialog(
                Title,
                "Overwrite Animator nếu đã tồn tại?",
                "Overwrite", "Giữ nguyên");
        }
    }
}
#endif
