using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorldDetail : MonoBehaviour
{
    //Split up only done for easier editing afterwards
    public enum WaterType
    {
        //Point/Polygon
        Basin,
        Dock,
        Lake,
        Ocean,
        Playa,
        Riverbank,
        SwimmingPool,
        Water,

        //Point only
        //Bay,
        //Fjord,
        //Strait,
        //Sea,

        //Line only
        Canal,
        Dam,
        Ditch,
        Drain,
        River,
        Stream,

        Undefined
    }

    public enum RoadType
    {
        Highway,
        MajorRoad,
        MinorRoad,
        Rail,
        Path,
        BicycleRoad,
        Undefined

        //Ferry
        //Piste
        //Aerialway
        //Exit
        //Racetrack
        //PortageWay
    }

    public enum BuildingType
    {
        Default,
        Undefined
    }

    [Header("Types")]
    [Tooltip("Put these in the order of the WaterType enum!")]
    [SerializeField]
    private List<WorldDetailDefinition> m_WaterDefinitions;

    [Tooltip("Put these in the order of the RoadType enum!")]
    [SerializeField]
    private List<WorldDetailDefinition> m_RoadDefinitions;

    [Tooltip("Put these in the order of the BuildingType enum!")]
    [SerializeField]
    private List<WorldDetailDefinition> m_BuildingDefinitions;

    [Header("Models")]
    [SerializeField]
    private LineString m_LineStringPrefab;

    [SerializeField]
    private Polygon m_PolygonPrefab;


    private WorldDetailDefinition m_MyDefinition;


    //Functions
    public void Initialize(WaterType waterType, int drawOrder)
    {
        m_MyDefinition = m_WaterDefinitions[(int)waterType];
        SetDrawOrder(drawOrder);
    }

    public void Initialize(RoadType roadType, int drawOrder)
    {
        m_MyDefinition = m_RoadDefinitions[(int)roadType];
        SetDrawOrder(drawOrder);
    }

    public void Initialize(BuildingType buildingType, int drawOrder)
    {
        m_MyDefinition = m_BuildingDefinitions[(int)buildingType];
        SetDrawOrder(drawOrder);
    }


    public void CreateLineString(List<Vector3> vertices)
    {
        //Add a new LineString component & parent it to this object
        LineString lineString = GameObject.Instantiate(m_LineStringPrefab) as LineString;
        lineString.name = name;

        lineString.transform.parent = this.transform;
        lineString.transform.position = this.transform.position;

        Renderer renderer = lineString.GetComponent<Renderer>();
        if (renderer != null) { renderer.material = m_MyDefinition.Material; }

        try
        {
            lineString.Initialize(vertices, m_MyDefinition.Width);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void CreatePolygon(List<Vector3> vertices)
    {
        //Add a new Polygon component & parent it to this object
        Polygon polygon = GameObject.Instantiate(m_PolygonPrefab) as Polygon;
        polygon.name = name;

        polygon.transform.parent = this.transform;
        polygon.transform.position = this.transform.position;

        Renderer renderer = polygon.GetComponent<Renderer>();
        if (renderer != null) { renderer.material = m_MyDefinition.Material; }

        try
        {
            polygon.Initialize(vertices);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private void SetDrawOrder(int drawOrder)
    {
        transform.Translate(new Vector3(0.0f, drawOrder, 0.0f));
    }
}
