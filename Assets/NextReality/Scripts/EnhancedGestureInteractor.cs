using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using Newtonsoft.Json.Linq;

/// <summary>
/// Enhanced gesture system for 3D object manipulation:
/// - Hand grab gestures for moving and rotating objects
/// - Dual hand pinch gestures for natural scaling
/// - Palm-up gesture for spawning cubes
/// Based on PolySpatial Manipulation patterns
/// </summary>
public class EnhancedGestureInteractor : MonoBehaviour
{
    [Header("References")]
    public XRHandSubsystem handSubsystem;
    public ObjectManager objectManager;
    public AsyncNetworkManager networkManager;

    [Header("Spawn Settings")]
    public float spawnCooldown = 1.5f;
    public float palmUpThreshold = 0.8f;
    public float spawnOffset = 0.2f;

    [Header("Grab Settings")]
    public float grabDistance = 0.3f;
    public float grabThreshold = 0.03f;
    public LayerMask grabbableLayer = -1;

    [Header("Dual Hand Scaling")]
    public float dualHandScaleThreshold = 0.05f;
    public float minScale = 0.1f;
    public float maxScale = 10f;
    public float scaleSensitivity = 1f;

    // Spawn tracking
    private float lastSpawnTime;

    // Grab tracking
    private GameObject grabbedObject;
    private string grabbedObjectId;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;
    private bool isGrabbing = false;

    // Dual hand scaling
    private bool isDualHandScaling = false;
    private Vector3 initialDualHandMidpoint;
    private float initialDualHandDistance;
    private Vector3 initialObjectScale;
    private Vector3 initialObjectPosition;

    // Hand tracking
    private XRHand leftHand;
    private XRHand rightHand;
    private bool leftHandTracked = false;
    private bool rightHandTracked = false;

    void Update()
    {
        if (handSubsystem == null) return;

        UpdateHandTracking();
        
        // 1. Palm-up spawn gesture
        HandlePalmUpSpawn();
        
        // 2. Single hand grab for move/rotate
        HandleSingleHandGrab();
        
        // 3. Dual hand pinch for scaling
        HandleDualHandScaling();
    }

    private void UpdateHandTracking()
    {
        leftHand = handSubsystem.leftHand;
        rightHand = handSubsystem.rightHand;
        
        leftHandTracked = leftHand.isTracked;
        rightHandTracked = rightHand.isTracked;
    }

    private void HandlePalmUpSpawn()
    {
        // Check both hands for palm-up gesture
        if (TryGetPalmUpGesture(leftHand, out Vector3 leftSpawnPos) && 
            Time.time - lastSpawnTime > spawnCooldown)
        {
            SpawnCube(leftSpawnPos);
            return;
        }
        
        if (TryGetPalmUpGesture(rightHand, out Vector3 rightSpawnPos) && 
            Time.time - lastSpawnTime > spawnCooldown)
        {
            SpawnCube(rightSpawnPos);
            return;
        }
    }

    private void HandleSingleHandGrab()
    {
        // Check for grab gesture on either hand
        if (TryGetGrabGesture(leftHand, out Vector3 leftGrabPos, out bool leftGrabActive))
        {
            HandleGrabGesture(leftGrabPos, leftGrabActive);
        }
        else if (TryGetGrabGesture(rightHand, out Vector3 rightGrabPos, out bool rightGrabActive))
        {
            HandleGrabGesture(rightGrabPos, rightGrabActive);
        }
        else
        {
            // No grab gesture detected, release if grabbing
            if (isGrabbing)
            {
                ReleaseGrab();
            }
        }
    }

    private void HandleDualHandScaling()
    {
        if (leftHandTracked && rightHandTracked)
        {
            if (TryGetDualHandPinch(out Vector3 leftPinchPos, out Vector3 rightPinchPos, out bool dualPinchActive))
            {
                if (dualPinchActive && !isDualHandScaling)
                {
                    StartDualHandScaling(leftPinchPos, rightPinchPos);
                }
                else if (dualPinchActive && isDualHandScaling)
                {
                    UpdateDualHandScaling(leftPinchPos, rightPinchPos);
                }
                else if (!dualPinchActive && isDualHandScaling)
                {
                    EndDualHandScaling();
                }
            }
        }
        else
        {
            if (isDualHandScaling)
            {
                EndDualHandScaling();
            }
        }
    }

    private bool TryGetPalmUpGesture(XRHand hand, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        
        if (!hand.isTracked) return false;
        
        var palmJoint = hand.GetJoint(XRHandJointID.Palm);
        if (!palmJoint.TryGetPose(out Pose palmPose)) return false;
        
        Vector3 palmUp = palmPose.rotation * Vector3.up;
        if (Vector3.Dot(palmUp, Vector3.up) > palmUpThreshold)
        {
            spawnPosition = palmPose.position + palmUp * spawnOffset;
            return true;
        }
        
        return false;
    }

    private bool TryGetGrabGesture(XRHand hand, out Vector3 grabPosition, out bool grabActive)
    {
        grabPosition = Vector3.zero;
        grabActive = false;
        
        if (!hand.isTracked) return false;
        
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        
        if (!thumbTip.TryGetPose(out Pose thumbPose) || 
            !indexTip.TryGetPose(out Pose indexPose) || 
            !middleTip.TryGetPose(out Pose middlePose))
            return false;
        
        // Check if thumb is close to both index and middle finger (grab gesture)
        float thumbToIndex = Vector3.Distance(thumbPose.position, indexPose.position);
        float thumbToMiddle = Vector3.Distance(thumbPose.position, middlePose.position);
        
        if (thumbToIndex < grabThreshold && thumbToMiddle < grabThreshold)
        {
            grabPosition = (thumbPose.position + indexPose.position + middlePose.position) / 3f;
            grabActive = true;
            return true;
        }
        
        return false;
    }

    private bool TryGetDualHandPinch(out Vector3 leftPinchPos, out Vector3 rightPinchPos, out bool pinchActive)
    {
        leftPinchPos = Vector3.zero;
        rightPinchPos = Vector3.zero;
        pinchActive = false;
        
        if (!leftHandTracked || !rightHandTracked) return false;
        
        var leftThumb = leftHand.GetJoint(XRHandJointID.ThumbTip);
        var leftIndex = leftHand.GetJoint(XRHandJointID.IndexTip);
        var rightThumb = rightHand.GetJoint(XRHandJointID.ThumbTip);
        var rightIndex = rightHand.GetJoint(XRHandJointID.IndexTip);
        
        if (!leftThumb.TryGetPose(out Pose leftThumbPose) || 
            !leftIndex.TryGetPose(out Pose leftIndexPose) ||
            !rightThumb.TryGetPose(out Pose rightThumbPose) || 
            !rightIndex.TryGetPose(out Pose rightIndexPose))
            return false;
        
        // Check pinch on both hands
        float leftPinchDist = Vector3.Distance(leftThumbPose.position, leftIndexPose.position);
        float rightPinchDist = Vector3.Distance(rightThumbPose.position, rightIndexPose.position);
        
        if (leftPinchDist < dualHandScaleThreshold && rightPinchDist < dualHandScaleThreshold)
        {
            leftPinchPos = (leftThumbPose.position + leftIndexPose.position) * 0.5f;
            rightPinchPos = (rightThumbPose.position + rightIndexPose.position) * 0.5f;
            pinchActive = true;
            return true;
        }
        
        return false;
    }

    private void SpawnCube(Vector3 position)
    {
        string cubeId = objectManager.SpawnCube(position);
        
        // Broadcast spawn
        var spawnMsg = new NetworkMessage {
            action = "spawn",
            id = cubeId,
            objectType = "Cube",
            parameters = new JObject {
                ["position"] = new JArray(position.x, position.y, position.z),
                ["scale"] = new JArray(1f, 1f, 1f)
            }
        };
        networkManager.Enqueue(spawnMsg);
        
        lastSpawnTime = Time.time;
        Debug.Log($"[EnhancedGesture] Spawned cube at {position}");
    }

    private void HandleGrabGesture(Vector3 grabPosition, bool grabActive)
    {
        if (grabActive && !isGrabbing)
        {
            // Try to grab an object
            (GameObject closestObject, string objectId) = objectManager.FindClosest(grabPosition, grabDistance);
            if (closestObject != null)
            {
                StartGrab(closestObject, objectId, grabPosition);
            }
        }
        else if (grabActive && isGrabbing)
        {
            // Update grabbed object position
            UpdateGrab(grabPosition);
        }
        else if (!grabActive && isGrabbing)
        {
            // Release grabbed object
            ReleaseGrab();
        }
    }

    private void StartGrab(GameObject objectToGrab, string objectId, Vector3 grabPosition)
    {
        grabbedObject = objectToGrab;
        grabbedObjectId = objectId;
        grabOffset = objectToGrab.transform.position - grabPosition;
        grabRotationOffset = Quaternion.Inverse(Quaternion.LookRotation(grabPosition - objectToGrab.transform.position)) * objectToGrab.transform.rotation;
        isGrabbing = true;
        
        Debug.Log($"[EnhancedGesture] Started grabbing {objectId}");
    }

    private void UpdateGrab(Vector3 grabPosition)
    {
        if (grabbedObject == null) return;
        
        // Update position
        Vector3 newPosition = grabPosition + grabOffset;
        grabbedObject.transform.position = newPosition;
        
        // Update rotation based on hand movement
        Quaternion newRotation = Quaternion.LookRotation(grabPosition - grabbedObject.transform.position) * grabRotationOffset;
        grabbedObject.transform.rotation = newRotation;
        
        // Broadcast update
        var updateMsg = new NetworkMessage {
            action = "update",
            id = grabbedObjectId,
            objectType = "Cube",
            parameters = new JObject {
                ["position"] = new JArray(newPosition.x, newPosition.y, newPosition.z),
                ["rotation"] = new JArray(newRotation.x, newRotation.y, newRotation.z, newRotation.w),
                ["scale"] = new JArray(grabbedObject.transform.localScale.x, grabbedObject.transform.localScale.y, grabbedObject.transform.localScale.z)
            }
        };
        networkManager.Enqueue(updateMsg);
    }

    private void ReleaseGrab()
    {
        if (grabbedObject != null)
        {
            Debug.Log($"[EnhancedGesture] Released grab on {grabbedObjectId}");
        }
        
        grabbedObject = null;
        grabbedObjectId = null;
        isGrabbing = false;
    }

    private void StartDualHandScaling(Vector3 leftPinchPos, Vector3 rightPinchPos)
    {
        // Find object between hands
        Vector3 midpoint = (leftPinchPos + rightPinchPos) * 0.5f;
        (GameObject closestObject, string objectId) = objectManager.FindClosest(midpoint, grabDistance);
        
        if (closestObject != null)
        {
            grabbedObject = closestObject;
            grabbedObjectId = objectId;
            initialDualHandMidpoint = midpoint;
            initialDualHandDistance = Vector3.Distance(leftPinchPos, rightPinchPos);
            initialObjectScale = closestObject.transform.localScale;
            initialObjectPosition = closestObject.transform.position;
            isDualHandScaling = true;
            
            Debug.Log($"[EnhancedGesture] Started dual hand scaling {objectId}");
        }
    }

    private void UpdateDualHandScaling(Vector3 leftPinchPos, Vector3 rightPinchPos)
    {
        if (grabbedObject == null) return;
        
        Vector3 currentMidpoint = (leftPinchPos + rightPinchPos) * 0.5f;
        float currentDistance = Vector3.Distance(leftPinchPos, rightPinchPos);
        
        // Calculate scale factor based on distance change
        float scaleFactor = currentDistance / initialDualHandDistance;
        scaleFactor = Mathf.Clamp(scaleFactor * scaleSensitivity, minScale, maxScale);
        
        // Apply scaling
        Vector3 newScale = initialObjectScale * scaleFactor;
        grabbedObject.transform.localScale = newScale;
        
        // Move object to follow hand midpoint
        Vector3 positionOffset = currentMidpoint - initialDualHandMidpoint;
        grabbedObject.transform.position = initialObjectPosition + positionOffset;
        
        // Broadcast update
        var updateMsg = new NetworkMessage {
            action = "update",
            id = grabbedObjectId,
            objectType = "Cube",
            parameters = new JObject {
                ["position"] = new JArray(grabbedObject.transform.position.x, grabbedObject.transform.position.y, grabbedObject.transform.position.z),
                ["rotation"] = new JArray(grabbedObject.transform.rotation.x, grabbedObject.transform.rotation.y, grabbedObject.transform.rotation.z, grabbedObject.transform.rotation.w),
                ["scale"] = new JArray(newScale.x, newScale.y, newScale.z)
            }
        };
        networkManager.Enqueue(updateMsg);
    }

    private void EndDualHandScaling()
    {
        if (grabbedObject != null)
        {
            Debug.Log($"[EnhancedGesture] Ended dual hand scaling {grabbedObjectId}");
        }
        
        grabbedObject = null;
        grabbedObjectId = null;
        isDualHandScaling = false;
    }
} 