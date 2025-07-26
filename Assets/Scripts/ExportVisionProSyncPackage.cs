// Assets/Editor/ExportVisionProSyncPackage.cs
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script to export all relevant assets as a .unitypackage.
/// </summary>
public class ExportVisionProSyncPackage
{
    private const string packagePath = "Assets/VisionProSyncApp.unitypackage";

    [MenuItem("Tools/Export VisionPro Sync Package")]
    public static void ExportPackage()
    {
        string[] assetPaths = new[]
        {
            "Assets/Scripts",
            "Assets/Prefabs",
            "Assets/Materials",
            "Assets/UI",
            "Assets/Plugins",
            "Assets/Plugins/visionos.speechbridge"
        };

        if (System.IO.File.Exists(packagePath))
            System.IO.File.Delete(packagePath);

        AssetDatabase.ExportPackage(assetPaths, packagePath, ExportPackageOptions.Recurse);
        Debug.Log($"[Export] Package created at: {packagePath}");
        EditorUtility.RevealInFinder(packagePath);
    }
}
