﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    public Color[] indicatorColors;
    public RectTransform targetRing = null;
    public Transform compass3d = null;

    public Text gpsDataText;
    public Text headingText;

    private bool startTracking = false;

    public Text LoadingText;
    public Text waitText;
    public List<TargetActor> arTargets;// list of all AR targets that will be rendered in the scene

    // separate into different lists so we can easily filter the UI later
    public List<GameObject> loadedFriendlies;
    public List<GameObject> loadedTargets;


    public Texture2D[] targets;


    private Vector2 userCoords = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Input.compass.enabled = true;
        Input.location.Start();
        StartCoroutine(InitializeCompass());
    }

    IEnumerator InitializeCompass()
    {
        yield return new WaitForSeconds(1f);
        startTracking |= Input.compass.enabled;
    }

    private void Update()
    {
        //Keep user coords updated from device
        userCoords.x = (float)ARVINO_GPS.Instance._UserLon;
        userCoords.y = (float)ARVINO_GPS.Instance._UserLat;
        string myLat = userCoords.y.ToString();
        string myLon = userCoords.x.ToString();
        gpsDataText.text = $"LAT:{myLat}    LON:{myLon}";



        if (startTracking)
        {
            transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);
            float smooth = 0.1f;
            float yVelocity = 0.0f;
            float zAngle = Mathf.SmoothDampAngle(targetRing.eulerAngles.z, Input.compass.trueHeading, ref yVelocity, smooth);
            targetRing.rotation = Quaternion.Euler(0, 0, zAngle);
            compass3d.localRotation = Quaternion.Euler(0, zAngle, 0);

            headingText.text = DegreesToCardinalDetailed(Input.compass.trueHeading);
        }

        if (ARVINO_GPS.Instance._UserLon == 0)
            return;


        // once we start getting target data
        if ((loadedTargets.Count > 0) || (loadedFriendlies.Count > 0))
        {
            HandleFiltering();
        }

    }

    #region UI Updates

    private void HandleFiltering()
    {
        //if (instance.loadedFriendlies.Count > 0)
        foreach (GameObject friend in instance.loadedFriendlies)
            FilterTrackers(friend, true);

        foreach (GameObject target in instance.loadedTargets)
            FilterTrackers(target, false);
    }


    private void FilterTrackers(GameObject tracker, bool isFriendly)
    {
        // setup the Vector2 for this target
        Vector2 targetCoords = new Vector2((float)tracker.GetComponent<ARVINO_Target>()._Lon, (float)tracker.GetComponent<ARVINO_Target>()._Lat);


        if (GetDistanceFromTarget(targetCoords) > UIManager.instance.distanceFilter)
        {
            tracker.GetComponent<ARVINO_Target>().HideMyTracker();
            return;
        }
        else
            tracker.GetComponent<ARVINO_Target>().ShowMyTracker();
    }

    public static void ToggleTargetTrackers()
    {
        foreach (GameObject tracker in instance.loadedTargets)
        {
            tracker.GetComponent<ARVINO_Target>().ToggleMyTracker();
        }

        foreach (GameObject tracker in instance.loadedFriendlies)
        {
            tracker.GetComponent<ARVINO_Target>().ToggleMyTracker();
        }


    }

    #endregion









    /// <summary>Called from FirebaseInit whenever new target data arrives. Immediately add it to the scene for processing </summary>
    public static void HandleNewTargetData(TargetActor newTarget)
    {
        instance.SetNewTarget(newTarget);
    }

    /// <summary>Called from FirebaseInit whenever a target is removed from the data. Pull it out of the scene </summary>
    public static void HandleRemovedTargetData(TargetActor targetToRemove)
    {
        instance.RemoveTarget(targetToRemove);
    }

    /// <summary>Called from FirebaseInit whenever updated target data arrives. Immediately process the data </summary>
    public static void HandleUpdatedTargetData(TargetActor targetUpdated)
    {
        instance.UpdateTarget(targetUpdated);
    }






    #region Set New Actors

    ///<summary>Instantiates the correct prefab in the scene, updates its position, and then adds the gameobject to the correct list</summary>
    private void AddActorToScene(ARVINOActor actor, List<GameObject> actorList)
    {
        // create the actual scene object at the center of the 3d world
        GameObject newActorSpawn = Instantiate(Resources.Load<GameObject>("Target"), Vector3.zero, Quaternion.identity);
        // update the position of the 3d actor
        newActorSpawn.GetComponent<ARVINO_Target>().SetInitialPosition(actor);
        // add this target to the scene data
        actorList.Add(newActorSpawn);
        // add a marker to the map
        Texture2D markerTexture = targets[0];
        OnlineMapsMarker newTargetMarker = OnlineMapsMarkerManager.CreateUserItem(actor._Lon, actor._Lat, markerTexture, "Target");
        newTargetMarker["data"] = actor;

        newTargetMarker.OnClick += AddTargetOnClick.OnTargetClick;

    }

    /// <summary>Add a new GPS based target to the 3d scene and its data to the correct list of gameobjects</summary>
    private void SetNewTarget(TargetActor newTarget)
    {
        AddActorToScene(newTarget, loadedTargets);
    }



    #endregion

    #region Remove Actors

    ///<summary>Locates this actor in the scene, destroys it and then removes its data from the app</summary>
    private void RemoveActorFromScene(ARVINOActor actorToRemove, List<GameObject> actorList)
    {
        // find this particular actor in the scene data
        int indexOfRemovedActor = actorList.FindIndex((a) => a.GetComponent<ARVINO_Target>()._ID == actorToRemove._ID);

        if (indexOfRemovedActor != -1)
        {
            // remove the indicator
            actorList[indexOfRemovedActor].GetComponent<ARVINO_Target>().RemoveTarget();
            // remove it from the data
            actorList.RemoveAt(indexOfRemovedActor);
            // remove the marker
            AddTargetOnClick.Remove_Target(actorToRemove);
        }
    }

    /// <summary>Remove this target actor from the scene</summary>
    private void RemoveTarget(TargetActor removedTarget)
    {
        RemoveActorFromScene(removedTarget, loadedTargets);
    }

    /// <summary>Remove this drone actor from the scene</summary>
    private void RemoveDrone(ARVINOActor removedDrone)
    {
        RemoveActorFromScene(removedDrone, loadedFriendlies);
    }

    /// <summary>Remove this android actor from the scene</summary>
    private void RemoveAndroid(ARVINOActor removedAndroid)
    {
        RemoveActorFromScene(removedAndroid, loadedFriendlies);
    }

    #endregion


    #region Update Actors

    ///<summary>Identifies the index of this actor in the loaded scene data, and calls its UpdatePosition method  </summary>
    private void UpdateActorInScene(ARVINOActor actorToUpdate, List<GameObject> actorList)
    {
        // find this particular actor in the scene data
        int indexOfUpdatedActor = actorList.FindIndex((a) => a.GetComponent<ARVINO_Target>()._ID == actorToUpdate._ID);
        // update its data IF we found it in the scene
        if (indexOfUpdatedActor != -1)
            actorList[indexOfUpdatedActor].GetComponent<ARVINO_Target>().UpdatePosition(actorToUpdate);
    }

    /// <summary>Update this target actor in the scene</summary>
    private void UpdateTarget(TargetActor updatedTarget)
    {
        UpdateActorInScene(updatedTarget, loadedTargets);
    }




    #endregion


    #region Utilities
    private static string DegreesToCardinalDetailed(double degrees)
    {
        string[] caridnals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };
        return caridnals[(int)System.Math.Round(((double)degrees * 10 % 3600) / 225)];
    }


    public static bool IsWithinARRange(Vector2 myCoords, Vector2 userCoords, float arRange, bool isFriendly)
    {
        float distanceToCamera = 0.0f;
        // get distance between us and the camera
        distanceToCamera = ARVINO_Utils.DistanceBetweenPoints(myCoords, userCoords).magnitude;
        float roundedMeterDist = (float)System.Math.Round(distanceToCamera, 2);
        // if this target is a drone or android and user wants to see

        return (roundedMeterDist <= arRange);

    }

    public static bool IsWithinGroupingRange(Vector2 tgt1Coords, Vector2 tgt2Coords, out float distanceTo)
    {
        // get distance between the two targets
        distanceTo = (float)System.Math.Round(ARVINO_Utils.DistanceBetweenPoints(tgt1Coords, tgt2Coords).magnitude, 2);
        return distanceTo <= 0.02; // within 10 meters?
    }

    ///<summary>Check for this actor in the loaded scene data</summary>    
    public static bool IsInScene(ARVINOActor actorToCheck)
    {
        return instance.arTargets.Exists((d) => d._ID == actorToCheck._ID);
    }






    public float GetDistanceFromTarget(Vector2 thisTargetCoords)
    {
        Vector2 targetCoords = new Vector2((float)thisTargetCoords.x, (float)thisTargetCoords.y);
        return (float)System.Math.Round(ARVINO_Utils.DistanceBetweenPoints(ARVINO_GPS.Instance._UserCoords, targetCoords).magnitude, 2);

    }
    #endregion

}
