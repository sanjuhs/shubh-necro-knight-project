#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SpriteAutoSlicerContext
{
    // Common sprite frame sizes (largest → smallest)
    private static readonly int[] commonFrameSizes = { 192, 128, 96, 64, 48, 32, 16 };

    // ----------------------------
    // CONTEXT MENU ENTRY
    // ----------------------------
    [MenuItem("Assets/Sprite Tools/Auto Slice (Grid Detect)", true)]
    private static bool ValidateAutoSlice()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (path.EndsWith(".png") || path.EndsWith(".jpg"))
                return true;
        }
        return false;
    }

    [MenuItem("Assets/Sprite Tools/Auto Slice (Grid Detect)")]
    private static void AutoSliceSelected()
    {
        int processed = 0;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".png") && !path.EndsWith(".jpg"))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            // -------- IMPORT SETTINGS --------
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100;

            // Load texture to read size
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
                continue;

            Vector2Int frameSize = DetectOptimalFrameSize(texture.width, texture.height);

            int columns = texture.width / frameSize.x;
            int rows = texture.height / frameSize.y;

            if (columns <= 1 && rows <= 1)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                continue;
            }

            List<SpriteMetaData> sprites = new List<SpriteMetaData>();
            string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            int index = 0;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    SpriteMetaData meta = new SpriteMetaData();

                    int x = col * frameSize.x;
                    int y = texture.height - ((row + 1) * frameSize.y);

                    meta.rect = new Rect(x, y, frameSize.x, frameSize.y);
                    meta.name = $"{baseName}_{index}";
                    meta.pivot = new Vector2(0.5f, 0.5f);
                    meta.alignment = (int)SpriteAlignment.Center;

                    sprites.Add(meta);
                    index++;
                }
            }

            importer.spritesheet = sprites.ToArray();
            importer.SaveAndReimport();

            Debug.Log(
                $"[SpriteAutoSlicer] '{baseName}' sliced → {columns}x{rows} @ {frameSize.x}x{frameSize.y}"
            );

            processed++;
        }

        Debug.Log($"[SpriteAutoSlicer] Completed. Processed {processed} texture(s).");
    }

    // ----------------------------
    // FRAME SIZE DETECTION
    // ----------------------------
    private static Vector2Int DetectOptimalFrameSize(int width, int height)
    {
        int bestScore = 0;
        Vector2Int best = new Vector2Int(width, height);

        foreach (int w in commonFrameSizes)
        {
            if (width % w != 0) continue;

            foreach (int h in commonFrameSizes)
            {
                if (height % h != 0) continue;

                int cols = width / w;
                int rows = height / h;

                int score = 0;

                if (w == h) score += 100;
                score += Mathf.Clamp(50 - Mathf.Abs(w - h), 0, 50);

                if (cols >= 2 && cols <= 12) score += 40;
                if (rows >= 1 && rows <= 6) score += 30;

                score += w / 4;

                if (cols * rows > 64) score -= 40;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = new Vector2Int(w, h);
                }
            }
        }

        return best;
    }
}
#endif
