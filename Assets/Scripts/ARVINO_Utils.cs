using UnityEngine;
using System;


public static class ARVINO_Utils
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


    public static float HandleHeadingToCamera(float lat, float lon)
    {
        Vector3 userCoords = ARVINO_GPS.Instance._UserCoords;

        Vector3 markerCoords = new Vector2(lon, lat);

        // Calculate the tile position of locations.
        int zoom = OnlineMaps.instance.zoom;

        double userTileX, userTileY, markerTileX, markerTileY;
        OnlineMaps.instance.projection.CoordinatesToTile(userCoords.x, userCoords.y, zoom, out userTileX, out userTileY);
        OnlineMaps.instance.projection.CoordinatesToTile(markerCoords.x, markerCoords.y, zoom, out markerTileX, out markerTileY);

        // Calculate the angle between locations.
        float angle = (float)OnlineMapsUtils.Angle2D(userTileX, userTileY, markerTileX, markerTileY) + 90;
        angle = (angle > 360) ? angle - 360 : (angle < 0) ? angle + 360 : angle;

        return Mathf.Round(angle);
    }
    public static float HandleDistanceToCamera(float lat, float lon)
    {
        // get distance between us and the camera
        Vector2 myCoords = new Vector2(lon, lat);
        float distanceToCamera = ARVINO_Utils.DistanceBetweenPoints(myCoords, ARVINO_GPS.Instance._UserCoords).magnitude;
        return (float)System.Math.Round(distanceToCamera, 2);
    }

    public static double KilometerToMeter(double km)
    {
        double METER = 0;

        METER = km * 1000;

        return METER;
    }



}