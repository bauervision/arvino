using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SURGE;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    public Color[] indicatorColors;
    public RectTransform targetRing = null;
    public Transform compass3d = null;
    public Transform NorthReversed3d = null;
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

    public Text targetCount;


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
        userCoords.x = (float)SURGE_GPS.Instance._UserLon;
        userCoords.y = (float)SURGE_GPS.Instance._UserLat;
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

            NorthReversed3d.localRotation = Quaternion.Euler(0, -zAngle, 0);
            headingText.text = DegreesToCardinalDetailed(Input.compass.trueHeading);
        }

        if (SURGE_GPS.Instance._UserLon == 0)
            return;


        // once we start getting target data
        if ((loadedTargets.Count > 0) || (loadedFriendlies.Count > 0))
        {
            targetCount.text = "";
            FirebaseInit.instance.SetReceivedDataTrue();// signal that we have processed targets successfully
            HandleFiltering();
        }
        else // no targets have been loaded
        {
            // we have received target data so we need to process it
            targetCount.text = "No AR Targets, listening for updates...";
            FirebaseInit.instance.SetReceivedDataRetry();// signal that we are still trying to load targets
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
        Vector2 targetCoords = new Vector2((float)tracker.GetComponent<SURGE_Target>()._Lon, (float)tracker.GetComponent<SURGE_Target>()._Lat);

        // if this is a friendly but we dont want to see them, hide
        if (!UIManager.instance.displayFriendlies && isFriendly)
        {
            tracker.GetComponent<SURGE_Target>().HideMyTracker();
            return;
        }

        if (GetDistanceFromTarget(targetCoords) > UIManager.instance.distanceFilter)
        {
            tracker.GetComponent<SURGE_Target>().HideMyTracker();
            return;
        }
        else
            tracker.GetComponent<SURGE_Target>().ShowMyTracker();
    }

    public static void ToggleTargetTrackers()
    {
        foreach (GameObject tracker in instance.loadedTargets)
        {
            tracker.GetComponent<SURGE_Target>().ToggleMyTracker();
        }

        foreach (GameObject tracker in instance.loadedFriendlies)
        {
            tracker.GetComponent<SURGE_Target>().ToggleMyTracker();
        }


    }

    #endregion


    #region Handle Data Updates From Firebase

    /// <summary>Called from FirebaseInit whenever new drone data arrives. Immediately add it to the scene for processing </summary>
    public static void HandleNewDroneData(SurgeActor newDrone)
    {
        instance.SetNewDrone(newDrone);
    }

    /// <summary>Called from FirebaseInit whenever a drone is removed from the data. Pull it out of the scene </summary>
    public static void HandleRemovedDroneData(SurgeActor droneToRemove)
    {
        instance.RemoveDrone(droneToRemove);
    }

    /// <summary>Called from FirebaseInit whenever updated drone data arrives. Immediately process the data </summary>
    public static void HandleUpdatedDroneData(SurgeActor droneUpdated)
    {
        instance.UpdateDrone(droneUpdated);
    }



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





    /// <summary>Called from FirebaseInit whenever new android data arrives. Immediately add it to the scene for processing </summary>
    public static void HandleNewAndroidData(SurgeActor newAndroid)
    {
        instance.SetNewAndroid(newAndroid);
    }

    /// <summary>Called from FirebaseInit whenever an android is removed from the data. Pull it out of the scene </summary>
    public static void HandleRemovedAndroidData(SurgeActor androidToRemove)
    {
        instance.RemoveAndroid(androidToRemove);
    }

    /// <summary>Called from FirebaseInit whenever updated android data arrives. Immediately process the data</summary>
    public static void HandleUpdatedAndroidData(SurgeActor androidUpdated)
    {
        instance.UpdateAndroid(androidUpdated);
    }

    #endregion


    #region Set New Actors

    ///<summary>Instantiates the correct prefab in the scene, updates its position, and then adds the gameobject to the correct list</summary>
    private void AddActorToScene(SurgeActor actor, SURGE_Target_Type type, List<GameObject> actorList)
    {
        // create the actual scene object at the center of the 3d world
        GameObject newActorSpawn = Instantiate(Resources.Load<GameObject>(GetTargetPrefab(type)), Vector3.zero, Quaternion.identity);
        // update the position of the 3d actor
        newActorSpawn.GetComponent<SURGE_Target>().SetInitialPosition(actor, type);
        // add this target to the scene data
        actorList.Add(newActorSpawn);
        // add a marker to the map
        Texture2D markerTexture = targets[(int)type];
        OnlineMapsMarker newTargetMarker = OnlineMapsMarkerManager.CreateUserItem(actor._Lon, actor._Lat, markerTexture, AddTargetOnClick.targetLabels[(int)type]);
        newTargetMarker["data"] = actor;

        newTargetMarker.OnClick += AddTargetOnClick.OnTargetClick;

    }

    /// <summary>Add a new GPS based target to the 3d scene and its data to the correct list of gameobjects</summary>
    private void SetNewTarget(TargetActor newTarget)
    {
        AddActorToScene(newTarget, (SURGE_Target_Type)newTarget._Type, loadedTargets);
    }

    /// <summary>Add a new GPS based drone to the 3d scene and its data to the correct list of gameobjects</summary>
    private void SetNewDrone(SurgeActor newDrone)
    {
        AddActorToScene(newDrone, (SURGE_Target_Type)0, loadedTargets);
    }

    /// <summary>Spawn a new GPS based android to the 3d scene and its data to the correct list of gameobjects</summary>
    private void SetNewAndroid(SurgeActor newAndroid)
    {
        AddActorToScene(newAndroid, (SURGE_Target_Type)4, loadedTargets);
    }

    #endregion

    #region Remove Actors

    ///<summary>Locates this actor in the scene, destroys it and then removes its data from the app</summary>
    private void RemoveActorFromScene(SurgeActor actorToRemove, List<GameObject> actorList)
    {
        // find this particular actor in the scene data
        int indexOfRemovedActor = actorList.FindIndex((a) => a.GetComponent<SURGE_Target>()._ID == actorToRemove._ID);

        if (indexOfRemovedActor != -1)
        {
            // remove the indicator
            actorList[indexOfRemovedActor].GetComponent<SURGE_Target>().RemoveTarget();
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
    private void RemoveDrone(SurgeActor removedDrone)
    {
        RemoveActorFromScene(removedDrone, loadedFriendlies);
    }

    /// <summary>Remove this android actor from the scene</summary>
    private void RemoveAndroid(SurgeActor removedAndroid)
    {
        RemoveActorFromScene(removedAndroid, loadedFriendlies);
    }

    #endregion


    #region Update Actors

    ///<summary>Identifies the index of this actor in the loaded scene data, and calls its UpdatePosition method  </summary>
    private void UpdateActorInScene(SurgeActor actorToUpdate, List<GameObject> actorList, SURGE_Target_Type type)
    {
        // find this particular actor in the scene data
        int indexOfUpdatedActor = actorList.FindIndex((a) => a.GetComponent<SURGE_Target>()._ID == actorToUpdate._ID);
        // update its data IF we found it in the scene
        if (indexOfUpdatedActor != -1)
            actorList[indexOfUpdatedActor].GetComponent<SURGE_Target>().UpdatePosition(actorToUpdate, type);
    }

    /// <summary>Update this target actor in the scene</summary>
    private void UpdateTarget(TargetActor updatedTarget)
    {
        UpdateActorInScene(updatedTarget, loadedTargets, (SURGE_Target_Type)updatedTarget._Type);
    }

    /// <summary>Update this drone actor in the scene</summary>
    private void UpdateDrone(SurgeActor updatedDrone)
    {
        UpdateActorInScene(updatedDrone, loadedFriendlies, (SURGE_Target_Type)0);// 0 = drone target type
    }

    /// <summary>Update this android actor in the scene</summary>
    private void UpdateAndroid(SurgeActor updatedAndroid)
    {
        UpdateActorInScene(updatedAndroid, loadedFriendlies, (SURGE_Target_Type)4); // 4 = android target type;
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
        distanceToCamera = SURGE_Utils.DistanceBetweenPoints(myCoords, userCoords).magnitude;
        float roundedMeterDist = (float)System.Math.Round(distanceToCamera, 2);
        // if this target is a drone or android and user wants to see

        return (roundedMeterDist <= arRange);

    }

    public static bool IsWithinGroupingRange(Vector2 tgt1Coords, Vector2 tgt2Coords, out float distanceTo)
    {
        // get distance between the two targets
        distanceTo = (float)System.Math.Round(SURGE_Utils.DistanceBetweenPoints(tgt1Coords, tgt2Coords).magnitude, 2);
        return distanceTo <= 0.02; // within 10 meters?
    }

    ///<summary>Check for this actor in the loaded scene data</summary>    
    public static bool IsInScene(SurgeActor actorToCheck)
    {
        return instance.arTargets.Exists((d) => d._ID == actorToCheck._ID);
    }


    private static string GetTargetPrefab(SURGE_Target_Type newTargetType)
    {
        switch (newTargetType)
        {
            case SURGE_Target_Type.DRONE: return "Drone";
            case SURGE_Target_Type.PERSON: return "Person";
            case SURGE_Target_Type.VEHICLE: return "Vehicle";
            case SURGE_Target_Type.OBJECTIVE: return "Objective";
        }
        return "Android";
    }

    private Color GetTargetIndicatorColor(SURGE_Target_Type newTargetType)
    {
        switch (newTargetType)
        {
            case SURGE_Target_Type.DRONE: return indicatorColors[0];
            case SURGE_Target_Type.PERSON: return indicatorColors[1];
            case SURGE_Target_Type.VEHICLE: return indicatorColors[2];
            case SURGE_Target_Type.OBJECTIVE: return indicatorColors[3];
        }
        return indicatorColors[4];
    }

    public float GetDistanceFromTarget(Vector2 thisTargetCoords)
    {
        Vector2 targetCoords = new Vector2((float)thisTargetCoords.x, (float)thisTargetCoords.y);
        return (float)System.Math.Round(SURGE_Utils.DistanceBetweenPoints(SURGE_GPS.Instance._UserCoords, targetCoords).magnitude, 2);

    }
    #endregion

}
