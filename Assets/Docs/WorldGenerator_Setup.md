# WorldGenerator Script Setup Guide

## Overview
The `WorldGenerator` script replaces the previous `CheckerboardBackground` script and uses Unity's **Tilemap system** to generate a procedurally sized world based on configurable width and height parameters.

## Prerequisites
- Unity 2021 LTS or higher (with 2D Tilemap support)
- The 2D Tilemap Renderer package installed (usually included by default)
- A tile asset (`.asset` file) to use for world generation

## Setup Instructions

### Step 1: Prepare Your Tilemap System
1. In your Unity scene, create a new GameObject for your world:
   - Right-click in Hierarchy → 2D Object → Tilemap → Rectangular
   - This creates a Grid parent object with a Tilemap child

2. Name the Tilemap object appropriately (e.g., "World" or "GameWorld")

3. Select the Tilemap object in the Hierarchy

### Step 2: Add the WorldGenerator Component
1. With the Tilemap selected, go to Inspector → Add Component
2. Search for and add the `WorldGenerator` script
3. The script will automatically detect the `Tilemap` component on the same GameObject

### Step 3: Configure WorldGenerator Settings
In the Inspector, you'll see the following sections:

#### World Settings
- **World Width** (default: 20)
  - Sets the horizontal size of your world in grid units
  - Adjust based on your desired game area
  
- **World Height** (default: 15)
  - Sets the vertical size of your world in grid units
  - Adjust based on your desired game area

#### Tile Settings
- **Tile Prefab**
  - Assign your Tile asset here (drag from Assets folder)
  - This tile will be used when populating the tilemap

#### Editor Settings
- **Auto Update In Editor** (default: true)
  - When enabled, changes to World Width/Height will automatically regenerate the world
  - Uncheck if you want manual control via context menu

### Step 4: Generate the World
You have two options:

**Option A: Automatic (Recommended)**
- Simply adjust the World Width or World Height values
- The world will regenerate automatically if "Auto Update In Editor" is enabled

**Option B: Manual**
- Right-click the WorldGenerator component in Inspector
- Select "Generate World" or "Force Regenerate" from the context menu

### Step 5: Populate Your Tilemap (Optional)
The `GenerateWorld()` method creates the tilemap structure. To add tiles:

1. Use the Tilemap Painter tool in Unity Editor, OR
2. Extend the `GenerateWorld()` method in the script to programmatically place tiles

Example code extension (add to `GenerateWorld()` method):
```csharp
int gridWidth = Mathf.RoundToInt(worldWidth);
int gridHeight = Mathf.RoundToInt(worldHeight);

for (int x = 0; x < gridWidth; x++)
{
    for (int y = 0; y < gridHeight; y++)
    {
        // Place your tile logic here
        tilemap.SetTile(new Vector3Int(x, y, 0), tilePrefab);
    }
}
```

## Public Properties & Methods

### Properties
- `WorldWidth` - Get the current world width in units
- `WorldHeight` - Get the current world height in units

### Methods
- `GetWorldBounds()` - Returns a Bounds object representing the world's dimensions
- `GetBoundaryCoordinates(out float minX, out float maxX, out float minY, out float maxY)` - Gets min/max world coordinates
- `GenerateWorld()` - Generates/regenerates the world structure
- `ForceRegenerate()` - Forces a complete regeneration and marks the scene as dirty

## Usage in Other Scripts

```csharp
using UnityEngine;

public class MyGameScript : MonoBehaviour
{
    private WorldGenerator worldGenerator;
    
    void Start()
    {
        worldGenerator = GetComponent<WorldGenerator>();
        
        // Get world bounds for gameplay logic
        Bounds worldBounds = worldGenerator.GetWorldBounds();
        
        // Get boundary coordinates
        worldGenerator.GetBoundaryCoordinates(
            out float minX, out float maxX, 
            out float minY, out float maxY
        );
        
        Debug.Log($"World Size: {worldGenerator.WorldWidth} x {worldGenerator.WorldHeight}");
    }
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Missing Tilemap component" error | Ensure the GameObject has both a Grid and Tilemap component. Add manually if needed. |
| World not updating in Editor | Check that "Auto Update In Editor" is enabled in the Inspector |
| Tiles not appearing | Assign a valid Tile asset to the "Tile Prefab" field and call "Generate World" |
| Script not found | Ensure WorldGenerator.cs is in the Assets/Scripts folder |

## File Location
- **Script:** `Assets/Scripts/WorldGenerator.cs`
- **Old Script (Remove):** `Assets/Scripts/CheckerboardBackground.cs` (if still present)

## Migration Notes
If you were using the old `CheckerboardBackground` script:
1. Delete or backup the old CheckerboardBackground.cs file
2. Replace the component reference with WorldGenerator on affected GameObjects
3. Add a Tilemap component if not already present
4. Delete the SpriteRenderer component from the GameObject
5. Reconfigure the world size settings as needed

## Next Steps
- Implement tile placement logic in the `GenerateWorld()` method
- Create a tile variation system for different terrain types
- Add procedural generation logic based on your game design
- Optimize tilemap rendering for your target platform
