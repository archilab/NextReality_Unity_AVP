// SceneDataHandler.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

/// <summary>
/// Saves/loads scene state and broadcasts those actions network‑wide.
/// </summary>
public class SceneDataHandler : MonoBehaviour
{
    [Tooltip("Manages object creation/updating")]
    public ObjectManager objectManager;

    [Tooltip("Handles WebSocket messaging")]
    public AsyncNetworkManager networkManager;

    [Tooltip("Filename in persistentDataPath")]
    public string filename = "scene.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, filename);

    // Helper classes for JSON serialization
    [System.Serializable]
    public class CubeData
    {
        public string id;
        public float[] pos;
        public float[] scale;
    }

    [System.Serializable]
    public class SceneData
    {
        public List<CubeData> cubes;
    }

    /// <summary>
    /// Locally save current scene state to JSON.
    /// </summary>
    public void SaveScene()
    {
        var sd = new SceneData { cubes = new List<CubeData>() };
        foreach (var kv in objectManager.AllObjects)
        {
            var go = kv.Value;
            sd.cubes.Add(new CubeData {
                id    = kv.Key,
                pos   = new[] { go.transform.position.x, go.transform.position.y, go.transform.position.z },
                scale = new[] { go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z }
            });
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(sd, true));
        Debug.Log($"[SceneData] Scene saved to {SavePath}");
    }

    /// <summary>
    /// Clear and broadcast clear, then spawn + broadcast each cube from JSON.
    /// </summary>
    public void LoadSceneNetworked()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"[SceneData] No file at {SavePath}");
            return;
        }

        ClearSceneNetworked();

        string json = File.ReadAllText(SavePath);
        var sd = JsonUtility.FromJson<SceneData>(json);

        foreach (var cd in sd.cubes)
        {
            var p = new JObject {
                ["position"] = new JArray(cd.pos[0], cd.pos[1], cd.pos[2]),
                ["scale"]    = new JArray(cd.scale[0], cd.scale[1], cd.scale[2])
            };
            var msg = new NetworkMessage {
                action     = "spawn",
                id         = cd.id,
                objectType = "Cube",
                parameters = p
            };

            objectManager.HandleMessage(msg);
            networkManager.Enqueue(msg);
        }

        Debug.Log("[SceneData] Scene loaded and broadcast.");
    }

    /// <summary>
    /// Deletes all objects locally and notifies all clients.
    /// </summary>
    public void ClearSceneNetworked()
    {
        // Local clear
        objectManager.HandleMessage(new NetworkMessage { action = "clear" });
        // Network clear
        networkManager.Enqueue(new NetworkMessage { action = "clear" });
        Debug.Log("[SceneData] Scene cleared and broadcast.");
    }
}
