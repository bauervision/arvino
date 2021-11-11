
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class AddTargetOnClick : MonoBehaviour
{

    private static TargetActor currentTarget;

    public static OnlineMapsMarker selectedMarker = null;

    public GameObject addTargetButton;

    private double lng, lat;
    private static Text targetCoordsText;
    private static Text targetHeadingText;
    private static Text targetDistanceText;

    public bool canAddTargets = true;


    private void Awake()
    {
        targetHeadingText = GameObject.Find("targetHeadingText").GetComponent<Text>();
        targetDistanceText = GameObject.Find("targetDistanceText").GetComponent<Text>();
        targetCoordsText = GameObject.Find("targetCoordsText").GetComponent<Text>();
    }

    private void Start()
    {
        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
        OnlineMapsControlBase.instance.OnMapDrag += OnMapDrag;
    }

    private void OnMapDrag()
    {
        if (UIManager.instance.selectedTargetPanel.activeInHierarchy)
            UIManager.instance.selectedTargetPanel.SetActive(false);
        canAddTargets = true;
    }

    private void OnMapClick()
    {
        currentTarget = null;

        if (canAddTargets)
        {
            // Get the coordinates under the cursor.
            OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

            // Create a label for the marker.
            string label = "Target " + (OnlineMapsMarkerManager.CountItems + 1);

            // make sure we have selected a target to place first
            if (lat != 0 && lng != 0)
            {
                //create the new target data
                TargetActor newTarget = new TargetActor(0, lat, lng);
                newTarget._Direction = ARVINO_Utils.HandleHeadingToCamera((float)lat, (float)lng);
                newTarget._Distance = ARVINO_Utils.HandleDistanceToCamera((float)lat, (float)lng);
                TargetManager.HandleNewTargetData(newTarget);
            }
        }

    }


    public void CanAddTargets()
    {
        canAddTargets = true;
    }

    public void CannotAddTargets()
    {
        canAddTargets = false;
    }


    ///<summary> Fired off when the user clicks a marker on the map. </summary>
    public static void OnTargetClick(OnlineMapsMarkerBase marker)
    {
        UIManager.instance.selectedTargetPanel.SetActive(true);
        currentTarget = marker["data"] as TargetActor;
        // handle UI
        targetCoordsText.text = $"Lat:{currentTarget._Lat}\nLng:{currentTarget._Lon}";
        targetDistanceText.text = $"Distance: {HandleDistanceToCamera(currentTarget)}";
        targetHeadingText.text = $"Heading: {HandleHeadingToCamera(currentTarget)}";
    }


    public void ClearTargets()
    {
        // clear all the current markers
        OnlineMapsMarkerManager.RemoveAllItems();

        // re-add the user
        OnlineMapsMarkerManager.CreateItem(ARVINO_GPS.Instance._UserCoords, "User");

    }




    public static void Remove_Target(ARVINOActor removedTarget)
    {
        // we know that this actor needs to be removed, but we need to figure out which marker represents it
        List<OnlineMapsMarker> currentMarkerList = OnlineMapsMarkerManager.instance.items;
        int indexOfMarker = -1;
        foreach (OnlineMapsMarker marker in currentMarkerList)
        {
            // run through all of the markers and match up the data
            TargetActor targetMarker = marker["data"] as TargetActor;
            if (targetMarker != null && (targetMarker._ID == removedTarget._ID))
                indexOfMarker = OnlineMapsMarkerManager.instance.items.IndexOf(marker);
        }

        if (indexOfMarker != -1)
            OnlineMapsMarkerManager.RemoveItem(currentMarkerList[indexOfMarker]);

    }

    public void Remove_SelectedTarget()
    {
        UIManager.instance.selectedTargetPanel.SetActive(false);
        canAddTargets = true;
        // we know that this actor needs to be removed, but we need to figure out which marker represents it
        List<OnlineMapsMarker> currentMarkerList = OnlineMapsMarkerManager.instance.items;
        int indexOfMarker = -1;
        TargetActor targetMarker = null;
        foreach (OnlineMapsMarker marker in currentMarkerList)
        {
            // run through all of the markers and match up the data
            targetMarker = marker["data"] as TargetActor;

            if (targetMarker != null && targetMarker._ID == currentTarget._ID)
                indexOfMarker = OnlineMapsMarkerManager.instance.items.IndexOf(marker);
        }

        if (indexOfMarker != -1)
            OnlineMapsMarkerManager.RemoveItem(currentMarkerList[indexOfMarker]);

        if (targetMarker != null)
            TargetManager.HandleRemovedTargetData(targetMarker);


    }


    private static string HandleHeadingToCamera(TargetActor selectedTarget)
    {
        Vector3 userCoords = ARVINO_GPS.Instance._UserCoords;

        Vector3 markerCoords = new Vector2((float)selectedTarget._Lon, (float)selectedTarget._Lat); ;

        // Calculate the tile position of locations.
        int zoom = OnlineMaps.instance.zoom;

        double userTileX, userTileY, markerTileX, markerTileY;
        OnlineMaps.instance.projection.CoordinatesToTile(userCoords.x, userCoords.y, zoom, out userTileX, out userTileY);
        OnlineMaps.instance.projection.CoordinatesToTile(markerCoords.x, markerCoords.y, zoom, out markerTileX, out markerTileY);

        // Calculate the angle between locations.
        float angle = (float)OnlineMapsUtils.Angle2D(userTileX, userTileY, markerTileX, markerTileY) + 90;
        angle = (angle > 360) ? angle - 360 : (angle < 0) ? angle + 360 : angle;

        return Mathf.RoundToInt(angle).ToString();
    }
    private static string HandleDistanceToCamera(TargetActor selectedTarget)
    {
        string distance;
        // get distance between us and the camera
        Vector2 myCoords = new Vector2((float)selectedTarget._Lon, (float)selectedTarget._Lat);
        float distanceToCamera = ARVINO_Utils.DistanceBetweenPoints(myCoords, ARVINO_GPS.Instance._UserCoords).magnitude;
        distanceToCamera = (float)System.Math.Round(distanceToCamera, 2);

        // if 1 kilometer or greater, use "km"
        if (distanceToCamera >= 1.0f)
            distance = distanceToCamera.ToString() + "km";
        else // less than 1 kilometer, so switch to meters
            distance = ((float)System.Math.Round(KilometerToMeter(distanceToCamera), 2)).ToString() + "m";

        return distance;
    }

    public static double KilometerToMeter(double km)
    {
        double METER = 0;

        METER = km * 1000;

        return METER;
    }


    private void Update()
    {
        if (!UIManager.instance.map.activeInHierarchy)
        {
            if (addTargetButton.activeInHierarchy && canAddTargets)
                addTargetButton.SetActive(false);

            if (!addTargetButton.activeInHierarchy && !canAddTargets)
                addTargetButton.SetActive(true);
        }

    }

}
