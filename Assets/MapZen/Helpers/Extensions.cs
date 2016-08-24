using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions
{
    public static Vector2 ToVector2xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 ToVector3xz(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static WorldDetail.WaterType ToWaterType(this string s)
    {
        //Could swap this for an Enum.Parse(), may decide that at a later date
        switch (s)
        {
            case "basin":           return WorldDetail.WaterType.Basin;
            case "dock":            return WorldDetail.WaterType.Dock;
            case "lake":            return WorldDetail.WaterType.Lake;
            case "ocean":           return WorldDetail.WaterType.Ocean;
            case "playa":           return WorldDetail.WaterType.Playa;
            case "riverbank":       return WorldDetail.WaterType.Riverbank;
            case "swimming_poll":   return WorldDetail.WaterType.SwimmingPool;
            case "water":           return WorldDetail.WaterType.Water;
            case "canal":           return WorldDetail.WaterType.Canal;
            case "dam":             return WorldDetail.WaterType.Dam;
            case "ditch":           return WorldDetail.WaterType.Ditch;
            case "drain":           return WorldDetail.WaterType.Drain;
            case "river":           return WorldDetail.WaterType.River;
            case "stream":          return WorldDetail.WaterType.Stream;

            default: return WorldDetail.WaterType.Undefined;
        }
    }

    public static WorldDetail.RoadType ToRoadType(this string s)
    {
        switch (s)
        {
            case "highway":     return WorldDetail.RoadType.Highway;
            case "major_road":  return WorldDetail.RoadType.MajorRoad;
            case "minor_road":  return WorldDetail.RoadType.MinorRoad;
            case "rail":        return WorldDetail.RoadType.Rail;
            case "path":        return WorldDetail.RoadType.Path;
            case "bicycle":     return WorldDetail.RoadType.BicycleRoad;

            default:            return WorldDetail.RoadType.Undefined;
        }
    }

    //http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
    public static Vector2 WorldToTilePos(double lat, double lon, int zoom)
    {
        Vector2 p = new Vector2();
        p.x = (float)((lon + 180.0) / 360.0 * (1 << zoom));
        p.y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
            1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

        p.x = Mathf.Floor(p.x);
        p.y = Mathf.Floor(p.y);
        return p;
    }

    public static Vector2 TileToWorldPos(double tile_x, double tile_y, int zoom)
    {
        Vector2 p = new Vector2();
        double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

        p.x = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
        p.y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

        return p;
    }
}
