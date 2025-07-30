// ObjectManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/// <summary>
/// Handles spawn/update/delete/clear commands for any object type.
/// Also provides helper methods for local instantiation and selection.
/// </summary>
public class ObjectManager : MonoBehaviour
{
    [Header("Defaults")]
    [Tooltip("Fallback material if MaterialLibrary lookup fails")]
    public Material defaultMaterial;

    [Tooltip("Assign prefabs by type name (e.g. name the Cube prefab 'Cube')")]
    public List<GameObject> prefabList;

    [Header("Spawn Settings")]
    [Tooltip("Index of the default prefab to spawn (0 = first in list)")]
    public int defaultPrefabIndex = 0;

    // Map typeName -> prefab
    private Dictionary<string, GameObject> prefabsByType;

    // Active objects: id → GameObject
    private readonly Dictionary<string, GameObject> objects = new();

    /// <summary>
    /// Expose current objects for saving/loading.
    /// </summary>
    public IReadOnlyDictionary<string, GameObject> AllObjects => objects;

    void Awake()
    {
        // Build prefab lookup
        prefabsByType = new Dictionary<string, GameObject>();
        foreach (var go in prefabList)
        {
            if (go == null) continue;
            prefabsByType[go.name] = go;
        }

        // Validate default prefab index
        if (prefabList.Count == 0)
        {
            Debug.LogError("[ObjectManager] No prefabs assigned in prefabList!");
        }
        else if (defaultPrefabIndex >= prefabList.Count)
        {
            Debug.LogWarning($"[ObjectManager] Default prefab index {defaultPrefabIndex} is out of range. Using index 0.");
            defaultPrefabIndex = 0;
        }
    }

    /// <summary>
    /// Locally spawn an object at the given position using the default prefab.
    /// Returns the generated GUID for network sync.
    /// </summary>
    public string SpawnObject(Vector3 position)
    {
        return SpawnObject(position, defaultPrefabIndex);
    }

    /// <summary>
    /// Locally spawn an object at the given position using the specified prefab index.
    /// Returns the generated GUID for network sync.
    /// </summary>
    public string SpawnObject(Vector3 position, int prefabIndex)
    {
        // Validate prefab index
        if (prefabIndex < 0 || prefabIndex >= prefabList.Count)
        {
            Debug.LogError($"[ObjectManager] Invalid prefab index: {prefabIndex}. Using default index {defaultPrefabIndex}.");
            prefabIndex = defaultPrefabIndex;
        }

        // Get the prefab
        GameObject prefab = prefabList[prefabIndex];
        if (prefab == null)
        {
            Debug.LogError($"[ObjectManager] Prefab at index {prefabIndex} is null.");
            return null;
        }

        // Generate unique ID
        string id = Guid.NewGuid().ToString();

        // Instantiate locally
        var go = Instantiate(prefab, position, Quaternion.identity);
        go.name = $"{prefab.name}_{id}";
        objects[id] = go;

        Debug.Log($"[ObjectManager] Spawned {prefab.name} at {position} with ID {id}");
        return id;
    }

    /// <summary>
    /// Locally spawn an object at the given position using the specified prefab name.
    /// Returns the generated GUID for network sync.
    /// </summary>
    public string SpawnObject(Vector3 position, string prefabName)
    {
        // Find the prefab by name
        if (!prefabsByType.TryGetValue(prefabName, out var prefab))
        {
            Debug.LogError($"[ObjectManager] Cannot spawn {prefabName}, prefab not found. Using default prefab.");
            return SpawnObject(position); // Fallback to default
        }

        // Generate unique ID
        string id = Guid.NewGuid().ToString();

        // Instantiate locally
        var go = Instantiate(prefab, position, Quaternion.identity);
        go.name = $"{prefabName}_{id}";
        objects[id] = go;

        Debug.Log($"[ObjectManager] Spawned {prefabName} at {position} with ID {id}");
        return id;
    }

    /// <summary>
    /// Legacy method for backward compatibility - now uses default prefab.
    /// </summary>
    [System.Obsolete("Use SpawnObject() instead. This method will be removed in a future version.")]
    public string SpawnCube(Vector3 position)
    {
        Debug.LogWarning("[ObjectManager] SpawnCube is deprecated. Use SpawnObject() instead.");
        return SpawnObject(position);
    }

    /// <summary>
    /// Find the closest object within maxDistance.
    /// Returns (GameObject, id) or (null, null) if none.
    /// </summary>
    public (GameObject, string) FindClosest(Vector3 point, float maxDistance)
    {
        GameObject bestGo = null;
        string bestId = null;
        float bestDist = maxDistance;

        foreach (var kv in objects)
        {
            float d = Vector3.Distance(point, kv.Value.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestGo = kv.Value;
                bestId = kv.Key;
            }
        }

        return (bestGo, bestId);
    }

    /// <summary>
    /// Entry point for all network messages.
    /// </summary>
    public void HandleMessage(NetworkMessage msg)
    {
        if (msg == null || string.IsNullOrEmpty(msg.action))
        {
            Debug.LogWarning("[ObjectManager] Received invalid message.");
            return;
        }

        switch (msg.action)
        {
            case "spawn":  DoSpawn(msg);  break;
            case "update": DoUpdate(msg); break;
            case "delete": DoDelete(msg); break;
            case "clear":  DoClear();     break;
            default:
                Debug.LogWarning($"[ObjectManager] Unhandled action: {msg.action}");
                break;
        }
    }

    private void DoSpawn(NetworkMessage msg)
    {
        if (objects.ContainsKey(msg.id)) return;

        if (!prefabsByType.TryGetValue(msg.objectType, out var prefab))
        {
            // Fallback to default prefab if specified type not found
            if (prefabList.Count > 0 && defaultPrefabIndex < prefabList.Count)
            {
                prefab = prefabList[defaultPrefabIndex];
                Debug.LogWarning($"[ObjectManager] Prefab '{msg.objectType}' not found, using default prefab '{prefab.name}'.");
            }
            else
            {
                Debug.LogError($"[ObjectManager] No prefab for type '{msg.objectType}' and no default prefab available.");
                return;
            }
        }

        var go = Instantiate(prefab);
        go.name = $"{msg.objectType}_{msg.id}";
        objects[msg.id] = go;

        ApplyParameters(go, msg.parameters);
    }

    private void DoUpdate(NetworkMessage msg)
    {
        if (objects.TryGetValue(msg.id, out var go))
            ApplyParameters(go, msg.parameters);
        else
            Debug.LogWarning($"[ObjectManager] Update failed, id '{msg.id}' not found.");
    }

    private void DoDelete(NetworkMessage msg)
    {
        if (objects.TryGetValue(msg.id, out var go))
        {
            Destroy(go);
            objects.Remove(msg.id);
        }
    }

    private void DoClear()
    {
        foreach (var kv in objects)
            Destroy(kv.Value);
        objects.Clear();
    }

    private void ApplyParameters(GameObject go, JObject p)
    {
        if (p == null) return;

        // Position
        if (p["position"] is JArray pos && pos.Count == 3)
        {
            go.transform.position = new Vector3(
                pos[0].Value<float>(),
                pos[1].Value<float>(),
                pos[2].Value<float>()
            );
        }
        
        // Rotation (new support for enhanced gestures)
        if (p["rotation"] is JArray rot && rot.Count == 4)
        {
            go.transform.rotation = new Quaternion(
                rot[0].Value<float>(),
                rot[1].Value<float>(),
                rot[2].Value<float>(),
                rot[3].Value<float>()
            );
        }
        
        // Scale
        if (p["scale"] is JArray scl && scl.Count == 3)
        {
            go.transform.localScale = new Vector3(
                scl[0].Value<float>(),
                scl[1].Value<float>(),
                scl[2].Value<float>()
            );
        }
        
        // Color
        if (p["color"] is JObject col)
        {
            if (go.TryGetComponent<Renderer>(out var rend))
            {
                rend.material.color = new Color(
                    col["r"].Value<float>(),
                    col["g"].Value<float>(),
                    col["b"].Value<float>(),
                    col["a"].Value<float>()
                );
            }
        }
        
        // Material by name
        if (p["materialName"] != null && go.TryGetComponent<Renderer>(out var rend2))
        {
            string matName = p["materialName"].Value<string>();
            var mat = MaterialLibrary.Instance.Get(matName);
            rend2.material = mat != null ? mat : defaultMaterial;
        }
    }
}
