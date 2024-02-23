using System.Collections.Generic;

using com.cyborgAssets.inspectorButtonPro;

using UnityEngine;

namespace _Scripts.Extensions
{
    //[RequireComponent(typeof(PolygonCollider2D))]
    public class RoundMeshGenerator : MonoBehaviour
    {
        public void CreateRoundMesh(float radius, int sides)
        {
            var mf = GetComponent<MeshFilter>();
            var mesh = new Mesh();
            mf.mesh = mesh;

            //verticies
            var verticesList = new List<Vector3>();

            for (var i = 0; i < sides; i ++)
            {
                var x = radius * Mathf.Sin((2 * Mathf.PI * i) / sides);
                var z = radius * Mathf.Cos((2 * Mathf.PI * i) / sides);
                verticesList.Add(new Vector3(x, z, 0f));
            }
            var vertices = verticesList.ToArray();

            //triangles
            var trianglesList = new List<int> { };
            for(var i = 0; i < (sides-2); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i+1);
                trianglesList.Add(i+2);
            }
            var triangles = trianglesList.ToArray();

            //normals
            var normalsList = new List<Vector3> { };
            for (var i = 0; i < vertices.Length; i++)
            {
                normalsList.Add(-Vector3.forward);
            }
            var normals = normalsList.ToArray();
            
            //uvs
            // Vector2[] uvs = new Vector2[vertices.Length];
            // for (int i = 0; i < uvs.Length; i++)
            // {
            //     uvs[i] = new Vector2(vertices[i].x / (radius*2) + 0.5f, vertices[i].y / (radius*2) + 0.5f);
            // }
            //mesh.uv = uvs;
            
            //initialise
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            
            //collider
            var mc = GetComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }
    }
}