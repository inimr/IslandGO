using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainToMAP
{
    [MenuItem("Tools/Export Terrain Heightmap as PNG")]
    static void ExportHeightmap()
    {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Error", "Selecciona un Terrain en la jerarquía.", "OK");
            return;
        }

        TerrainData data = terrain.terrainData;
        int width = data.heightmapResolution;
        int height = data.heightmapResolution;
        float[,] heights = data.GetHeights(0, 0, width, height);

        Texture2D tex = new Texture2D(width, height, TextureFormat.R16, false, true);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = heights[y, x];
                tex.SetPixel(x, y, new Color(v, v, v));
            }
        }

        tex.Apply();

        string path = EditorUtility.SaveFilePanel("Guardar heightmap", "", "Heightmap.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            Debug.Log("Heightmap exportado a: " + path);
        }

        Object.DestroyImmediate(tex);
    } 
}
