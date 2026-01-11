using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically imports sprites with Multiple mode and auto-slices them
/// based on the most fitting grid size for each sprite sheet.
/// </summary>
public class SpriteImportSettings : AssetPostprocessor
{
    // Common sprite frame sizes to detect (width x height)
    // Ordered from largest to smallest for better detection
    private static readonly int[] commonFrameSizes = { 192, 128, 96, 64, 48, 32, 16 };
    
    /// <summary>
    /// Called before a texture is imported - sets up import settings
    /// </summary>
    void OnPreprocessTexture()
    {
        // Only process sprites in specific folders (adjust as needed)
        if (!assetPath.Contains("Tiny Swords") && 
            !assetPath.Contains("Sprites") && 
            !assetPath.Contains("Units"))
        {
            return;
        }
        
        TextureImporter importer = (TextureImporter)assetImporter;
        
        // Set texture type to Sprite
        importer.textureType = TextureImporterType.Sprite;
        
        // Set to Multiple sprite mode for sprite sheets
        importer.spriteImportMode = SpriteImportMode.Multiple;
        
        // Pixel art settings
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100;
        
        // Disable mipmaps for pixel art
        importer.mipmapEnabled = false;
    }

    /// <summary>
    /// Called after a texture is imported - performs the actual slicing
    /// </summary>
    void OnPostprocessTexture(Texture2D texture)
    {
        // Only process sprites in specific folders
        if (!assetPath.Contains("Tiny Swords") && 
            !assetPath.Contains("Sprites") && 
            !assetPath.Contains("Units"))
        {
            return;
        }
        
        TextureImporter importer = (TextureImporter)assetImporter;
        
        // Only slice if it's set to Multiple mode
        if (importer.spriteImportMode != SpriteImportMode.Multiple)
            return;
        
        // Get texture dimensions
        int textureWidth = texture.width;
        int textureHeight = texture.height;
        
        // Detect optimal frame size
        Vector2Int frameSize = DetectOptimalFrameSize(textureWidth, textureHeight);
        
        // Calculate grid dimensions
        int columns = textureWidth / frameSize.x;
        int rows = textureHeight / frameSize.y;
        
        // Don't slice if it's just a single sprite
        if (columns <= 1 && rows <= 1)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            return;
        }
        
        // Create sprite metadata for each frame
        SpriteMetaData[] spriteSheet = new SpriteMetaData[columns * rows];
        
        string baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        int spriteIndex = 0;
        
        // Slice from top-left to bottom-right, row by row
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                SpriteMetaData meta = new SpriteMetaData();
                
                // Calculate position (Unity's Y is bottom-up, so we flip)
                int x = col * frameSize.x;
                int y = textureHeight - ((row + 1) * frameSize.y);
                
                meta.rect = new Rect(x, y, frameSize.x, frameSize.y);
                meta.name = $"{baseName}_{spriteIndex}";
                meta.pivot = new Vector2(0.5f, 0.5f); // Center pivot
                meta.alignment = (int)SpriteAlignment.Center;
                
                spriteSheet[spriteIndex] = meta;
                spriteIndex++;
            }
        }
        
        // Apply the sprite sheet data
        importer.spritesheet = spriteSheet;
        
        Debug.Log($"[SpriteImporter] Auto-sliced '{baseName}': {columns} columns x {rows} rows, frame size: {frameSize.x}x{frameSize.y}");
    }

    /// <summary>
    /// Detects the optimal frame size for a sprite sheet based on its dimensions
    /// </summary>
    private Vector2Int DetectOptimalFrameSize(int textureWidth, int textureHeight)
    {
        int bestWidth = textureWidth;
        int bestHeight = textureHeight;
        int bestScore = 0;
        
        // Try common frame sizes
        foreach (int size in commonFrameSizes)
        {
            // Check if this size divides evenly into width
            if (textureWidth % size == 0)
            {
                int cols = textureWidth / size;
                
                // Check various heights
                foreach (int heightSize in commonFrameSizes)
                {
                    if (textureHeight % heightSize == 0)
                    {
                        int rows = textureHeight / heightSize;
                        
                        // Score based on:
                        // - Prefer square-ish frames
                        // - Prefer reasonable number of frames (not too many, not too few)
                        // - Prefer common sizes
                        int score = 0;
                        
                        // Bonus for square frames
                        if (size == heightSize)
                            score += 100;
                        else
                            score += 50 - Mathf.Abs(size - heightSize) / 2;
                        
                        // Bonus for reasonable frame counts (2-12 frames per row is common)
                        if (cols >= 2 && cols <= 12)
                            score += 50;
                        if (rows >= 1 && rows <= 4)
                            score += 30;
                        
                        // Bonus for larger frame sizes (more detail)
                        score += size / 4;
                        
                        // Penalty for too many small frames
                        if (cols * rows > 64)
                            score -= 50;
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestWidth = size;
                            bestHeight = heightSize;
                        }
                    }
                }
            }
        }
        
        // Fallback: try to find any divisor that makes sense
        if (bestScore == 0)
        {
            bestWidth = FindBestDivisor(textureWidth);
            bestHeight = FindBestDivisor(textureHeight);
        }
        
        return new Vector2Int(bestWidth, bestHeight);
    }

    /// <summary>
    /// Finds the best divisor for a dimension (prefers common sprite sizes)
    /// </summary>
    private int FindBestDivisor(int dimension)
    {
        // First try common sizes
        foreach (int size in commonFrameSizes)
        {
            if (dimension % size == 0 && dimension / size >= 1 && dimension / size <= 16)
            {
                return size;
            }
        }
        
        // Fallback: find a reasonable divisor
        for (int divisor = 2; divisor <= 16; divisor++)
        {
            if (dimension % divisor == 0)
            {
                int frameSize = dimension / divisor;
                if (frameSize >= 16 && frameSize <= 256)
                {
                    return frameSize;
                }
            }
        }
        
        // Last resort: return the full dimension (single sprite)
        return dimension;
    }
}

/// <summary>
/// Editor window to manually re-import and slice sprites
/// </summary>
public class SpriteSlicerWindow : EditorWindow
{
    [MenuItem("Tools/Sprite Auto-Slicer/Re-import All Sprites")]
    public static void ReimportAllSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Tiny Swords (Free Pack)" });
        
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".png") || path.EndsWith(".jpg"))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImporter] Re-imported {count} sprites!");
    }
    
    [MenuItem("Tools/Sprite Auto-Slicer/Re-import Selected Sprites")]
    public static void ReimportSelectedSprites()
    {
        Object[] selectedObjects = Selection.objects;
        int count = 0;
        
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg")))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImporter] Re-imported {count} selected sprites!");
    }
}
