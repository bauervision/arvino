
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class AddTargetOnClick : MonoBehaviour
{
    public static Text targetTypeText;


    private static TargetActor currentTarget;

    public static int targetIndex = -1;
    public static string[] targetLabels = new string[] { "Drone", "Personel", "Vehicle", "Objective", "Android" };

    public static OnlineMapsMarker selectedMarker = null;

    private double lng, lat;

    private void Awake()
    {
        targetTypeText = GameObject.Find("TargetTypeText").GetComponent<Text>();

    }

    private void Start()
    {
        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
    }


    public void SetPersonelTarget()
    {
        lat = 0;
        lng = 0;
        targetIndex = 1;

    }

    public void SetVehicleTarget()
    {
        lat = 0;
        lng = 0;
        targetIndex = 2;
    }
    public void SetObjectiveTarget()
    {
        lat = 0;
        lng = 0;
        targetIndex = 3;
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
        if (targetIndex != -1 && (lat != 0 && lng != 0))
        {
            //create the new target data
            TargetType newType = (TargetType)(targetIndex); // add 1 to the selected type to match what is expected
            TargetActor newTarget = new TargetActor(newType, lat, lng);
            newTarget._Alt = alt;

            // push to database
            FirebaseInit.instance.PushNewTarget(newTarget);

        }

    }

    ///<summary> Fired off when the user clicks a marker on the map. </summary>
    public static void OnTargetClick(OnlineMapsMarkerBase marker)
    {
        UIManager.instance.selectedTargetPanel.SetActive(true);
        UIHover.overUI = false;
        currentTarget = marker["data"] as TargetActor;
        targetTypeText.text = ((TargetType)currentTarget._Type).ToString();

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
        targetIndex = -1;

        // clear all the current markers
        OnlineMapsMarkerManager.RemoveAllItems();


        foreach (string key in FirebaseInit.instance.userAddedTargets)
        {
            FirebaseInit.instance.RemoveAddedTarget(key);
        }

        // re-add the user
        OnlineMapsMarkerManager.CreateItem(SURGE_GPS.Instance._UserCoords, "User");

    }


    private void ClearMarkerData()
    {
        UIManager.instance.selectedTargetPanel.SetActive(false);
        targetTypeText.text = "";

    }

    public static void Remove_Target(SurgeActor removedTarget)
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

        FirebaseInit.instance.RemoveAddedTarget(currentTarget._ID);

    }

    public static void ListenTo_SelectedTarget()
    {
        // we want to set this target as our active listening target
        List<OnlineMapsMarker> currentMarkerList = OnlineMapsMarkerManager.instance.items;
        int indexOfMarker = -1;

        foreach (OnlineMapsMarker marker in currentMarkerList)
        {
            // run through all of the markers and match up the data
            TargetActor targetMarker = marker["data"] as TargetActor;
            if (targetMarker != null && (targetMarker._ID == currentTarget._ID))
            {
                indexOfMarker = OnlineMapsMarkerManager.instance.items.IndexOf(marker);
                UIManager.instance.SetActiveListeningTarget(targetMarker._ID);
            }
        }





    }

}
