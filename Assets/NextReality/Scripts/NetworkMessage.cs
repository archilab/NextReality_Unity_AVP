// NetworkMessage.cs
using Newtonsoft.Json.Linq;

/// <summary>
/// A generic, dynamic message for spawning/updating/deleting objects.
/// </summary>
[System.Serializable]
public class NetworkMessage
{
    // "spawn" | "update" | "delete" | "clear"
    public string action;

    // Unique identifier for the object
    public string id;

    // e.g. "Cube", "Sphere", custom types
    public string objectType;

    // Arbitrary payload (position, scale, color, materialName, etc)
    public JObject parameters;
}
