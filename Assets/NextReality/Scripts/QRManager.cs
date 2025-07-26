// QRManager.cs
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Detects your floor‐printed QR via ARTrackedImageManager,
/// creates a spatial anchor **asynchronously**, and stabilizes the scene under it.
/// Falls back to a manual handler if the QR isn’t in view.
/// </summary>
public class QRManager : MonoBehaviour
{
    [Header("AR Tracking")]
    public ARTrackedImageManager trackedImageManager;
    public ARAnchorManager       anchorManager;

    [Header("XR Origin")]
    public XROrigin xrOrigin;

    [Header("Visuals")]
    public GameObject originMarkerPrefab;
    public GameObject handlerPrefab;

    ARAnchor   _currentAnchor;
    GameObject _originMarker;
    GameObject _handler;

    [System.Obsolete]
    void Start()
    {
        // Instantiate & hide the origin marker
        _originMarker = Instantiate(originMarkerPrefab);
        _originMarker.SetActive(false);

        // Instantiate the manual handler (XRGrabInteractable) for fallback
        _handler = Instantiate(handlerPrefab);

        // Listen for QR updates
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    [System.Obsolete]
    void OnDestroy()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    [System.Obsolete]
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs evt)
    {
        foreach (var img in evt.added)    TryRecenterAsync(img);
        foreach (var img in evt.updated)
            if (img.trackingState == TrackingState.Tracking)
                TryRecenterAsync(img);
    }

    /// <summary>
    /// If this image is our QR, re‐anchor asynchronously.
    /// </summary>
    async void TryRecenterAsync(ARTrackedImage img)
    {
        if (img.referenceImage.name != "SceneOriginQR")
            return;

        // 1) Remove old anchor
        if (_currentAnchor != null)
            anchorManager.TryRemoveAnchor(_currentAnchor);

        // 2) Pose where we want the anchor
        Pose qrPose = new Pose(img.transform.position, img.transform.rotation);

        // 3) Create the anchor **asynchronously** via ARAnchorManager
        //    This returns an Awaitable<Result<ARAnchor>> under the hood
        var promise = anchorManager.TryAddAnchorAsync(qrPose);
        var result  = await promise;                      // <-- await it
        var anchor  = result.value;                       // <-- extract the real ARAnchor

        if (anchor == null)
        {
            Debug.LogError("[QRManager] Anchor creation failed!");
            return;
        }

        _currentAnchor = anchor;

        // 4) Parent XR Origin under the new anchor
        xrOrigin.transform.SetParent(_currentAnchor.transform, false);
        xrOrigin.transform.localPosition = Vector3.zero;
        xrOrigin.transform.localRotation = Quaternion.identity;

        // 5) Show the visual marker at local (0,0,0)
        _originMarker.transform.SetParent(_currentAnchor.transform, false);
        _originMarker.transform.localPosition = Vector3.zero;
        _originMarker.transform.localRotation = Quaternion.identity;
        _originMarker.SetActive(true);

        // 6) Hide the manual handler
        _handler.SetActive(false);

        Debug.Log("[QRManager] Anchored and stabilized at QR.");
    }

    /// <summary>
    /// Called by “set QR” (voice/UI).  
    /// Tries QR recenter; if none visible, enables the manual handler.
    /// </summary>
    public void RecenterFromQR()
    {
        foreach (var img in trackedImageManager.trackables)
        {
            if (img.trackingState == TrackingState.Tracking)
            {
                TryRecenterAsync(img);
                return;
            }
        }

        // fallback: show handler to grab and place
        if (_currentAnchor != null)
            anchorManager.TryRemoveAnchor(_currentAnchor);

        _originMarker.SetActive(false);
        _handler.SetActive(true);
        Debug.LogWarning("[QRManager] No QR detected; manual handler active.");
    }
}
