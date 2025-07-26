// FilePickerUI.cs
using UnityEngine;
using System;
using SFB;  // StandaloneFileBrowser

/// <summary>
/// Minimal wrapper for file‐picker panel.
/// </summary>
public class FilePickerUI : MonoBehaviour
{
    [Tooltip("Panel shown during file picking")]
    public GameObject panel;

    /// <summary>
    /// Shows the open‐file dialog; returns first path via callback.
    /// </summary>
    public void Show(Action<string> callback)
    {
        if (panel != null) panel.SetActive(true);

        var extensions = new[] {
            new ExtensionFilter("JSON Files", "json"),
            new ExtensionFilter("All Files", "*")
        };

        StandaloneFileBrowser.OpenFilePanelAsync("Load Scene", "", extensions, false,
            (string[] paths) =>
            {
                if (panel != null) panel.SetActive(false);
                if (paths.Length > 0) callback(paths[0]);
            });
    }
}
