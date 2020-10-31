 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int mapWidth = heightMap.GetLength(0);
        int mapHeight= heightMap.GetLength(1);
        float topLeftX = (mapWidth - 1) / -2f;
        float topLeftZ = (mapHeight - 1) / 2f;


        MeshData meshData = new MeshData(mapWidth, mapHeight);
        int vertexIndex = 0;

        for(int i = 0; i < mapHeight; ++i)
        {
            for (int j = 0; j < mapWidth; ++j)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + j, heightCurve.Evaluate(heightMap[j, i]) * heightMultiplier, topLeftZ - i);
                meshData.uvs[vertexIndex] = new Vector2(j / (float)mapWidth, i / (float)mapHeight);

                if (j < mapWidth - 1 && i < mapHeight - 1)
                {
                    meshData.addTriangle(vertexIndex, vertexIndex + mapWidth + 1, vertexIndex + mapWidth);
                    meshData.addTriangle(vertexIndex + mapWidth + 1, vertexIndex, vertexIndex + 1);
                }

                ++vertexIndex;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void addTriangle(int x, int y, int z)
    {
        triangles[triangleIndex] = x;
        triangles[triangleIndex + 1] = y;
        triangles[triangleIndex + 2] = z;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
