// MaterialLibrary.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton registry for Materials, accessible by name.
/// Populate the 'materials' list in the Inspector.
/// </summary>
public class MaterialLibrary : MonoBehaviour
{
    // Static instance
    public static MaterialLibrary Instance { get; private set; }

    [Header("Assign all materials you want to expose here")]
    [Tooltip("Drag & drop Materials here; their names will be used as keys.")]
    public List<Material> materials = new List<Material>();

    // Internal lookup
    private Dictionary<string, Material> materialDict;

    void Awake()
    {
        // Enforce singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build lookup dictionary
        materialDict = new Dictionary<string, Material>(materials.Count);
        foreach (var mat in materials)
        {
            if (mat == null) continue;
            var key = mat.name;
            if (!materialDict.ContainsKey(key))
                materialDict.Add(key, mat);
            else
                Debug.LogWarning($"[MaterialLibrary] Duplicate material name '{key}' in list.");
        }
    }

    /// <summary>
    /// Get a Material by its name. Returns null if not found.
    /// </summary>
    public Material Get(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
            return null;

        if (materialDict != null && materialDict.TryGetValue(materialName, out var mat))
            return mat;

        Debug.LogWarning($"[MaterialLibrary] Material '{materialName}' not found.");
        return null;
    }

    /// <summary>
    /// Optional: add or override a material at runtime.
    /// </summary>
    public void Register(string materialName, Material mat)
    {
        if (string.IsNullOrEmpty(materialName) || mat == null)
        {
            Debug.LogError("[MaterialLibrary] Invalid parameters for Register().");
            return;
        }

        materialDict[materialName] = mat;
    }
}
