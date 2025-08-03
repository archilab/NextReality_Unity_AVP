# Vision Pro Grabbable Object Setup Guide

## Overview
This guide explains how to prepare GameObjects in Unity to become grabbable/interactable on Apple Vision Pro devices.

## Required Components

### 1. Collider Component
**Purpose**: Defines the interaction area for the object
- **Type**: BoxCollider, SphereCollider, MeshCollider, etc.
- **Settings**:
  - `Is Trigger`: `false` (for physical interaction)
  - `Size`: Appropriate for your object
  - `Layer`: Set to grabbable layer (e.g., "Grabbable")

### 2. Rigidbody Component
**Purpose**: Enables physics simulation and interaction
- **Settings**:
  - `Use Gravity`: `false` (objects float in space)
  - `Is Kinematic`: `false` (allow physics interaction)
  - `Drag`: `1f` (air resistance)
  - `Angular Drag`: `1f` (angular resistance)
  - `Constraints`: Lock specific axes if needed

### 3. XR Grab Interactable Component
**Purpose**: Enables hand grabbing functionality
- **Settings**:
  - `Interaction Manager`: Assign XR Interaction Manager
  - `Interaction Layers`: Set to appropriate layer mask
  - `Select Action Trigger`: `Select On Activate`
  - `Throw On Detach`: `false` (don't throw when released)

## Optional Components

### 4. PolySpatial Volume Component
**Purpose**: Provides spatial anchoring for stable positioning
- **Settings**:
  - `Size`: Match object bounds
  - `Enabled`: `true`
  - `Anchoring Behavior`: Configure as needed

### 5. Custom Interaction Scripts
**Purpose**: Add specific behavior (rotation, scaling, etc.)
- Extend `MonoBehaviour`
- Subscribe to XR events
- Implement custom logic

## Step-by-Step Setup

### Method 1: Manual Setup
1. **Select your GameObject** in the hierarchy
2. **Add Collider**:
   - Right-click → Add Component → Physics → Box Collider
   - Adjust size to match your object
3. **Add Rigidbody**:
   - Right-click → Add Component → Physics → Rigidbody
   - Configure settings as described above
4. **Add XR Grab Interactable**:
   - Right-click → Add Component → XR → XR Grab Interactable
   - Assign XR Interaction Manager
   - Configure interaction layers
5. **Add PolySpatial Volume** (optional):
   - Right-click → Add Component → PolySpatial → Volume
   - Set size to match object bounds

### Method 2: Using the GrabbableObjectSetup Script
1. **Add the script** to your GameObject
2. **Right-click the script** → "Setup Required Components"
3. **Review and adjust** the automatically added components
4. **Configure settings** as needed

### Method 3: Prefab Setup
1. **Create a prefab** with all required components
2. **Configure the prefab** with proper settings
3. **Use the prefab** for consistent grabbable objects

## Configuration Examples

### Basic Cube Setup
```csharp
// Required components
- BoxCollider (size: 1,1,1)
- Rigidbody (useGravity: false, isKinematic: false)
- XRGrabInteractable (selectActionTrigger: SelectOnActivate)
```

### Floating Sphere Setup
```csharp
// Required components
- SphereCollider (radius: 0.5)
- Rigidbody (useGravity: false, drag: 2f)
- XRGrabInteractable (throwOnDetach: false)
- PolySpatial Volume (for spatial anchoring)
```

### Constrained Object Setup
```csharp
// Required components
- Collider (appropriate type)
- Rigidbody (constraints: FreezePositionY | FreezeRotationZ)
- XRGrabInteractable (custom interaction layers)
```

## Event Handling

### Hover Events
```csharp
grabInteractable.hoverEntered.AddListener(OnHoverEntered);
grabInteractable.hoverExited.AddListener(OnHoverExited);
```

### Select Events
```csharp
grabInteractable.selectEntered.AddListener(OnSelectEntered);
grabInteractable.selectExited.AddListener(OnSelectExited);
```

### Custom Behavior Example
```csharp
public class CustomGrabbableBehavior : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }
    
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Add grab feedback (haptic, sound, visual)
        Debug.Log("Object grabbed!");
    }
    
    private void OnReleased(SelectExitEventArgs args)
    {
        // Add release feedback
        Debug.Log("Object released!");
    }
}
```

## Testing

### Vision Pro Simulator
1. **Build for VisionOS** in Unity
2. **Open in Xcode**
3. **Run on Vision Pro Simulator**
4. **Test hand tracking** and interaction

### Debug Tips
- **Check Console** for interaction events
- **Verify Layer Masks** are set correctly
- **Test Physics** behavior
- **Validate Network** synchronization

## Common Issues

### Object Not Grabbable
- **Check Collider**: Ensure it's not a trigger
- **Verify Rigidbody**: Must be present and not kinematic
- **Layer Masks**: Ensure interaction layers match
- **XR Interaction Manager**: Must be assigned

### Physics Issues
- **Gravity**: Disable for floating objects
- **Constraints**: Lock axes if needed
- **Drag Values**: Adjust for desired behavior
- **Collision Detection**: Set to appropriate mode

### Performance Issues
- **Collider Complexity**: Use simple colliders when possible
- **Rigidbody Count**: Limit number of active rigidbodies
- **Update Frequency**: Optimize physics timestep
- **LOD System**: Use level-of-detail for complex objects

## Best Practices

1. **Consistent Setup**: Use prefabs for consistent behavior
2. **Performance**: Optimize colliders and rigidbodies
3. **Feedback**: Add visual/audio feedback for interactions
4. **Testing**: Test on actual Vision Pro device when possible
5. **Documentation**: Document custom behavior and settings

## Integration with Existing Systems

### Network Synchronization
- **ObjectManager**: Handles spawn/update/delete
- **AsyncNetworkManager**: Broadcasts changes
- **SceneDataHandler**: Saves/loads object states

### Gesture Systems
- **GestureInteractor**: Basic hand gestures
- **EnhancedGestureInteractor**: Advanced multi-hand gestures
- **Custom Scripts**: Extend for specific needs

This setup ensures your objects are fully interactive on Apple Vision Pro while maintaining good performance and user experience. 