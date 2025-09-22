using UnityEngine;
using UnityEditor;
using System.IO;

public class ClearPackageCache : EditorWindow {
  [MenuItem("Tools/Clear Package Cache")]
  public static void ClearCache() {
    // Clear package cache
    string packageCachePath = Path.Combine(Application.dataPath, "../Library/PackageCache");
    if (Directory.Exists(packageCachePath)) {
      Directory.Delete(packageCachePath, true);
      Debug.Log("Package cache cleared");
    }

    // Clear Library folder
    string libraryPath = Path.Combine(Application.dataPath, "../Library");
    if (Directory.Exists(libraryPath)) {
      Directory.Delete(libraryPath, true);
      Debug.Log("Library folder cleared");
    }

    // Refresh project
    AssetDatabase.Refresh();
    EditorUtility.RequestScriptReload();
    
    Debug.Log("Package cache and Library cleared. Please restart Unity.");
  }
}
