using Assets;
using UnityEngine;
using System.Collections;
using Assets.Helpers;

public class World : Singleton<World>
{
    [SerializeField]
    private Tile m_TilePrefab;

    [SerializeField]
    private float m_Latitude;

    [SerializeField]
    private float m_Longitude;

    [SerializeField]
    private int m_Zoom = 0;

    [SerializeField] private bool m_Earth;
    [SerializeField] private bool m_Water;
    [SerializeField] private bool m_Roads;
    [SerializeField] private bool m_Landuse;
    [SerializeField] private bool m_Buildings;

    private Vector2 m_CentralMapID;
    private Rect m_CentralMapSize;

	private void Start() 
    {
        //Central map ID
        m_CentralMapID = Extensions.WorldToTilePos(m_Latitude, m_Longitude, m_Zoom);
        m_CentralMapSize = GM.TileBounds(m_CentralMapID, m_Zoom);

        Tile.TileDetail detail = 0x0;
        if (m_Earth) detail |= Tile.TileDetail.Earth;
        if (m_Water) detail |= Tile.TileDetail.Water;
        if (m_Roads) detail |= Tile.TileDetail.Roads;
        if (m_Landuse) detail |= Tile.TileDetail.Landuse;
        if (m_Buildings) detail |= Tile.TileDetail.Buildings;

        //Generate 9 maps
        //for (int x = 0; x < 1; ++x)
        //{
        //    for (int y = 0; y < 1; ++y)
        //    {
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                Vector2 currentMapID = new Vector2(m_CentralMapID.x + x, m_CentralMapID.y + y);

                Tile tile = GameObject.Instantiate(m_TilePrefab) as Tile;
                tile.name = "Tile x: " + currentMapID.x + " y: " + currentMapID.y + " z: " + m_Zoom;

                tile.transform.parent = this.transform;
                tile.transform.position = this.transform.position + new Vector3(x * m_CentralMapSize.width, 0.0f, y * m_CentralMapSize.height);

                tile.Initialize(new Vector2(currentMapID.x, currentMapID.y), m_Zoom, detail);
            }
        }
    }

    public Vector2 CalculateOffset(Vector2 mapID)
    {
        Vector2 diff = mapID - m_CentralMapID;
        diff.x *= m_CentralMapSize.width;
        diff.y *= m_CentralMapSize.height;

        return diff;
    }
}
