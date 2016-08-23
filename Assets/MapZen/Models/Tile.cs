using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Helpers;
using UnityEngine;
using SimpleJSON;

public class Tile : MonoBehaviour
{
    public enum TileDetail
    {
        Earth = 0x1,
        Water = 0x2,
        Roads = 0x4,
        Landuse = 0x5,
        Buildings = 0x10
    }

    public enum LineStringType
    {
        Undefined,
        
        //Water
        Canal,
        Dam,
        Ditch,
        Drain,
        River,
        Stream,

        //Roads
        Highway,
        MajorRoad,
        MinorRoad,
        Rail,
        Path
    }

    [SerializeField]
    private LineString m_WaterLinePrefab;

    [SerializeField]
    private Polygon m_WaterPolygonPrefab;

    [SerializeField]
    private LineString m_RoadPrefab;

    [SerializeField]
    private Polygon m_BuildingPrefab;

    private Rect m_Rect;

    #if UNITY_EDITOR
        private Vector2 m_MapID;
        public Vector2 MapID
        {
            get { return m_MapID; }
        }

        private int m_Zoom;
        public int Zoom
        {
            get { return m_Zoom;}
        }
    #endif

    public void Initialize(Vector2 mapID, int zoom, TileDetail detail)
    {
        StartCoroutine(CreateTile(mapID, zoom, detail));
    }

    private IEnumerator CreateTile(Vector2 mapID, int zoom, TileDetail detail)
    {
        //Generate the url
        string dirPath = Application.persistentDataPath;
        string filePath = dirPath + "/" + zoom + "_" + mapID.x + "_" + mapID.y + "_";
        string url = "http://vector.mapzen.com/osm/";

        if (detail == 0)
        {
            Debug.LogError("You have to have at least 1 detail option selected!");
            yield return null;
        }
            
        if ((detail & TileDetail.Earth)     == TileDetail.Earth)     { url += "earth,"; filePath += "E"; }
        if ((detail & TileDetail.Water)     == TileDetail.Water)     { url += "water,"; filePath += "W"; }
        if ((detail & TileDetail.Roads)     == TileDetail.Roads)     { url += "roads,"; filePath += "R"; }
        if ((detail & TileDetail.Landuse)   == TileDetail.Landuse)   { url += "landuse,"; filePath += "L"; }
        if ((detail & TileDetail.Buildings) == TileDetail.Buildings) { url += "buildings,"; filePath += "B"; }

        url = url.Remove(url.Length - 1); //Remove the last comma (cleaner than putting if statements everywhere wether or not we should place one.
        url += "/" + zoom + "/" + mapID.x + "/" + mapID.y + ".json";
        filePath += ".json";

        //Debug.Log(url);
        JSONNode mapData = null;

        //If the data is already cached, use it.
        bool cacheExists = false;

        if (File.Exists(filePath))
        {
            var r = new StreamReader(filePath, Encoding.Default);
            mapData = JSON.Parse(r.ReadToEnd());

            cacheExists = true;
            //Write down somewhere when it was last used, so we can clear the cache on demand. Otherwise player's phones may fill up A LOT!
        }

        //The exact file doesn't exist, but maybe a file with more data exists
        else if (Directory.Exists(dirPath))
        {
            string[] allFiles = Directory.GetFiles(dirPath);

            for (int i = 0; i < allFiles.Length; ++i)
            {
                //It's a file of the same map, but does it have enough data?
                if (allFiles[i].Contains(zoom + "_" + mapID.x + "_" + mapID.y))
                {
                    if ((detail & TileDetail.Earth) == TileDetail.Earth)         { cacheExists = allFiles[i].Contains("E"); }
                    if ((detail & TileDetail.Water) == TileDetail.Water)         { cacheExists = allFiles[i].Contains("W"); }
                    if ((detail & TileDetail.Roads) == TileDetail.Roads)         { cacheExists = allFiles[i].Contains("R"); }
                    if ((detail & TileDetail.Landuse) == TileDetail.Landuse)     { cacheExists = allFiles[i].Contains("L"); }
                    if ((detail & TileDetail.Buildings) == TileDetail.Buildings) { cacheExists = allFiles[i].Contains("B"); }

                    if (cacheExists)
                    {
                        var r = new StreamReader(allFiles[i], Encoding.Default);
                        mapData = JSON.Parse(r.ReadToEnd());
                        break;
                    }
                }
            }
        }

        //If data does not exist, we download it.
        if (!cacheExists)
        {
            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                Debug.LogError(www.error);
                yield return null;
            }
            else
            {
                StreamWriter sr = File.CreateText(filePath);
                Debug.Log("Wrote to: " + Application.persistentDataPath);
                sr.Write(www.text);
                sr.Close();

                mapData = JSON.Parse(www.text);
            }
        }

        //Once we have the data go build some things!
        m_Rect = GM.TileBounds(mapID, zoom);

        //If there are only roads, we pass the entire file.
        if (detail == TileDetail.Water)     { CreateWater(mapData);     yield return null; }
        if (detail == TileDetail.Roads)     { CreateRoads(mapData);     yield return null; }
        if (detail == TileDetail.Buildings) { CreateBuildings(mapData); yield return null; }

        //Else we only need part of it.
        if ((detail & TileDetail.Water) == TileDetail.Water)
            CreateWater(mapData["water"]);

        if ((detail & TileDetail.Roads) == TileDetail.Roads)
            CreateRoads(mapData["roads"]);

        if ((detail & TileDetail.Buildings) == TileDetail.Buildings)
            CreateBuildings(mapData["buildings"]);

        #if UNITY_EDITOR
            InitializeDebug(mapID, zoom);
        #endif
    }

    private void CreateWater(JSONNode waterData)
    {
        if (waterData == null)
            return;

        //Roads are always linestrings
        int i = 0;
        foreach (JSONNode geoData in waterData["features"].AsArray)
        {
            string name = "Water " + i;
            ++i;

            if (geoData["geometry"]["type"].Value == "LineString")
            {
                LineStringType roadType = geoData["properties"]["kind"].Value.ToLineStringType();
                CreateLineString(geoData["geometry"], m_WaterLinePrefab, roadType.ToWidthFloat(), name);
            }

            else if (geoData["geometry"]["type"].Value == "Polygon")
            {
                CreatePolygon(geoData["geometry"], m_WaterPolygonPrefab, name);
            }
        }
    }

    private void CreateRoads(JSONNode roadData)
    {
        if (roadData == null)
            return;

        //Roads are always linestrings
        int i = 0;
        foreach (JSONNode geoData in roadData["features"].AsArray)
        {
            string name = "Road " + i;
            ++i;

            LineStringType roadType = geoData["properties"]["kind"].Value.ToLineStringType();
            CreateLineString(geoData["geometry"], m_RoadPrefab, roadType.ToWidthFloat(), name);
        }
    }

    private void CreateBuildings(JSONNode buildingData)
    {
        if (buildingData == null)
            return;

        //Buildings are always polygons (we ignore the points)
        int i = 0;
        foreach (JSONNode geoData in buildingData["features"].AsArray)
        {
            if (geoData["geometry"]["type"].Value == "Polygon")
            {
                string name = "Building " + i;
                ++i;

                CreatePolygon(geoData["geometry"], m_BuildingPrefab, name);
            }
            
        }
    }

    private void CreateLineString(JSONNode lineStringData, LineString prefab, float width, string name)
    {
        if (lineStringData == null)
            return;

        List<Vector3> vertices = new List<Vector3>();
        foreach (JSONNode coordinate in lineStringData["coordinates"].AsArray)
        {
            Vector2 bm = GM.LatLonToMeters(coordinate[1].AsFloat, coordinate[0].AsFloat);
            Vector2 pm = new Vector2(bm.x - m_Rect.center.x, bm.y - m_Rect.center.y);

            vertices.Add(pm.ToVector3xz());
        }

        //Add a new LineString component & parent it to this object
        LineString lineString = GameObject.Instantiate(prefab) as LineString;
        lineString.name = name;

        lineString.transform.parent = this.transform;
        lineString.transform.position = this.transform.position;

        try
        {
            lineString.Initialize(vertices, width);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void CreatePolygon(JSONNode polygonData, Polygon prefab, string name)
    {
        if (polygonData == null)
            return;

        List<Vector3> vertices = new List<Vector3>();

        //foreach (JSONObject coordinate in polygonData["coordinates"][0].list) //[0] as we only want the base layer of the building.
        // {
        for (int i = 0; i < polygonData["coordinates"][0].AsArray.Count - 1; i++) //For some reason you should NOT take the last vertex as well. This ruins everything and I have no idea why (took me hours).
        {
            JSONNode coordinate = polygonData["coordinates"][0].AsArray[i];

            Vector2 bm = GM.LatLonToMeters(coordinate[1].AsFloat, coordinate[0].AsFloat);
            Vector2 pm = new Vector2(bm.x - m_Rect.center.x, bm.y - m_Rect.center.y);

            vertices.Add(pm.ToVector3xz());
        }

        try
        {
           // Vector3 center = vertices.Aggregate((acc, cur) => acc + cur) / vertices.Count;
            //for (int i = 0; i < vertices.Count; i++)
            //{
            //    vertices[i] = vertices[i] - center;
            //}

            //Add a new Polygon component & parent it to this object
            Polygon polygon = GameObject.Instantiate(prefab) as Polygon;
            polygon.name = name;

            polygon.transform.parent = this.transform;
            polygon.transform.position = this.transform.position;
            //polygon.transform.position = center;
            //polygon.transform.localPosition = center;
            
            polygon.Initialize(vertices);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    #if UNITY_EDITOR
    private void InitializeDebug(Vector2 mapID, int zoom)
    {
        //Used for getting debug GPS locations
        m_MapID = mapID;
        m_Zoom = zoom;

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(m_Rect.width, 1.0f, m_Rect.height);
        boxCollider.isTrigger = true;
    }   
    #endif

}
