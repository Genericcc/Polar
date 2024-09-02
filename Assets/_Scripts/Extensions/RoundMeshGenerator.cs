using UnityEngine;

using System.Collections.Generic;

public class RoundMeshGenerator : MonoBehaviour
{
    public void CreateRoundMesh(float radius, int sides)
    {
        var mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Vertices
        var vertices = new Vector3[sides + 1];
        vertices[0] = Vector3.zero;

        for (var i = 0; i < sides; i++)
        {
            var x = radius * Mathf.Sin((2 * Mathf.PI * i) / sides);
            var z = radius * Mathf.Cos((2 * Mathf.PI * i) / sides);
            vertices[i + 1] = new Vector3(x, 0f, z);
        }

        // Triangles
        var triangles = new int[sides * 3];

        for (var i = 0; i < sides; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = (i + 1) % sides + 1;
            triangles[i * 3 + 2] = (i + 2) % sides + 1;
        }

        // Normals (all facing forward)
        var normals = new Vector3[sides + 1];

        for (var i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        // Uvs
        var uvs = new Vector2[sides + 1];
        uvs[0] = new Vector2(0.5f, 0.5f); // Center vertex

        for (var i = 0; i < sides; i++)
        {
            uvs[i + 1] = new Vector2(vertices[i + 1].x / (radius * 2) + 0.5f, vertices[i + 1].z / (radius * 2) + 0.5f);
        }

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        // Assign to collider
        var collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }
}