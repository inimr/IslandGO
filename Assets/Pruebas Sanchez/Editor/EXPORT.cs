using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
public class EXPORT : MonoBehaviour
{
     [MenuItem("Tools/Export Terrain As Mesh (OBJ)")]
    static void ExportTerrain()
    {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Selecciona un terrain en la jerarquía");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 size = terrainData.size;

        // Simplificación
        int resolution = terrainData.heightmapResolution;
        int simplification = EditorUtility.DisplayDialogComplex("Simplificación", "Elige nivel de simplificación", "x1 (alta)", "x2 (media)", "x4 (baja)") switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            _ => 2
        };

        int width = resolution;
        int height = resolution;
        int step = simplification;

        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        int vertCountX = width / step;
        int vertCountZ = height / step;
        Vector3[] vertices = new Vector3[vertCountX * vertCountZ];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(vertCountX - 1) * (vertCountZ - 1) * 6];

        // Vértices y UVs
        for (int z = 0; z < vertCountZ; z++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                int index = z * vertCountX + x;
                float xPos = ((float)(x * step) / (width - 1)) * size.x;
                float yPos = heights[z * step, x * step] * size.y;
                float zPos = ((float)(z * step) / (height - 1)) * size.z;
                vertices[index] = new Vector3(xPos, yPos, zPos);
                uvs[index] = new Vector2((float)x / vertCountX, (float)z / vertCountZ);
            }
        }

        // Triángulos
        int t = 0;
        for (int z = 0; z < vertCountZ - 1; z++)
        {
            for (int x = 0; x < vertCountX - 1; x++)
            {
                int i = z * vertCountX + x;

                triangles[t++] = i;
                triangles[t++] = i + vertCountX;
                triangles[t++] = i + vertCountX + 1;

                triangles[t++] = i;
                triangles[t++] = i + vertCountX + 1;
                triangles[t++] = i + 1;
            }
        }

        // Crear Mesh
        Mesh mesh = new Mesh();
        mesh.name = "TerrainMesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Guardar como .obj
        string path = EditorUtility.SaveFilePanel("Guardar como OBJ", "", "TerrainMesh.obj", "obj");
        if (!string.IsNullOrEmpty(path))
        {
            ExportMeshToOBJ(mesh, path);
            Debug.Log("Terrain exportado como mesh .OBJ en: " + path);
        }
    }

    static void ExportMeshToOBJ(Mesh mesh, string path)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("g TerrainMesh");

        foreach (Vector3 v in mesh.vertices)
            sb.AppendLine($"v {v.x} {v.y} {v.z}");

        foreach (Vector3 n in mesh.normals)
            sb.AppendLine($"vn {n.x} {n.y} {n.z}");

        foreach (Vector2 uv in mesh.uv)
            sb.AppendLine($"vt {uv.x} {uv.y}");

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int i1 = mesh.triangles[i] + 1;
            int i2 = mesh.triangles[i + 1] + 1;
            int i3 = mesh.triangles[i + 2] + 1;
            sb.AppendLine($"f {i1}/{i1}/{i1} {i2}/{i2}/{i2} {i3}/{i3}/{i3}");
        }

        File.WriteAllText(path, sb.ToString());
    }
}
