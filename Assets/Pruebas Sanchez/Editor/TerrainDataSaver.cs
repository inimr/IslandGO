using UnityEditor;
using UnityEngine;

public class TerrainDataSaver
{
    [MenuItem("Tools/Guardar TerrainData del Terrain seleccionado")]
    public static void SaveTerrainData()
    {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Selecciona un Terrain con TerrainData.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Guardar TerrainData",
            "NewTerrainData",
            "asset",
            "Elige dónde guardar el TerrainData"
        );

        if (string.IsNullOrEmpty(path)) return;

        TerrainData newData = Object.Instantiate(terrain.terrainData);
        AssetDatabase.CreateAsset(newData, path);
        AssetDatabase.SaveAssets();

        terrain.terrainData = newData;
        Debug.Log("TerrainData guardado correctamente en: " + path);
    }
}
