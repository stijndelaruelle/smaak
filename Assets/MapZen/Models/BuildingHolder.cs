using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BuildingHolder
{
    public GameObject GameObject { get; set; }
    public Vector3 Center { get; set; }
    public float Rotation { get; set; }
    public bool IsModelCreated;
    private List<Vector3> _verts;

    public BuildingHolder(Vector3 center, List<Vector3> verts)
    {
        IsModelCreated = false;
        Center = center;
        _verts = verts;
    }

    public GameObject CreateModel()
    {
        if (IsModelCreated)
            return null;

        var m = new GameObject().AddComponent<Polygon>();
        m.gameObject.transform.position = Center;
        m.Initialize(_verts);
        IsModelCreated = true;
        return m.gameObject;
    }
}
