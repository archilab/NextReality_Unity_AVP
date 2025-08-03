// EnhancedGesturePrefabSetup.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_VISIONOS
using Unity.PolySpatial;
#endif

/// <summary>
/// Setup script for configuring GameObject prefabs to work with EnhancedGestureInteractor.
/// This script ensures prefabs are properly configured for hand gesture interaction.
/// </summary>
public class EnhancedGesturePrefabSetup : MonoBehaviour
{
    [Header("EnhancedGestureInteractor Requirements")]
    [Tooltip("Collider for interaction detection")]
    public Collider interactionCollider;
    
    [Tooltip("Rigidbody for physics simulation")]
    public Rigidbody objectRigidbody;
    
    [Tooltip("Renderer for visual feedback")]
    public Renderer objectRenderer;
    
    [Header("Optional Components")]
    [Tooltip("PolySpatial Volume for spatial anchoring")]
    #if UNITY_VISIONOS
    public Volume spatialVolume;
    #else
    public MonoBehaviour spatialVolume; // Placeholder for non-VisionOS platforms
    #endif
    
    [Tooltip("Audio source for interaction feedback")]
    public AudioSource audioSource;
    
    [Tooltip("Custom interaction behavior")]
    public MonoBehaviour customInteractionScript;

    [Header("EnhancedGestureInteractor Settings")]
    [Tooltip("Layer for grabbable objects")]
    public LayerMask grabbableLayer = 6; // Default grabbable layer
    
    [Tooltip("Enable visual feedback on hover")]
    public bool enableHoverFeedback = true;
    
    [Tooltip("Enable audio feedback on interaction")]
    public bool enableAudioFeedback = true;
    
    [Tooltip("Enable spatial anchoring")]
    public bool enableSpatialAnchoring = true;

    [Header("Setup Instructions")]
    [TextArea(15, 25)]
    public string setupInstructions = @"
ENHANCEDGESTUREINTERACTOR PREFAB SETUP GUIDE:

1. REQUIRED COMPONENTS FOR ENHANCEDGESTUREINTERACTOR:
   - Collider (for interaction detection)
   - Rigidbody (for physics and manipulation)
   - Renderer (for visual feedback)
   - Proper Layer (set to grabbable layer)

2. ENHANCEDGESTUREINTERACTOR INTERACTION TYPES:
   - Thumb-finger tap: Spawns objects (handled by ObjectManager)
   - Hand grab gesture: Moves and rotates objects
   - Dual hand pinch: Scales objects naturally

3. PREFAB CONFIGURATION REQUIREMENTS:
   - Must be in ObjectManager.prefabList
   - Must have proper collider setup
   - Must have rigidbody for physics
   - Should be on grabbable layer
   - Should have visual feedback components

4. OBJECTMANAGER INTEGRATION:
   - Add prefab to ObjectManager.prefabList
   - Set defaultPrefabIndex if needed
   - Ensure prefab name is unique
   - Test spawn functionality

5. NETWORK SYNCHRONIZATION:
   - Objects are automatically synced via AsyncNetworkManager
   - Position, rotation, and scale are broadcast
   - All clients receive updates in real-time

6. TESTING CHECKLIST:
   - Test thumb-finger tap spawning
   - Test hand grab movement
   - Test hand grab rotation
   - Test dual hand scaling
   - Test network synchronization
   - Test visual feedback
   - Test audio feedback (if enabled)
";

    void Start()
    {
        // Auto-setup if components are missing
        SetupForEnhancedGestureInteractor();
    }

    /// <summary>
    /// Automatically configures the GameObject for EnhancedGestureInteractor
    /// </summary>
    [ContextMenu("Setup for EnhancedGestureInteractor")]
    public void SetupForEnhancedGestureInteractor()
    {
        // 1. Add Collider if missing
        SetupCollider();
        
        // 2. Add Rigidbody if missing
        SetupRigidbody();
        
        // 3. Setup Renderer for visual feedback
        SetupRenderer();
        
        // 4. Set proper layer
        SetupLayer();
        
        // 5. Add optional components
        SetupOptionalComponents();
        
        // 6. Validate setup
        ValidateSetup();
    }

    private void SetupCollider()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider>();
            if (interactionCollider == null)
            {
                // Add appropriate collider based on object type
                if (GetComponent<Renderer>() != null)
                {
                    Bounds bounds = GetComponent<Renderer>().bounds;
                    if (bounds.size.x > bounds.size.y && bounds.size.x > bounds.size.z)
                    {
                        // Wide object - use BoxCollider
                        interactionCollider = gameObject.AddComponent<BoxCollider>();
                    }
                    else
                    {
                        // Tall or deep object - use SphereCollider
                        interactionCollider = gameObject.AddComponent<SphereCollider>();
                    }
                }
                else
                {
                    // Default to BoxCollider
                    interactionCollider = gameObject.AddComponent<BoxCollider>();
                }
                
                Debug.Log($"[EnhancedGesturePrefabSetup] Added {interactionCollider.GetType().Name} to {gameObject.name}");
            }
        }

        // Configure collider for interaction
        if (interactionCollider != null)
        {
            interactionCollider.isTrigger = false; // Physical interaction
            interactionCollider.enabled = true;
        }
    }

    private void SetupRigidbody()
    {
        if (objectRigidbody == null)
        {
            objectRigidbody = GetComponent<Rigidbody>();
            if (objectRigidbody == null)
            {
                objectRigidbody = gameObject.AddComponent<Rigidbody>();
                Debug.Log($"[EnhancedGesturePrefabSetup] Added Rigidbody to {gameObject.name}");
            }
        }

        // Configure rigidbody for EnhancedGestureInteractor
        if (objectRigidbody != null)
        {
            objectRigidbody.useGravity = false; // Objects float in space
            objectRigidbody.isKinematic = false; // Allow physics interaction
            objectRigidbody.linearDamping = 1f; // Air resistance
            objectRigidbody.angularDamping = 1f; // Angular resistance
            objectRigidbody.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
            objectRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection
        }
    }

    private void SetupRenderer()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer == null)
            {
                // Try to find renderer in children
                objectRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // Ensure renderer is enabled for visual feedback
        if (objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }
    }

    private void SetupLayer()
    {
        // Set to grabbable layer
        if (grabbableLayer != -1)
        {
            gameObject.layer = (int)Mathf.Log(grabbableLayer.value, 2);
            Debug.Log($"[EnhancedGesturePrefabSetup] Set {gameObject.name} to layer {gameObject.layer}");
        }
    }

    private void SetupOptionalComponents()
    {
        // Add PolySpatial Volume for spatial anchoring
        #if UNITY_VISIONOS
        if (enableSpatialAnchoring && spatialVolume == null)
        {
            spatialVolume = GetComponent<Volume>();
            if (spatialVolume == null)
            {
                spatialVolume = gameObject.AddComponent<Volume>();
                ConfigureSpatialVolume();
                Debug.Log($"[EnhancedGesturePrefabSetup] Added PolySpatial Volume to {gameObject.name}");
            }
        }
        #else
        if (enableSpatialAnchoring)
        {
            Debug.Log($"[EnhancedGesturePrefabSetup] PolySpatial Volume not available on this platform for {gameObject.name}");
        }
        #endif

        // Add AudioSource for feedback
        if (enableAudioFeedback && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                ConfigureAudioSource();
                Debug.Log($"[EnhancedGesturePrefabSetup] Added AudioSource to {gameObject.name}");
            }
        }

        // Add visual feedback script
        if (enableHoverFeedback && customInteractionScript == null)
        {
            customInteractionScript = GetComponent<EnhancedGestureVisualFeedback>();
            if (customInteractionScript == null)
            {
                customInteractionScript = gameObject.AddComponent<EnhancedGestureVisualFeedback>();
                Debug.Log($"[EnhancedGesturePrefabSetup] Added Visual Feedback to {gameObject.name}");
            }
        }
    }

    private void ConfigureSpatialVolume()
    {
        #if UNITY_VISIONOS
        if (spatialVolume == null) return;

        // Set volume size based on object bounds
        Bounds bounds = objectRenderer?.bounds ?? new Bounds(Vector3.zero, Vector3.one);
        ((Volume)spatialVolume).size = bounds.size;
        
        // Enable spatial anchoring
        spatialVolume.enabled = true;
        #endif
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null) return;

        // Configure for interaction feedback
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D audio
        audioSource.volume = 0.5f;
        audioSource.pitch = 1f;
    }

    private void ValidateSetup()
    {
        bool isValid = true;
        string issues = "";

        // Check required components
        if (interactionCollider == null)
        {
            issues += "- Missing Collider\n";
            isValid = false;
        }

        if (objectRigidbody == null)
        {
            issues += "- Missing Rigidbody\n";
            isValid = false;
        }

        if (objectRenderer == null)
        {
            issues += "- Missing Renderer\n";
            isValid = false;
        }

        // Check layer
        if (gameObject.layer != (int)Mathf.Log(grabbableLayer.value, 2))
        {
            issues += "- Wrong layer (should be grabbable layer)\n";
            isValid = false;
        }

        // Report results
        if (isValid)
        {
            Debug.Log($"[EnhancedGesturePrefabSetup] {gameObject.name} is properly configured for EnhancedGestureInteractor");
        }
        else
        {
            Debug.LogWarning($"[EnhancedGesturePrefabSetup] {gameObject.name} has issues:\n{issues}");
        }
    }

    /// <summary>
    /// Test the prefab setup
    /// </summary>
    [ContextMenu("Test Prefab Setup")]
    public void TestPrefabSetup()
    {
        Debug.Log($"[EnhancedGesturePrefabSetup] Testing {gameObject.name}...");
        
        // Test collider
        if (interactionCollider != null)
        {
            Debug.Log($"✓ Collider: {interactionCollider.GetType().Name}");
        }
        
        // Test rigidbody
        if (objectRigidbody != null)
        {
            Debug.Log($"✓ Rigidbody: useGravity={objectRigidbody.useGravity}, isKinematic={objectRigidbody.isKinematic}");
        }
        
        // Test renderer
        if (objectRenderer != null)
        {
            Debug.Log($"✓ Renderer: {objectRenderer.GetType().Name}");
        }
        
        // Test layer
        Debug.Log($"✓ Layer: {gameObject.layer} (should be grabbable)");
        
        // Test optional components
        #if UNITY_VISIONOS
        if (spatialVolume != null) Debug.Log("✓ PolySpatial Volume");
        #endif
        if (audioSource != null) Debug.Log("✓ AudioSource");
        if (customInteractionScript != null) Debug.Log("✓ Visual Feedback");
    }
}

/// <summary>
/// Visual feedback component for EnhancedGestureInteractor
/// </summary>
public class EnhancedGestureVisualFeedback : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    public Color hoverColor = Color.yellow;
    public Color grabColor = Color.red;
    public float colorLerpSpeed = 5f;
    
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private Color targetColor;
    private bool isHovered = false;
    private bool isGrabbed = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = originalMaterial.color;
            targetColor = originalColor;
        }
    }

    void Update()
    {
        if (objectRenderer != null && originalMaterial != null)
        {
            // Lerp to target color
            originalMaterial.color = Color.Lerp(originalMaterial.color, targetColor, colorLerpSpeed * Time.deltaTime);
        }
    }

    public void OnHoverEnter()
    {
        isHovered = true;
        UpdateTargetColor();
    }

    public void OnHoverExit()
    {
        isHovered = false;
        UpdateTargetColor();
    }

    public void OnGrabStart()
    {
        isGrabbed = true;
        UpdateTargetColor();
    }

    public void OnGrabEnd()
    {
        isGrabbed = false;
        UpdateTargetColor();
    }

    private void UpdateTargetColor()
    {
        if (isGrabbed)
        {
            targetColor = grabColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }
        else
        {
            targetColor = originalColor;
        }
    }
} 