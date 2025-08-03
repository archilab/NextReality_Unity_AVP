# EnhancedGestureInteractor Prefab Setup Guide

## Overview
This guide explains how to configure GameObject prefabs in Unity to work with the `EnhancedGestureInteractor` system for Apple Vision Pro.

## EnhancedGestureInteractor Requirements

### Core Interaction Types
1. **Thumb-Finger Tap**: Spawns objects (handled by ObjectManager)
2. **Hand Grab Gesture**: Moves and rotates objects
3. **Dual Hand Pinch**: Scales objects naturally

### Required Components for Prefabs

#### 1. Collider Component
**Purpose**: Defines interaction area for hand detection
- **Type**: BoxCollider, SphereCollider, MeshCollider
- **Settings**:
  - `Is Trigger`: `false` (physical interaction)
  - `Size`: Appropriate for object bounds
  - `Enabled`: `true`

#### 2. Rigidbody Component
**Purpose**: Enables physics simulation and manipulation
- **Settings**:
  - `Use Gravity`: `false` (objects float in space)
  - `Is Kinematic`: `false` (allow physics interaction)
  - `Drag`: `1f` (air resistance)
  - `Angular Drag`: `1f` (angular resistance)
  - `Interpolation`: `Interpolate` (smooth movement)
  - `Collision Detection`: `Continuous` (better detection)

#### 3. Renderer Component
**Purpose**: Visual representation and feedback
- **Type**: MeshRenderer, SkinnedMeshRenderer, etc.
- **Settings**:
  - `Enabled`: `true`
  - `Material`: Proper material assignment
  - `Cast Shadows`: As needed

#### 4. Layer Configuration
**Purpose**: Interaction layer management
- **Layer**: Set to grabbable layer (typically layer 6)
- **Layer Mask**: Configured in EnhancedGestureInteractor

## Setup Methods

### Method 1: Using EnhancedGesturePrefabSetup Script

#### Step-by-Step Setup
1. **Add the Script**:
   ```
   Add Component → Scripts → EnhancedGesturePrefabSetup
   ```

2. **Auto-Setup**:
   ```
   Right-click script → "Setup for EnhancedGestureInteractor"
   ```

3. **Configure Settings**:
   - Set `grabbableLayer` (default: 6)
   - Enable/disable optional features
   - Adjust visual feedback settings

4. **Test Setup**:
   ```
   Right-click script → "Test Prefab Setup"
   ```

#### Configuration Options
```csharp
[Header("EnhancedGestureInteractor Settings")]
public LayerMask grabbableLayer = 6; // Default grabbable layer
public bool enableHoverFeedback = true;
public bool enableAudioFeedback = true;
public bool enableSpatialAnchoring = true;
```

### Method 2: Manual Setup

#### Step 1: Add Required Components
1. **Add Collider**:
   ```
   Add Component → Physics → Box Collider
   ```
   - Configure size to match object
   - Set `Is Trigger` to `false`

2. **Add Rigidbody**:
   ```
   Add Component → Physics → Rigidbody
   ```
   - Configure settings as described above

3. **Set Layer**:
   ```
   Inspector → Layer → Grabbable (or custom layer)
   ```

#### Step 2: Configure ObjectManager Integration
1. **Add to Prefab List**:
   ```
   ObjectManager → prefabList → Add your prefab
   ```

2. **Set Default Index** (optional):
   ```
   ObjectManager → defaultPrefabIndex → Set to your prefab index
   ```

3. **Ensure Unique Name**:
   - Prefab name should be unique
   - Used for network synchronization

### Method 3: Prefab Template Creation

#### Create Base Prefab
1. **Create Empty GameObject**
2. **Add EnhancedGesturePrefabSetup script**
3. **Run auto-setup**
4. **Save as prefab**
5. **Use as template** for new objects

## Optional Components

### 1. PolySpatial Volume
**Purpose**: Spatial anchoring for stable positioning
```csharp
// Auto-added by EnhancedGesturePrefabSetup
public Volume spatialVolume;
```
- **Settings**:
  - `Size`: Match object bounds
  - `Enabled`: `true`
  - `Anchoring Behavior`: Configure as needed

### 2. Audio Source
**Purpose**: Interaction feedback sounds
```csharp
// Auto-added by EnhancedGesturePrefabSetup
public AudioSource audioSource;
```
- **Settings**:
  - `Play On Awake`: `false`
  - `Spatial Blend`: `1f` (3D audio)
  - `Volume`: `0.5f`

### 3. Visual Feedback Script
**Purpose**: Visual interaction feedback
```csharp
// Auto-added by EnhancedGesturePrefabSetup
public EnhancedGestureVisualFeedback visualFeedback;
```
- **Features**:
  - Hover color change
  - Grab color change
  - Smooth color transitions

## Integration with ObjectManager

### Prefab List Configuration
```csharp
// In ObjectManager inspector
public List<GameObject> prefabList;
public int defaultPrefabIndex = 0;
```

### Spawn Methods
```csharp
// Spawn default object
string id = objectManager.SpawnObject(position);

// Spawn specific object by index
string id = objectManager.SpawnObject(position, prefabIndex);

// Spawn specific object by name
string id = objectManager.SpawnObject(position, "MyPrefab");
```

### Network Synchronization
- **Automatic**: All spawned objects are synced
- **Parameters**: Position, rotation, scale
- **Real-time**: Updates broadcast to all clients

## Testing Checklist

### Basic Functionality
- [ ] **Thumb-finger tap spawning** works
- [ ] **Hand grab movement** works
- [ ] **Hand grab rotation** works
- [ ] **Dual hand scaling** works

### Network Features
- [ ] **Object spawning** syncs across devices
- [ ] **Position updates** sync in real-time
- [ ] **Rotation updates** sync in real-time
- [ ] **Scale updates** sync in real-time

### Visual/Audio Feedback
- [ ] **Hover feedback** works
- [ ] **Grab feedback** works
- [ ] **Audio feedback** works (if enabled)
- [ ] **Spatial anchoring** works (if enabled)

## Common Issues and Solutions

### Object Not Spawning
**Problem**: Thumb-finger tap doesn't spawn objects
**Solutions**:
- Check ObjectManager.prefabList has your prefab
- Verify defaultPrefabIndex is correct
- Ensure prefab has required components

### Object Not Grabbable
**Problem**: Hand grab gesture doesn't work
**Solutions**:
- Check collider is not a trigger
- Verify rigidbody settings
- Ensure proper layer configuration
- Check grabDistance in EnhancedGestureInteractor

### Physics Issues
**Problem**: Objects behave unexpectedly
**Solutions**:
- Adjust rigidbody drag values
- Configure collision detection mode
- Set appropriate constraints
- Check interpolation settings

### Network Sync Issues
**Problem**: Objects don't sync across devices
**Solutions**:
- Verify AsyncNetworkManager is connected
- Check network message format
- Ensure object IDs are unique
- Test network connectivity

## Performance Optimization

### Collider Optimization
- Use simple colliders when possible
- Avoid complex mesh colliders
- Optimize collider size

### Rigidbody Optimization
- Limit number of active rigidbodies
- Use appropriate collision detection
- Configure interpolation settings

### Rendering Optimization
- Use LOD systems for complex objects
- Optimize material complexity
- Configure shadow settings

## Best Practices

### Prefab Design
1. **Consistent Setup**: Use templates for consistency
2. **Proper Naming**: Use descriptive, unique names
3. **Component Organization**: Group related components
4. **Documentation**: Document custom behavior

### Performance
1. **Optimize Colliders**: Use appropriate collider types
2. **Limit Rigidbodies**: Only add when necessary
3. **Efficient Rendering**: Optimize materials and meshes
4. **Network Efficiency**: Minimize network traffic

### User Experience
1. **Visual Feedback**: Provide clear interaction cues
2. **Audio Feedback**: Add appropriate sound effects
3. **Smooth Interaction**: Ensure responsive controls
4. **Intuitive Behavior**: Make interactions predictable

## Advanced Configuration

### Custom Interaction Behavior
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
        // Custom grab behavior
    }
    
    private void OnReleased(SelectExitEventArgs args)
    {
        // Custom release behavior
    }
}
```

### Layer Mask Configuration
```csharp
// In EnhancedGestureInteractor
public LayerMask grabbableLayer = 6; // Layer mask for grabbable objects

// In prefab setup
gameObject.layer = 6; // Set to grabbable layer
```

### Network Message Customization
```csharp
// Custom network parameters
var spawnMsg = new NetworkMessage {
    action = "spawn",
    id = objectId,
    objectType = "CustomObject",
    parameters = new JObject {
        ["position"] = new JArray(x, y, z),
        ["rotation"] = new JArray(qx, qy, qz, qw),
        ["scale"] = new JArray(sx, sy, sz),
        ["customProperty"] = "customValue"
    }
};
```

This setup ensures your prefabs work seamlessly with the EnhancedGestureInteractor system while maintaining good performance and user experience. 