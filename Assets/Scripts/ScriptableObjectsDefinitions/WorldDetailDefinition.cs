using UnityEngine;
using System.Collections;

public class WorldDetailDefinition : ScriptableObject
{
    [SerializeField]
    private float m_Width;
    public float Width
    {
        get { return m_Width; }
    }

    [SerializeField]
    private Material m_Material;
    public Material Material
    {
        get { return m_Material; }
    }
}
