using UnityEngine;
using System;


public static class SURGE_Utils
{
    public const double Rad2Deg = 180 / Math.PI;
    public const double Deg2Rad = Math.PI / 180;
    public const double earthRadius = 6371;


    /// <summary>
    /// The distance between two geographical coordinates.
    /// </summary>
    /// <param name="point1">Coordinate (X - Lng, Y - Lat)</param>
    /// <param name="point2">Coordinate (X - Lng, Y - Lat)</param>
    /// <returns>Distance (km).</returns>
    public static Vector2 DistanceBetweenPoints(Vector2 point1, Vector2 point2)
    {
        double scfY = Math.Sin(point1.y * Deg2Rad);
        double sctY = Math.Sin(point2.y * Deg2Rad);
        double ccfY = Math.Cos(point1.y * Deg2Rad);
        double cctY = Math.Cos(point2.y * Deg2Rad);
        double cX = Math.Cos((point1.x - point2.x) * Deg2Rad);
        double sizeX1 = Math.Abs(earthRadius * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
        double sizeX2 = Math.Abs(earthRadius * Math.Acos(sctY * sctY + cctY * cctY * cX));
        float sizeX = (float)((sizeX1 + sizeX2) / 2.0);
        float sizeY = (float)(earthRadius * Math.Acos(scfY * sctY + ccfY * cctY));
        if (float.IsNaN(sizeX)) sizeX = 0;
        if (float.IsNaN(sizeY)) sizeY = 0;
        return new Vector2(sizeX, sizeY);
    }


    /// <summary>
    /// The angle between the two points in degree.
    /// </summary>
    /// <param name="point1">Point 1</param>
    /// <param name="point2">Point 2</param>
    /// <returns>Angle in degree</returns>
    public static float Angle2D(Vector2 point1, Vector2 point2)
    {
        return Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;
    }




}