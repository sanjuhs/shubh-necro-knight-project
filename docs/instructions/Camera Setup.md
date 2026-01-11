# Camera Follow Setup Instructions

This guide explains how to set up the smooth camera follow system with boundary clamping.

## Overview

The `CameraFollow` script provides:

- **Smooth following** - Camera smoothly tracks the player
- **Boundary clamping** - Camera stops at map edges (doesn't show beyond the map)
- **Automatic viewport adjustment** - Boundaries account for camera size
- **Visual debugging** - See boundaries in Scene view

---

## Quick Setup (5 Steps)

### Step 1: Add the Script to Main Camera

1. Select **Main Camera** in the Hierarchy
2. Click **Add Component** in the Inspector
3. Search for **CameraFollow** and add it

### Step 2: Assign the Target

**Option A: Automatic (Recommended)**

- Tag your Warrior as "Player":
  1. Select the **Warrior** GameObject
  2. In Inspector, click the **Tag** dropdown
  3. Select **Player** (or create it if it doesn't exist)
- The camera will auto-find it!

**Option B: Manual**

- Drag your **Warrior** GameObject into the **Target** field in the CameraFollow component

### Step 3: Set the Camera Offset

The default offset is `(0, 0, -10)` which works for 2D.

| Setting | Value | Description                                                  |
| ------- | ----- | ------------------------------------------------------------ |
| X       | 0     | Horizontal offset from player                                |
| Y       | 0     | Vertical offset (set positive to look slightly above player) |
| Z       | -10   | Distance from the 2D plane (keep negative!)                  |

### Step 4: Configure Boundaries

Set the map boundaries based on your level size:

| Setting   | Description             |
| --------- | ----------------------- |
| **Min X** | Left edge of your map   |
| **Max X** | Right edge of your map  |
| **Min Y** | Bottom edge of your map |
| **Max Y** | Top edge of your map    |

**Tip:** Look at your tilemap/background in Scene view and note the coordinates of the corners!

### Step 5: Adjust Smoothness

| Smooth Speed | Feel                                 |
| ------------ | ------------------------------------ |
| 0.05         | Very smooth, cinematic, slight delay |
| 0.125        | Default - balanced smoothness        |
| 0.3          | Responsive, slight smoothing         |
| 1.0          | Nearly instant, snappy               |

---

## Inspector Settings Reference

```
┌─────────────────────────────────────────────────────────┐
│ Camera Follow (Script)                                  │
├─────────────────────────────────────────────────────────┤
│ Target Settings                                         │
│   Target: [Warrior]              ← Your player          │
│   Offset: (0, 0, -10)            ← Camera offset        │
├─────────────────────────────────────────────────────────┤
│ Smoothing Settings                                      │
│   Smooth Speed: 0.125            ← Lower = smoother     │
│   Use Unscaled Time: [ ]         ← Check for pause menu │
├─────────────────────────────────────────────────────────┤
│ Boundary Settings                                       │
│   Use Boundaries: [✓]            ← Enable clamping      │
│   Min X: -20                     ← Left edge            │
│   Max X: 20                      ← Right edge           │
│   Min Y: -15                     ← Bottom edge          │
│   Max Y: 15                      ← Top edge             │
├─────────────────────────────────────────────────────────┤
│ Debug                                                   │
│   Show Boundary Gizmos: [✓]      ← See in Scene view    │
│   Boundary Color: Yellow         ← Gizmo color          │
└─────────────────────────────────────────────────────────┘
```

---

## How Boundary Clamping Works

The camera automatically accounts for its viewport size:

```
     Map Boundaries (what you set)
     ┌─────────────────────────────────────┐
     │                                     │
     │   Camera Clamp Zone                 │
     │   ┌─────────────────────────┐       │
     │   │                         │       │
     │   │   Player can move here  │       │
     │   │   Camera center stays   │       │
     │   │   in this zone          │       │
     │   │                         │       │
     │   └─────────────────────────┘       │
     │         ↑                           │
     │    Camera viewport                  │
     │    accounts for this                │
     └─────────────────────────────────────┘
```

The camera's **center** is clamped so that the **edges** of the viewport never exceed the map boundaries.

---

## Finding Your Map Boundaries

### Method 1: Using Scene View

1. Open Scene view
2. Navigate to the corners of your map/tilemap
3. Look at the position in the bottom of Scene view
4. Note the X and Y values for each corner

### Method 2: Using a Background Sprite

If you have a background:

1. Select the background sprite
2. Check its Transform position and SpriteRenderer bounds
3. Calculate: `Min = Position - (Size/2)`, `Max = Position + (Size/2)`

### Method 3: Using Tilemap

If using a Tilemap:

1. Select the Tilemap
2. Go to **Tilemap → Compress Bounds** in the menu
3. Check the Tilemap's **Cell Bounds** in the Inspector

---

## Runtime API

You can control the camera from other scripts:

```csharp
// Get reference to camera
CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

// Change target
cameraFollow.SetTarget(newTarget.transform);

// Update boundaries (e.g., when entering a new room)
cameraFollow.SetBoundaries(-30f, 30f, -20f, 20f);

// Instantly snap to target (no smoothing)
cameraFollow.SnapToTarget();
```

---

## Troubleshooting

### Camera doesn't follow player

- Check that **Target** is assigned
- Make sure the player has the "Player" tag if using auto-detection
- Verify the script is on the **Main Camera**

### Camera shows beyond map edges

- Check that **Use Boundaries** is enabled
- Verify your boundary values are correct
- Make sure the camera's orthographic size isn't too large for your boundaries

### Camera movement is jerky

- Increase **Smooth Speed** slightly
- Make sure you're not also moving the camera from another script

### Boundaries are too tight

- The script accounts for camera viewport size
- If `maxX - minX` is smaller than the camera's width, the camera won't move horizontally
- Increase your boundary range or decrease camera orthographic size

### Can't see boundary gizmos

- Enable **Show Boundary Gizmos** in the component
- Make sure **Gizmos** are enabled in the Scene view (top-right toggle)

---

## Example Configuration

For a typical small level:

```
Target: Warrior
Offset: (0, 1, -10)      ← Slightly above player
Smooth Speed: 0.1
Use Boundaries: ✓
Min X: -25
Max X: 25
Min Y: -15
Max Y: 15
```

For a larger exploration level:

```
Target: Warrior
Offset: (0, 0, -10)
Smooth Speed: 0.08       ← Smoother for exploration
Use Boundaries: ✓
Min X: -100
Max X: 100
Min Y: -50
Max Y: 50
```
