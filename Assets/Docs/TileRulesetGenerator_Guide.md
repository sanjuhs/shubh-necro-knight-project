# Tile Ruleset Generator Tool

## Overview
The **Tile Ruleset Generator** is an editor tool that simplifies the creation of tile rulesets from a set of 9 sprites. It's designed for 2D tile-based games using Unity's Tilemap system.

## What is a 9-Tile Ruleset?
A 9-tile ruleset consists of:
- **1 Center** - The main tile used for filled areas
- **4 Edges** - Top, Bottom, Left, Right (for borders)
- **4 Corners** - Top-Left, Top-Right, Bottom-Left, Bottom-Right (for corners)

This pattern is commonly used for creating seamless tileable terrain, walls, and other tilemap features.

## How to Use

### Step 1: Open the Tile Ruleset Generator
1. In Unity Editor, go to **Window → Tile Ruleset Generator**
2. A new window will open

### Step 2: Prepare Your Sprites
Make sure you have 9 appropriately named sprites:
- `center` - The main/fill tile
- `top` - Top edge tile
- `bottom` - Bottom edge tile
- `left` - Left edge tile
- `right` - Right edge tile
- `top_left` - Top-left corner tile
- `top_right` - Top-right corner tile
- `bottom_left` - Bottom-left corner tile
- `bottom_right` - Bottom-right corner tile

### Step 3: Assign Sprites in the Generator
In the Tile Ruleset Generator window:
1. Drag and drop the **center** sprite into the "Center Tile" field
2. Drag the 4 edge sprites into their respective fields (Top, Bottom, Left, Right)
3. Drag the 4 corner sprites into their respective fields

### Step 4: Name Your Ruleset
Enter a name for your ruleset in the "Ruleset Name" field (e.g., "GrassRuleset", "WallRuleset")

### Step 5: Generate
Click the **"Generate Ruleset"** button

### Step 6: Find Your Assets
The generated assets will be saved to:
```
Assets/Tiles/TileRulesets/
```

Two files will be created:
- `{RulesetName}.asset` - The RuleTile asset (for Unity's tilemap system)
- `{RulesetName}_Data.asset` - The sprite data container

## Output Files

### RuleTile Asset
- Used directly by Unity's Tilemap system
- Can be painted onto tilemaps in the editor
- Automatically handles edge/corner rendering based on neighboring tiles

### TileRulesetData Asset
- Contains all 9 sprite references
- Can be used by game scripts to access tiles programmatically
- Provides utility methods like `GetSprite()` and `IsComplete()`

## Accessing Rulesets in Scripts

### Using TileRulesetData
```csharp
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private TileRulesetData grassRuleset;
    
    void Start()
    {
        // Get a specific sprite
        Sprite centerSprite = grassRuleset.GetSprite(TilePosition.Center);
        
        // Get all sprites
        Sprite[] allSprites = grassRuleset.GetAllSprites();
        
        // Check if complete
        if (grassRuleset.IsComplete())
        {
            Debug.Log("Ruleset is ready to use!");
        }
    }
}
```

## Directory Structure
After generating rulesets, your folder structure will look like:
```
Assets/
└── Tiles/
    └── TileRulesets/
        ├── GrassRuleset.asset
        ├── GrassRuleset_Data.asset
        ├── WallRuleset.asset
        └── WallRuleset_Data.asset
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Missing Sprite" error | Ensure all 9 sprite fields are filled before generating |
| Assets folder doesn't have TileRulesets | The tool will create the folder automatically if it's missing |
| Can't find the Assets/Tiles folder | Create it manually or use Assets/Editor/Create Tiles Folder context menu |
| Generated tileset doesn't look right | Make sure your 9 sprites follow a consistent size and naming convention |

## Best Practices

1. **Consistent Sprite Size** - All 9 sprites should be the same dimensions
2. **Clear Naming** - Use descriptive names for your rulesets (e.g., "GrassGround", "StoneWall")
3. **Organization** - Keep sprites grouped by tileset type in your Assets/Sprites folder
4. **Pixel Perfect** - For pixel-art games, ensure sprites align perfectly with no gaps
5. **Backup** - Save your ruleset assets in version control

## Advanced Usage

### Creating Multiple Rulesets
Repeat the process for different tile types:
- Create a "WallRuleset" from wall sprites
- Create a "WaterRuleset" from water sprites
- Create a "GrassRuleset" from grass sprites

Each ruleset is independent and can be used on different layers or tilemaps.

### Custom Tile Logic
Extend `TileRulesetData` for game-specific properties:
```csharp
public class GameTileRulesetData : TileRulesetData
{
    [SerializeField] public bool isWalkable = true;
    [SerializeField] public int harvestValue = 0;
}
```

## File Locations
- **Generator Script:** `Assets/Editor/TileRulesetGenerator.cs`
- **Data Class:** `Assets/Scripts/TileRulesetData.cs`
- **Generated Assets:** `Assets/Tiles/TileRulesets/`
