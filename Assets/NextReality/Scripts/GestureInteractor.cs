// GestureInteractor.cs
using UnityEngine;
using UnityEngine.XR.Hands;
using Newtonsoft.Json.Linq;

/// <summary>
/// Spawns cubes on palm‑up and lets you move/scale them by pinching.
/// Sends all actions over the network.
/// </summary>
public class GestureInteractor : MonoBehaviour
{
    [Header("References")]
    public XRHandSubsystem        handSubsystem;
    public ObjectManager         objectManager;
    public AsyncNetworkManager   networkManager;

    [Header("Settings")]
    public float spawnCooldown  = 1.5f;
    public float pinchThreshold = 0.03f;

    float    lastSpawnTime;
    GameObject selectedGO;
    string     selectedId;
    float      initialPinchDistance;
    Vector3    initialScale;

    void Update()
    {
        if (handSubsystem == null) return;

        // 1) Palm–up spawn
        XRHand left = handSubsystem.leftHand;
        if (TryGetPalmPose(left, out Pose palmPose))
        {
            Vector3 palmUp = palmPose.rotation * Vector3.up;
            if (Time.time - lastSpawnTime > spawnCooldown &&
                Vector3.Dot(palmUp, Vector3.up) > 0.8f)
            {
                Vector3 spawnPos = palmPose.position + palmUp * 0.2f;
                selectedId       = objectManager.SpawnCube(spawnPos);

                // Broadcast spawn
                var spawnMsg = new NetworkMessage {
                    action     = "spawn",
                    id         = selectedId,
                    objectType = "Cube",
                    parameters = new JObject {
                        ["position"] = new JArray(spawnPos.x, spawnPos.y, spawnPos.z),
                        ["scale"]    = new JArray(1f, 1f, 1f)
                    }
                };
                networkManager.Enqueue(spawnMsg);

                lastSpawnTime = Time.time;
                return; // skip pinch this frame
            }
        }

        // 2) Pinch to select/move/scale
        XRHand right = handSubsystem.rightHand;
        if (TryGetPinch(right, out Vector3 pinchPos, out float pinchDist))
        {
            if (selectedGO == null)
            {
                // find closest object within 20cm
                (selectedGO, selectedId) = objectManager.FindClosest(pinchPos, 0.2f);
                if (selectedGO != null)
                {
                    initialPinchDistance = pinchDist;
                    initialScale         = selectedGO.transform.localScale;
                }
            }

            if (selectedGO != null)
            {
                // update transform
                selectedGO.transform.position = pinchPos;
                float scaleFactor = pinchDist / initialPinchDistance;
                Vector3 newScale = initialScale * Mathf.Clamp(scaleFactor, 0.1f, 10f);
                selectedGO.transform.localScale = newScale;

                // broadcast update
                var updMsg = new NetworkMessage {
                    action     = "update",
                    id         = selectedId,
                    objectType = "Cube",
                    parameters = new JObject {
                        ["position"] = new JArray(pinchPos.x, pinchPos.y, pinchPos.z),
                        ["scale"]    = new JArray(newScale.x, newScale.y, newScale.z)
                    }
                };
                networkManager.Enqueue(updMsg);
            }
        }
        else
        {
            // release selection
            selectedGO = null;
            selectedId = null;
        }
    }

    // Try to get the palm joint's Pose
    bool TryGetPalmPose(XRHand hand, out Pose palmPose)
    {
        palmPose = default;
        var joint = hand.GetJoint(XRHandJointID.Palm);
        return joint.TryGetPose(out palmPose);
    }

    // Try to detect a pinch gesture
    bool TryGetPinch(XRHand hand, out Vector3 position, out float distance)
    {
        position = Vector3.zero;
        distance = 0f;
        var thumb  = hand.GetJoint(XRHandJointID.ThumbTip);
        var index  = hand.GetJoint(XRHandJointID.IndexTip);
        if (!thumb.TryGetPose(out Pose tPose) || !index.TryGetPose(out Pose iPose))
            return false;

        distance = Vector3.Distance(tPose.position, iPose.position);
        if (distance < pinchThreshold)
        {
            position = (tPose.position + iPose.position) * 0.5f;
            return true;
        }
        return false;
    }
}
