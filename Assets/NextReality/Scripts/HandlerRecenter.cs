// HandlerRecenter.cs
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// When the user releases the handler, we asynchronously create an ARAnchor
/// at the handler’s pose and reparent the XROrigin so that the handler’s
/// world‐position becomes the new scene origin.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class HandlerRecenter : MonoBehaviour
{
    [Tooltip("Assign the same XROrigin used by QRManager")]
    public XROrigin xrOrigin;

    [Tooltip("ARAnchorManager for creating spatial anchors")]
    public ARAnchorManager anchorManager;

    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grab.selectExited.RemoveListener(OnRelease);
    }

    // Note: async void is acceptable for event callbacks
    private async void OnRelease(SelectExitEventArgs args)
    {
        // 1) Build the pose where the handler is in world space
        Pose handlerPose = new Pose(transform.position, transform.rotation);

        if (anchorManager == null)
        {
            Debug.LogError("[HandlerRecenter] anchorManager not assigned!");
            return;
        }

        // 2) Asynchronously create the ARAnchor at that pose
        var promise = anchorManager.TryAddAnchorAsync(handlerPose);
        var result  = await promise;            // await the Awaitable<Result<ARAnchor>>
        ARAnchor anchor = result.value;        // extract the actual ARAnchor

        if (anchor == null)
        {
            Debug.LogError("[HandlerRecenter] Failed to create anchor!");
            return;
        }

        // 3) Reparent the XR Origin under this new anchor
        xrOrigin.transform.SetParent(anchor.transform, false);
        xrOrigin.transform.localPosition = Vector3.zero;
        xrOrigin.transform.localRotation = Quaternion.identity;

        Debug.Log($"[HandlerRecenter] Scene re-anchored at {handlerPose.position}");
    }
}
