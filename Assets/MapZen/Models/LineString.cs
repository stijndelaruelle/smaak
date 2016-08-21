using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LineString : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_MeshFilter;

    //DEBUG
    //private List<Vector3> m_DebugVertices;

    public void Initialize(List<Vector3> vertices, float width)
    {
        //m_DebugVertices = vertices;
        m_MeshFilter.mesh = CreateMesh(vertices, width);
    }

    private Mesh CreateMesh(List<Vector3> sourceVertices, float width)
    {
        Mesh mesh = new Mesh();

        //Loop trough all the vertices, and create vertices next to it depending on the width and the direction

        Vector3 prevPerpendicular = new Vector3();
        Vector2 prevUV = new Vector2();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < sourceVertices.Count - 1; ++i)
        {
            Vector3 startVertex = sourceVertices[i];
            Vector3 endVertex = sourceVertices[i + 1];

            //Calculate direction
            Vector3 direction = (endVertex - startVertex).normalized;

            //Calculate the perpendicular on this vertor
            Vector3 perpendicular = new Vector3(direction.z, 0.0f, -direction.x);

            //If this point already exists we average it, then normalize & multiply with the width so the rest of the roads doesn't get weird.
            if (i > 0)
            {
                //Average the perpendicular
                Vector3 averagePerpendicular = ((perpendicular + prevPerpendicular) / 2.0f).normalized;

                //Resave
                vertices[i * 2] = (startVertex - (averagePerpendicular * width));
                vertices[(i * 2) + 1] = (startVertex + (averagePerpendicular * width));
            }
            else
            {
                vertices.Add(startVertex - (perpendicular * width));
                vertices.Add(startVertex + (perpendicular * width));

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 1.0f));
            }

            vertices.Add(endVertex - (perpendicular * width));
            vertices.Add(endVertex + (perpendicular * width));

            uvs.Add(new Vector2(i + 1, 0.0f));
            uvs.Add(new Vector2(i + 1, 1.0f));

            //Cache it
            prevPerpendicular = perpendicular;
        }

        //Set the indices
        List<int> indices = new List<int>();

        //We count per 2 as vertices are shared
        for (int i = 0; i < sourceVertices.Count - 1; ++i)
        {
            int i2 = (i * 2);

            indices.Add(i2 + 0);
            indices.Add(i2 + 2);
            indices.Add(i2 + 1);

            indices.Add(i2 + 1);
            indices.Add(i2 + 2);
            indices.Add(i2 + 3);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    //DEBUG
    //private void Update()
    //{
    //    for (int i = 0; i < m_DebugVertices.Count - 1; ++i)
    //    {
    //        Debug.DrawLine(m_DebugVertices[i], m_DebugVertices[i + 1], Color.yellow);
    //    }

    //}
}
