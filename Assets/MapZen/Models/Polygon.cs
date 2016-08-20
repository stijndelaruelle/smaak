using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

//FIX ME: Barely touched this script from it's source, code conventions may not apply!!!!
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Polygon : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_MeshFilter;

    public void Initialize(List<Vector3> vertices)
    {
        m_MeshFilter.mesh = CreateMesh(vertices);
    }

    private Mesh CreateMesh(List<Vector3> verts)
    {
        Triangulator triangulator = new Triangulator(verts.Select(x => x.ToVector2xz()).ToArray());
        Mesh mesh = new Mesh();

        List<Vector3> vertices = verts.Select(x => new Vector3(x.x, 0, x.z)).ToList();
        var indices = triangulator.Triangulate().ToList();

        var n = vertices.Count;
        for (int index = 0; index < n; index++)
        {
            var v = vertices[index];
            vertices.Add(new Vector3(v.x, 0, v.z));
        }

        for (int i = 0; i < n - 1; i++)
        {
            indices.Add(i);
            indices.Add(i + n);
            indices.Add(i + n + 1);
            indices.Add(i);
            indices.Add(i + n + 1);
            indices.Add(i + 1);
        }

        indices.Add(n - 1);
        indices.Add(n);
        indices.Add(0);

        indices.Add(n - 1);
        indices.Add(n + n - 1);
        indices.Add(n);



        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
