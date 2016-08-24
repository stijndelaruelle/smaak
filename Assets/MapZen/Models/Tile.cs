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
    }

    [SerializeField]
    private WorldDetail m_WorldDetailPrefab;

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
        bool otherFileUsed = false;

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
                        otherFileUsed = true;
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

        //If there are only roads, we pass the entire file. (unless it came from a file with more data!)
        if (!otherFileUsed)
        {
            if (detail == TileDetail.Water) { CreateWater(mapData); yield return null; }
            if (detail == TileDetail.Roads) { CreateRoads(mapData); yield return null; }
            if (detail == TileDetail.Buildings) { CreateBuildings(mapData); yield return null; }
        }

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


    //Types
    private void CreateWater(JSONNode waterData)
    {
        if (waterData == null)
            return;

        int i = 0;
        foreach (JSONNode geoData in waterData["features"].AsArray)
        {
            //Create a root object
            WorldDetail.WaterType waterType = geoData["properties"]["kind"].Value.ToWaterType();
            int sort_key = geoData["properties"]["sort_key"].AsInt;

            WorldDetail water = GameObject.Instantiate(m_WorldDetailPrefab) as WorldDetail;
            water.name = "Water " + i;

            water.transform.parent = this.transform;
            water.transform.position = this.transform.position;

            water.Initialize(waterType, sort_key);

            if (geoData["geometry"]["type"].Value == "LineString")
            {
                ReadLineString(geoData["geometry"]["coordinates"], water);
            }

            else if (geoData["geometry"]["type"].Value == "Polygon")
            {
                ReadPolygon(geoData["geometry"], water);
            }

            ++i;
        }
    }

    private void CreateRoads(JSONNode roadData)
    {
        if (roadData == null)
            return;

        int i = 0;
        foreach (JSONNode geoData in roadData["features"].AsArray)
        {
            //Create a root object
            WorldDetail.RoadType roadType = geoData["properties"]["kind"].Value.ToRoadType();
            int sort_key = geoData["properties"]["sort_key"].AsInt;

            WorldDetail road = GameObject.Instantiate(m_WorldDetailPrefab) as WorldDetail;
            road.name = "Road " + i;

            road.transform.parent = this.transform;
            road.transform.position = this.transform.position;

            road.Initialize(roadType, sort_key);

            //Gather data & generate models
            if (geoData["geometry"]["type"].Value == "LineString")
            {
                ReadLineString(geoData["geometry"]["coordinates"], road);
            }

            else if (geoData["geometry"]["type"].Value == "MultiLineString")
            {
                ReadMultiLineString(geoData["geometry"]["coordinates"], road);
            }

            ++i;
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
            //Create a root object
            int sort_key = geoData["properties"]["sort_key"].AsInt;

            WorldDetail building = GameObject.Instantiate(m_WorldDetailPrefab) as WorldDetail;
            building.name = "Building " + i;

            building.transform.parent = this.transform;
            building.transform.position = this.transform.position;

            building.Initialize(WorldDetail.BuildingType.Default, sort_key);

            if (geoData["geometry"]["type"].Value == "Polygon")
            {
                ReadPolygon(geoData["geometry"], building);
            }

            ++i;
        }
    }


    //Models
    private void ReadMultiLineString(JSONNode multiLineStringData, WorldDetail parent)
    {
        //Every coordinate is another linestring in this case (confusing naming by the guys of mapzen)
        foreach (JSONNode lineString in multiLineStringData.AsArray)
        {
            ReadLineString(lineString, parent);
        }
    }

    private void ReadLineString(JSONNode lineStringData, WorldDetail worldDetail)
    {
        if (lineStringData == null)
            return;

        List<Vector3> vertices = new List<Vector3>();
        foreach (JSONNode coordinate in lineStringData.AsArray)
        {
            Vector2 bm = GM.LatLonToMeters(coordinate[1].AsFloat, coordinate[0].AsFloat);
            Vector2 pm = new Vector2(bm.x - m_Rect.center.x, bm.y - m_Rect.center.y);

            vertices.Add(pm.ToVector3xz());
        }

        worldDetail.CreateLineString(vertices);
    }

    private void ReadPolygon(JSONNode polygonData, WorldDetail worldDetail)
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

        worldDetail.CreatePolygon(vertices);
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
