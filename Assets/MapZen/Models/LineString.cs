using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Helpers;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineString : MonoBehaviour
{
    [SerializeField]
    private LineRenderer m_LineRenderer;

    public void Initialize(List<Vector3> vertices, float width)
    {
        m_LineRenderer.SetVertexCount(vertices.Count);
        m_LineRenderer.SetPositions(vertices.ToArray());

        m_LineRenderer.SetWidth(width, width);
    }
}
