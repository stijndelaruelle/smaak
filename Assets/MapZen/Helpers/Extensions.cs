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

    public static Tile.LineStringType ToLineStringType(this string s)
    {
        switch (s)
        {
            //Water
            case "canal":       return Tile.LineStringType.Canal;
            case "dam":         return Tile.LineStringType.Dam;
            case "ditch":       return Tile.LineStringType.Ditch;
            case "drain":       return Tile.LineStringType.Drain;
            case "river":       return Tile.LineStringType.River;
            case "stream":      return Tile.LineStringType.Stream;

            //Roads
            case "highway":     return Tile.LineStringType.Highway;
            case "major_road":  return Tile.LineStringType.MajorRoad;
            case "minor":       return Tile.LineStringType.MinorRoad;
            case "rail":        return Tile.LineStringType.Rail;
            case "path":        return Tile.LineStringType.Path;

            default:            return Tile.LineStringType.Undefined;
        }
    }

    public static float ToWidthFloat(this Tile.LineStringType s)
    {
        switch (s)
        {
            //Water
            case Tile.LineStringType.Canal:     return 1;
            case Tile.LineStringType.Dam:       return 5;
            case Tile.LineStringType.Ditch:     return 5;
            case Tile.LineStringType.Drain:     return 5;
            case Tile.LineStringType.River:     return 10;
            case Tile.LineStringType.Stream:    return 10;

            //Roads
            case Tile.LineStringType.Highway:   return 10;
            case Tile.LineStringType.MajorRoad: return 5;
            case Tile.LineStringType.MinorRoad: return 3;
            case Tile.LineStringType.Rail:      return 3;
            case Tile.LineStringType.Path:      return 2;
        }

        return 2;
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
