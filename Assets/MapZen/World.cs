using Assets;
using UnityEngine;
using System.Collections;
using Assets.Helpers;
using System.Collections.Generic;

public class World : Singleton<World>
{
    [SerializeField]
    private Tile m_TilePrefab;

    [SerializeField]
    private int m_Zoom = 0;

    [SerializeField] private bool m_Earth;
    [SerializeField] private bool m_Water;
    [SerializeField] private bool m_Roads;
    [SerializeField] private bool m_Landuse;
    [SerializeField] private bool m_Buildings;
    private Tile.TileDetail m_Detail;

    private bool m_IsInitialized = false;

    private Vector2 m_CentralMapID;
    private Rect m_CentralMapSize;

    //These never change as we constantly center the world
    private Rect m_Boundries;

    //The 9 tiles
    // 0 3 6
    // 1 4 7
    // 2 5 8
    private List<Tile> m_Tiles;

    protected override void Awake()
    {
        base.Awake();
        m_Tiles = new List<Tile>();
    }

	private void Start() 
    {
        GPSManager.Instance.GPSInitializedEvent += OnGPSInitialized;
    }

    private void Initialize()
    {
        GPSManager gps = GPSManager.Instance;
        float latitude = gps.GetLatitude();
        float longitude = gps.GetLongitude();

        //Central map ID
        m_CentralMapID = Extensions.WorldToTilePos(latitude, longitude, m_Zoom);
        m_CentralMapSize = GM.TileBounds(m_CentralMapID, m_Zoom);

        m_Detail = 0x0;
        if (m_Earth) m_Detail |= Tile.TileDetail.Earth;
        if (m_Water) m_Detail |= Tile.TileDetail.Water;
        if (m_Roads) m_Detail |= Tile.TileDetail.Roads;
        if (m_Landuse) m_Detail |= Tile.TileDetail.Landuse;
        if (m_Buildings) m_Detail |= Tile.TileDetail.Buildings;

        //Generate 9 maps
        //for (int x = 0; x < 1; ++x)
        //{
        //   for (int y = 0; y < 1; ++y)
        //    {
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                Vector2 currentMapID = new Vector2(m_CentralMapID.x + x, m_CentralMapID.y + y);

                Tile tile = CreateTile(currentMapID);
                m_Tiles.Add(tile);
            }
        }

        m_Boundries.xMin = -m_CentralMapSize.width;
        m_Boundries.yMin = -m_CentralMapSize.height;
        m_Boundries.xMax = m_Boundries.xMin + (m_CentralMapSize.width * 2);
        m_Boundries.yMax = m_Boundries.yMin + (m_CentralMapSize.height * 2);

        m_IsInitialized = true;
    }

    private void Update()
    {
        if (!m_IsInitialized)
            return;

        Vector3 left = Camera.main.WorldToViewportPoint(new Vector3(m_Boundries.xMin + transform.position.x, 0.0f, 0.0f));
        Vector3 right = Camera.main.WorldToViewportPoint(new Vector3(m_Boundries.xMax + transform.position.x, 0.0f, 0.0f));
        Vector3 bottom = Camera.main.WorldToViewportPoint(new Vector3(0.0f, 0.0f, m_Boundries.yMax + transform.position.z));
        Vector3 top = Camera.main.WorldToViewportPoint(new Vector3(0.0f, 0.0f, m_Boundries.yMin + transform.position.z));

        //You can probably make an algorithm so that it works for all 4 sides.
        //This however would completely obfuscate the simple thing that we are actually doing. (Therefore I have chosen not to spend time on it)
        if      (left.x > 0.2f)   { MoveLeft(); }
        else if (right.x < 0.8f)  { MoveRight(); }
        else if (top.y < 0.8f)    { MoveUp(); }
        else if (bottom.y > 0.2f) { MoveDown(); }
    }

    private void RecenterWorld(Vector2 offset)
    {
        //Make sure that our worlds stays somewhat centered and we don't get insane position coords (can freak out)

        //Move all our tiles 1 step to the right (locally)
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).transform.Translate(-offset.x, 0.0f, -offset.y);
        }

        //Move our world transform 1 step to the left
        transform.Translate(offset.x, 0, offset.y);
    }

    private void MoveLeft()
    {
        Debug.Log("Add new on the left");

        //Move our pivot to the new central map
        m_CentralMapID.x -= 1;

        RecenterWorld(new Vector2(-m_CentralMapSize.width, 0.0f));

        //These could all be loops, but in my opinion it really obfuscates the simple operation we are doing.
        //Writing them all the way makes it easier to visualize what's going on (and it doesn't impact performance at all).

        //Remove the 3 right maps
        GameObject.Destroy(m_Tiles[8].gameObject);
        GameObject.Destroy(m_Tiles[7].gameObject);
        GameObject.Destroy(m_Tiles[6].gameObject);

        //Move all remaining maps in the array to the right
        m_Tiles[8] = m_Tiles[5];
        m_Tiles[7] = m_Tiles[4];
        m_Tiles[6] = m_Tiles[3];
        m_Tiles[5] = m_Tiles[2];
        m_Tiles[4] = m_Tiles[1];
        m_Tiles[3] = m_Tiles[0];

        //Add 3 new maps to the left
        m_Tiles[2] = CreateTile(new Vector2(m_CentralMapID.x - 1, m_CentralMapID.y + 1));
        m_Tiles[1] = CreateTile(new Vector2(m_CentralMapID.x - 1, m_CentralMapID.y + 0));
        m_Tiles[0] = CreateTile(new Vector2(m_CentralMapID.x - 1, m_CentralMapID.y - 1));
    }

    private void MoveRight()
    {
        Debug.Log("Add new on the right");

        //Move our pivot to the new central map
        m_CentralMapID.x += 1;

        RecenterWorld(new Vector2(m_CentralMapSize.width, 0.0f));

        //These could all be loops, but in my opinion it really obfuscates the simple operation we are doing.
        //Writing them all the way makes it easier to visualize what's going on (and it doesn't impact performance at all).

        //Remove the 3 left maps
        GameObject.Destroy(m_Tiles[0].gameObject);
        GameObject.Destroy(m_Tiles[1].gameObject);
        GameObject.Destroy(m_Tiles[2].gameObject);

        //Move all remaining maps in the array to the left
        m_Tiles[0] = m_Tiles[3];
        m_Tiles[1] = m_Tiles[4];
        m_Tiles[2] = m_Tiles[5];
        m_Tiles[3] = m_Tiles[6];
        m_Tiles[4] = m_Tiles[7];
        m_Tiles[5] = m_Tiles[8];

        //Add 3 new maps to the right
        m_Tiles[6] = CreateTile(new Vector2(m_CentralMapID.x + 1, m_CentralMapID.y - 1));
        m_Tiles[7] = CreateTile(new Vector2(m_CentralMapID.x + 1, m_CentralMapID.y + 0));
        m_Tiles[8] = CreateTile(new Vector2(m_CentralMapID.x + 1, m_CentralMapID.y + 1));
    }

    private void MoveUp()
    {
        Debug.Log("Add new on the top");

        //Move our pivot to the new central map
        m_CentralMapID.y -= 1;
        RecenterWorld(new Vector2(0.0f, -m_CentralMapSize.height));

        //These could all be loops, but in my opinion it really obfuscates the simple operation we are doing.
        //Writing them all the way makes it easier to visualize what's going on (and it doesn't impact performance at all).

        //Remove the 3 bottom maps
        GameObject.Destroy(m_Tiles[2].gameObject);
        GameObject.Destroy(m_Tiles[5].gameObject);
        GameObject.Destroy(m_Tiles[8].gameObject);

        //Move all remaining maps in the array down
        m_Tiles[2] = m_Tiles[1];
        m_Tiles[5] = m_Tiles[4];
        m_Tiles[8] = m_Tiles[7];

        m_Tiles[1] = m_Tiles[0];
        m_Tiles[4] = m_Tiles[3];
        m_Tiles[7] = m_Tiles[6];

        //Add 3 new maps to the top
        m_Tiles[0] = CreateTile(new Vector2(m_CentralMapID.x - 1, m_CentralMapID.y - 1));
        m_Tiles[3] = CreateTile(new Vector2(m_CentralMapID.x + 0, m_CentralMapID.y - 1));
        m_Tiles[6] = CreateTile(new Vector2(m_CentralMapID.x + 1, m_CentralMapID.y - 1));
    }

    private void MoveDown()
    {
        Debug.Log("Add new on the bottom");

        //These could all be loops, but in my opinion it really obfuscates the simple operation we are doing.
        //Writing them all the way makes it easier to visualize what's going on (and it doesn't impact performance at all).

        //Move our pivot to the new central map
        m_CentralMapID.y += 1;
        RecenterWorld(new Vector2(0.0f, m_CentralMapSize.height));

        //Remove the 3 top maps
        GameObject.Destroy(m_Tiles[0].gameObject);
        GameObject.Destroy(m_Tiles[3].gameObject);
        GameObject.Destroy(m_Tiles[6].gameObject);

        //Move all remaining maps in the array up
        m_Tiles[0] = m_Tiles[1];
        m_Tiles[3] = m_Tiles[4];
        m_Tiles[6] = m_Tiles[7];

        m_Tiles[1] = m_Tiles[2];
        m_Tiles[4] = m_Tiles[5];
        m_Tiles[7] = m_Tiles[8];

        //Add 3 new maps to the top
        m_Tiles[2] = CreateTile(new Vector2(m_CentralMapID.x - 1, m_CentralMapID.y + 1));
        m_Tiles[5] = CreateTile(new Vector2(m_CentralMapID.x + 0, m_CentralMapID.y + 1));
        m_Tiles[8] = CreateTile(new Vector2(m_CentralMapID.x + 1, m_CentralMapID.y + 1));
    }

    private Tile CreateTile(Vector2 mapID)
    {
        Vector2 diff = mapID - m_CentralMapID;

        Tile tile = GameObject.Instantiate(m_TilePrefab) as Tile;
        tile.name = "Tile x: " + mapID.x + " y: " + mapID.y + " z: " + m_Zoom;

        tile.transform.parent = this.transform;
        tile.transform.position = this.transform.position + new Vector3(diff.x * m_CentralMapSize.width, 0.0f, diff.y * m_CentralMapSize.height);

        tile.Initialize(new Vector2(mapID.x, mapID.y), m_Zoom, m_Detail);

        return tile;
    }

    public Vector2 CalculateOffset(Vector2 mapID)
    {
        Vector2 diff = mapID - m_CentralMapID;
        diff.x *= m_CentralMapSize.width;
        diff.y *= m_CentralMapSize.height;

        return diff;
    }

    //Event callbacks
    private void OnGPSInitialized()
    {
        Initialize();
    }

}
