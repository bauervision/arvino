
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class AddTargetOnClick : MonoBehaviour
{

    private static TargetActor currentTarget;

    public static OnlineMapsMarker selectedMarker = null;

    private double lng, lat;



    private void Start()
    {
        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
    }




    private void OnMapClick()
    {

        Debug.Log("Map Click");
        ClearMarkerData();

        currentTarget = null;

        // Get the coordinates under the cursor.

        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        float alt = OnlineMapsElevationManagerBase.GetUnscaledElevationByCoordinate(lng, lat);

        // Create a label for the marker.
        string label = "Target " + (OnlineMapsMarkerManager.CountItems + 1);

        // make sure we have selected a target to place first
        if (lat != 0 && lng != 0)
        {
            //create the new target data
            TargetActor newTarget = new TargetActor(0, lat, lng);
            newTarget._Alt = alt;
        }

    }

    ///<summary> Fired off when the user clicks a marker on the map. </summary>
    public static void OnTargetClick(OnlineMapsMarkerBase marker)
    {
        UIManager.instance.selectedTargetPanel.SetActive(true);
        UIHover.overUI = false;
        currentTarget = marker["data"] as TargetActor;


    }


    public static DateTime GetTime(string timestamp)
    {
        double ticks = Convert.ToInt64(timestamp);
        TimeSpan time = TimeSpan.FromMilliseconds(ticks);
        return new DateTime(1970, 1, 1) + time;

    }


    public void ClearTargets()
    {
        ClearMarkerData();
        // set that we aren't adding new targets right now


        // clear all the current markers
        OnlineMapsMarkerManager.RemoveAllItems();


        // re-add the user
        OnlineMapsMarkerManager.CreateItem(ARVINO_GPS.Instance._UserCoords, "User");

    }


    private void ClearMarkerData()
    {
        UIManager.instance.selectedTargetPanel.SetActive(false);

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

    public static void Remove_SelectedTarget()
    {
        // we know that this actor needs to be removed, but we need to figure out which marker represents it
        List<OnlineMapsMarker> currentMarkerList = OnlineMapsMarkerManager.instance.items;
        int indexOfMarker = -1;
        foreach (OnlineMapsMarker marker in currentMarkerList)
        {
            // run through all of the markers and match up the data
            TargetActor targetMarker = marker["data"] as TargetActor;
            if (targetMarker != null && (targetMarker._ID == currentTarget._ID))
                indexOfMarker = OnlineMapsMarkerManager.instance.items.IndexOf(marker);
        }

        if (indexOfMarker != -1)
            OnlineMapsMarkerManager.RemoveItem(currentMarkerList[indexOfMarker]);

    }



}
