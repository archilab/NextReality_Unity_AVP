// GestureInteractor.cs
using UnityEngine;
using UnityEngine.XR.Hands;
using Newtonsoft.Json.Linq;

/// <summary>
/// Spawns cubes on thumb-finger tap and lets you move/rotate them by pinch-and-hold.
/// Sends all actions over the network.
/// Based on PolySpatial manipulation patterns.
/// </summary>
public class GestureInteractor : MonoBehaviour
{
    [Header("References")]
    public XRHandSubsystem        handSubsystem;
    public ObjectManager         objectManager;
    public AsyncNetworkManager   networkManager;

    [Header("Spawn Settings")]
    public float spawnCooldown  = 1.5f;
    public float tapThreshold = 0.03f;
    public float tapDuration = 0.3f; // Maximum time for a tap gesture

    [Header("Manipulation Settings")]
    public float pinchThreshold = 0.03f;
    public float grabDistance = 0.3f;
    public float rotationSensitivity = 1f;

    // Spawn tracking
    float lastSpawnTime;
    bool rightHandTapStarted = false;
    float rightHandTapStartTime = 0f;

    // Manipulation tracking
    GameObject selectedGO;
    string     selectedId;
    Vector3    grabOffset;
    Quaternion grabRotationOffset;
    bool       isGrabbing = false;
    Vector3    initialGrabPosition;
    Quaternion initialGrabRotation;

    void Update()
    {
        if (handSubsystem == null) return;

        // 1) Thumb-finger tap spawn (right hand only)
        HandleTapSpawn();

        // 2) Pinch-and-hold for move/rotate
        HandlePinchManipulation();
    }

    private void HandleTapSpawn()
    {
        XRHand right = handSubsystem.rightHand;
        if (!right.isTracked) return;

        // Check for thumb-finger tap gesture
        if (TryGetThumbFingerTap(right, out Vector3 tapPosition, out bool tapStarted, out bool tapCompleted))
        {
            if (tapStarted)
            {
                rightHandTapStarted = true;
                rightHandTapStartTime = Time.time;
            }
            
            if (tapCompleted && rightHandTapStarted && Time.time - lastSpawnTime > spawnCooldown)
            {
                // Spawn object at tap position
                Vector3 spawnPos = tapPosition + Vector3.up * 0.1f; // Slight offset above hand
                selectedId = objectManager.SpawnObject(spawnPos);

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
                rightHandTapStarted = false;
            }
        }
        else
        {
            // Reset tap state if fingers are too far apart
            rightHandTapStarted = false;
        }
    }

    private void HandlePinchManipulation()
    {
        XRHand right = handSubsystem.rightHand;
        if (!right.isTracked) return;

        // Check for pinch gesture
        if (TryGetPinch(right, out Vector3 pinchPos, out float pinchDist, out bool pinchActive))
        {
            if (pinchActive)
            {
                if (!isGrabbing)
                {
                    // Start grab - find closest object
                    (selectedGO, selectedId) = objectManager.FindClosest(pinchPos, grabDistance);
                    if (selectedGO != null)
                    {
                        StartGrab(selectedGO, selectedId, pinchPos);
                    }
                }
                else if (selectedGO != null)
                {
                    // Continue grab - update position and rotation
                    UpdateGrab(pinchPos);
                }
            }
            else
            {
                // Release grab
                if (isGrabbing)
                {
                    ReleaseGrab();
                }
            }
        }
        else
        {
            // Release grab if pinch is lost
            if (isGrabbing)
            {
                ReleaseGrab();
            }
        }
    }

    private void StartGrab(GameObject objectToGrab, string objectId, Vector3 grabPosition)
    {
        selectedGO = objectToGrab;
        selectedId = objectId;
        isGrabbing = true;
        
        // Calculate offset from grab position to object center
        grabOffset = objectToGrab.transform.position - grabPosition;
        
        // Store initial rotation for relative rotation calculation
        initialGrabPosition = grabPosition;
        initialGrabRotation = objectToGrab.transform.rotation;
        grabRotationOffset = objectToGrab.transform.rotation;
    }

    private void UpdateGrab(Vector3 grabPosition)
    {
        if (selectedGO == null) return;

        // Update position
        Vector3 newPosition = grabPosition + grabOffset;
        selectedGO.transform.position = newPosition;

        // Calculate rotation based on hand movement
        Vector3 handMovement = grabPosition - initialGrabPosition;
        if (handMovement.magnitude > 0.01f)
        {
            // Create rotation based on hand movement direction
            Quaternion movementRotation = Quaternion.LookRotation(handMovement.normalized, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(initialGrabRotation, movementRotation, rotationSensitivity * Time.deltaTime);
            selectedGO.transform.rotation = newRotation;
        }

        // Broadcast update
        var updMsg = new NetworkMessage {
            action     = "update",
            id         = selectedId,
            objectType = "Cube",
            parameters = new JObject {
                ["position"] = new JArray(newPosition.x, newPosition.y, newPosition.z),
                ["rotation"] = new JArray(selectedGO.transform.rotation.x, selectedGO.transform.rotation.y, selectedGO.transform.rotation.z, selectedGO.transform.rotation.w),
                ["scale"]    = new JArray(selectedGO.transform.localScale.x, selectedGO.transform.localScale.y, selectedGO.transform.localScale.z)
            }
        };
        networkManager.Enqueue(updMsg);
    }

    private void ReleaseGrab()
    {
        selectedGO = null;
        selectedId = null;
        isGrabbing = false;
    }

    // Try to detect a thumb-finger tap gesture
    private bool TryGetThumbFingerTap(XRHand hand, out Vector3 position, out bool tapStarted, out bool tapCompleted)
    {
        position = Vector3.zero;
        tapStarted = false;
        tapCompleted = false;

        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var index = hand.GetJoint(XRHandJointID.IndexTip);
        
        if (!thumb.TryGetPose(out Pose tPose) || !index.TryGetPose(out Pose iPose))
            return false;

        float distance = Vector3.Distance(tPose.position, iPose.position);
        position = (tPose.position + iPose.position) * 0.5f;

        // Check if fingers are close enough for a tap
        if (distance < tapThreshold)
        {
            // Check if this is a new tap
            if (!rightHandTapStarted)
            {
                tapStarted = true;
            }
            
            // Check if tap duration is within acceptable range
            if (rightHandTapStarted && Time.time - rightHandTapStartTime <= tapDuration)
            {
                // Continue tap
                return true;
            }
        }
        else
        {
            // Fingers separated - check if this completes a tap
            if (rightHandTapStarted && Time.time - rightHandTapStartTime <= tapDuration)
            {
                tapCompleted = true;
                return true;
            }
        }

        return false;
    }

    // Try to detect a pinch gesture with active state
    private bool TryGetPinch(XRHand hand, out Vector3 position, out float distance, out bool pinchActive)
    {
        position = Vector3.zero;
        distance = 0f;
        pinchActive = false;

        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var index = hand.GetJoint(XRHandJointID.IndexTip);
        
        if (!thumb.TryGetPose(out Pose tPose) || !index.TryGetPose(out Pose iPose))
            return false;

        distance = Vector3.Distance(tPose.position, iPose.position);
        position = (tPose.position + iPose.position) * 0.5f;

        // Pinch is active when fingers are close enough
        pinchActive = distance < pinchThreshold;
        return true;
    }

    // Legacy method for compatibility (deprecated)
    bool TryGetPalmPose(XRHand hand, out Pose palmPose)
    {
        palmPose = default;
        var joint = hand.GetJoint(XRHandJointID.Palm);
        return joint.TryGetPose(out palmPose);
    }
}
