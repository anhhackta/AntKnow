using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Project Cleaner - Xóa các file không cần thiết cho server build
/// Menu: Tools → Clean Server Project
/// </summary>
public class ProjectCleaner : EditorWindow
{
    private bool confirmCleanup = false;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Clean Server Project 🧹")]
    public static void ShowWindow()
    {
        GetWindow<ProjectCleaner>("Project Cleaner");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("🧹 PROJECT CLEANER", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Tool này sẽ xóa các file/folder không cần thiết cho server build:\n\n" +
            "❌ Scenes: LoginScene, MenuScene\n" +
            "❌ Scripts: UI, Presentation, Client\n" +
            "❌ Assets: Art, Audio, Animations, Materials\n" +
            "❌ Packages: TextMeshPro, UI Toolkit, VFX, Shader Graph\n\n" +
            "✅ Giữ lại: GameScene, Server scripts, Domain layer, NetworkPlayer prefab",
            MessageType.Warning
        );

        GUILayout.Space(10);

        confirmCleanup = EditorGUILayout.Toggle("Tôi hiểu và muốn tiếp tục", confirmCleanup);

        GUILayout.Space(10);

        GUI.enabled = confirmCleanup;
        if (GUILayout.Button("🧹 CLEAN PROJECT (KHÔNG THỂ UNDO!)", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Xác nhận cleanup",
                "Bạn có chắc chắn muốn xóa các file không cần thiết?\n\nHành động này KHÔNG THỂ UNDO!",
                "Có, xóa đi",
                "Hủy"))
            {
                CleanProject();
            }
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (GUILayout.Button("📋 Preview Files to Delete", GUILayout.Height(30)))
        {
            PreviewFilesToDelete();
        }

        EditorGUILayout.EndScrollView();
    }

    private void PreviewFilesToDelete()
    {
        Debug.Log("========== FILES TO DELETE ==========");

        // Scenes
        Debug.Log("\n=== SCENES ===");
        CheckAndLog("Assets/Scenes/LoginScene.unity");
        CheckAndLog("Assets/Scenes/MenuScene.unity");

        // Scripts
        Debug.Log("\n=== SCRIPTS ===");
        CheckAndLog("Assets/Script/UI");
        CheckAndLog("Assets/Script/Presentation");
        CheckAndLog("Assets/Script/Client");

        // Assets
        Debug.Log("\n=== ASSETS ===");
        CheckAndLog("Assets/Art");
        CheckAndLog("Assets/Audio");
        CheckAndLog("Assets/Animations");
        CheckAndLog("Assets/Materials");
        CheckAndLog("Assets/Prefabs/UI");

        Debug.Log("\n========== END PREVIEW ==========");
        Debug.Log("Total files/folders to delete: Check log above");
    }

    private void CheckAndLog(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            Debug.Log($"❌ [FOLDER] {path}");
        }
        else if (File.Exists(path))
        {
            Debug.Log($"❌ [FILE] {path}");
        }
        else
        {
            Debug.Log($"⚠️ [NOT FOUND] {path}");
        }
    }

    private void CleanProject()
    {
        Debug.Log("========== STARTING PROJECT CLEANUP ==========");
        int deletedCount = 0;

        // Delete scenes
        deletedCount += DeleteAsset("Assets/Scenes/LoginScene.unity");
        deletedCount += DeleteAsset("Assets/Scenes/MenuScene.unity");

        // Delete script folders
        deletedCount += DeleteAsset("Assets/Script/UI");
        deletedCount += DeleteAsset("Assets/Script/Presentation");
        deletedCount += DeleteAsset("Assets/Script/Client");

        // Delete asset folders
        deletedCount += DeleteAsset("Assets/Art");
        deletedCount += DeleteAsset("Assets/Audio");
        deletedCount += DeleteAsset("Assets/Animations");
        deletedCount += DeleteAsset("Assets/Materials");
        deletedCount += DeleteAsset("Assets/Prefabs/UI");

        // Refresh
        AssetDatabase.Refresh();

        Debug.Log($"========== CLEANUP COMPLETE ==========");
        Debug.Log($"✅ Deleted {deletedCount} items");
        Debug.Log($"Project size reduced significantly!");

        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"✅ Deleted {deletedCount} items\n\nProject is now clean for server build!",
            "OK"
        );
    }

    private int DeleteAsset(string path)
    {
        if (AssetDatabase.IsValidFolder(path) || File.Exists(path))
        {
            if (AssetDatabase.DeleteAsset(path))
            {
                Debug.Log($"✅ Deleted: {path}");
                return 1;
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to delete: {path}");
                return 0;
            }
        }
        else
        {
            Debug.Log($"⏭️ Skipped (not found): {path}");
            return 0;
        }
    }
}

