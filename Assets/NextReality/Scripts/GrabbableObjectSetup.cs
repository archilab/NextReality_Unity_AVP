// GrabbableObjectSetup.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.PolySpatial;

/// <summary>
/// Example script showing how to set up grabbable objects for Apple Vision Pro.
/// This script demonstrates the essential components needed for hand interaction.
/// </summary>
public class GrabbableObjectSetup : MonoBehaviour
{
    [Header("Required Components")]
    [Tooltip("Collider for interaction detection")]
    public Collider interactionCollider;
    
    [Tooltip("Rigidbody for physics simulation")]
    public Rigidbody objectRigidbody;
    
    [Tooltip("XR Grab Interactable for hand grabbing")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    
    [Header("Optional Components")]
    [Tooltip("PolySpatial Volume for spatial anchoring")]
    public Volume volume;
    
    [Tooltip("Custom interaction behavior")]
    public MonoBehaviour customInteractionScript;

    [Header("Setup Instructions")]
    [TextArea(10, 20)]
    public string setupInstructions = @"
SETUP INSTRUCTIONS FOR GRABBABLE OBJECTS ON VISION PRO:

1. REQUIRED COMPONENTS:
   - Collider (BoxCollider, SphereCollider, etc.)
   - Rigidbody (for physics)
   - XR Grab Interactable (for hand interaction)

2. OPTIONAL COMPONENTS:
   - PolySpatial Volume (for spatial anchoring)
   - Custom interaction scripts
   - Audio sources for feedback

3. RIGIDBODY SETTINGS:
   - Use Gravity: Usually false for floating objects
   - Is Kinematic: false for physics interaction
   - Constraints: Lock rotation/position as needed

4. COLLIDER SETTINGS:
   - Is Trigger: false for physical interaction
   - Size: Appropriate for the object
   - Layer: Set to grabbable layer

5. XR GRAB INTERACTABLE SETTINGS:
   - Interaction Manager: Assign XR Interaction Manager
   - Interaction Layers: Set to appropriate layer mask
   - Select Action Trigger: Select On Activate
   - Hover Entered/Exited: Add visual feedback
   - Select Entered/Exited: Add grab feedback

6. POLYSPATIAL VOLUME (Optional):
   - Enable for spatial anchoring
   - Configure size and shape
   - Set anchoring behavior

7. TESTING:
   - Use Vision Pro simulator
   - Test with hand tracking
   - Verify physics behavior
   - Check network synchronization
";

    void Start()
    {
        // Auto-setup if components are missing
        SetupRequiredComponents();
    }

    /// <summary>
    /// Automatically adds required components if they're missing
    /// </summary>
    [ContextMenu("Setup Required Components")]
    public void SetupRequiredComponents()
    {
        // Add Collider if missing
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider>();
            if (interactionCollider == null)
            {
                // Add a default BoxCollider
                interactionCollider = gameObject.AddComponent<BoxCollider>();
                Debug.Log($"[GrabbableObjectSetup] Added BoxCollider to {gameObject.name}");
            }
        }

        // Add Rigidbody if missing
        if (objectRigidbody == null)
        {
            objectRigidbody = GetComponent<Rigidbody>();
            if (objectRigidbody == null)
            {
                objectRigidbody = gameObject.AddComponent<Rigidbody>();
                ConfigureRigidbody();
                Debug.Log($"[GrabbableObjectSetup] Added Rigidbody to {gameObject.name}");
            }
        }

        // Add XR Grab Interactable if missing
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                ConfigureGrabInteractable();
                Debug.Log($"[GrabbableObjectSetup] Added XRGrabInteractable to {gameObject.name}");
            }
        }

        // Add PolySpatial Volume if missing (optional)
        if (volume == null)
        {
            volume = GetComponent<Volume>();
            if (volume == null)
            {
                volume = gameObject.AddComponent<Volume>();
                ConfigureVolume();
                Debug.Log($"[GrabbableObjectSetup] Added PolySpatial Volume to {gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Configure Rigidbody for Vision Pro interaction
    /// </summary>
    private void ConfigureRigidbody()
    {
        if (objectRigidbody == null) return;

        // Typical settings for grabbable objects
        objectRigidbody.useGravity = false; // Objects float in space
        objectRigidbody.isKinematic = false; // Allow physics interaction
        objectRigidbody.linearDamping = 1f; // Air resistance
        objectRigidbody.angularDamping = 1f; // Angular resistance
        
        // Optional: Lock certain axes
        // objectRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Configure XR Grab Interactable for hand interaction
    /// </summary>
    private void ConfigureGrabInteractable()
    {
        if (grabInteractable == null) return;

        // Set interaction settings
        grabInteractable.selectActionTrigger = UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor.InputTriggerType.SelectOnActivate;
        grabInteractable.throwOnDetach = false; // Don't throw when released
        
        // Set interaction layers (adjust as needed)
        grabInteractable.interactionLayers = 1; // Default layer
        
        // Add event listeners for feedback
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    /// <summary>
    /// Configure PolySpatial Volume for spatial anchoring
    /// </summary>
    private void ConfigureVolume()
    {
        if (volume == null) return;

        // Set volume size based on object bounds
        Bounds bounds = GetComponent<Renderer>()?.bounds ?? new Bounds(Vector3.zero, Vector3.one);
        volume.size = bounds.size;
        
        // Enable spatial anchoring
        volume.enabled = true;
    }

    // Event handlers for interaction feedback
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"[GrabbableObjectSetup] Hover entered on {gameObject.name}");
        // Add visual feedback here (e.g., highlight, glow effect)
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log($"[GrabbableObjectSetup] Hover exited on {gameObject.name}");
        // Remove visual feedback here
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"[GrabbableObjectSetup] Grabbed {gameObject.name}");
        // Add grab feedback here (e.g., haptic feedback, sound)
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"[GrabbableObjectSetup] Released {gameObject.name}");
        // Add release feedback here
    }

    /// <summary>
    /// Create a prefab setup for grabbable objects
    /// </summary>
    [ContextMenu("Create Grabbable Prefab")]
    public void CreateGrabbablePrefab()
    {
        // This would typically be done in the editor
        Debug.Log("[GrabbableObjectSetup] Use this method to create prefabs in the editor");
    }
}

/// <summary>
/// Example of a custom interaction script for specific behavior
/// </summary>
public class CustomGrabbableBehavior : MonoBehaviour
{
    [Header("Custom Settings")]
    public bool enableRotation = true;
    public bool enableScaling = false;
    public float rotationSpeed = 1f;
    public float scaleSpeed = 1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 initialScale;
    private Quaternion initialRotation;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        initialScale = transform.localScale;
        initialRotation = transform.rotation;

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log($"[CustomGrabbableBehavior] Custom grab behavior for {gameObject.name}");
        // Add custom grab logic here
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log($"[CustomGrabbableBehavior] Custom release behavior for {gameObject.name}");
        // Add custom release logic here
    }

    void Update()
    {
        // Add custom update logic here
        if (enableRotation && grabInteractable.isSelected)
        {
            // Example: Rotate while grabbed
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
} 