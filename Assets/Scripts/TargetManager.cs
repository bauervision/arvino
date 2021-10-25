using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    public Color[] indicatorColors;
    public RectTransform targetRing = null;


    public TextMeshProUGUI HeadingText;

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
        if (ARVINO_GPS.Instance._UserLon == 0)
            return;

        //Keep user coords updated from device
        userCoords.x = (float)ARVINO_GPS.Instance._UserLon;
        userCoords.y = (float)ARVINO_GPS.Instance._UserLat;
        string myLat = userCoords.y.ToString();
        string myLon = userCoords.x.ToString();

        if (startTracking)
        {
            transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);
            float smooth = 0.1f;
            float yVelocity = 0.1f;
            float zAngle = Mathf.SmoothDampAngle(targetRing.eulerAngles.z, Input.compass.trueHeading, ref yVelocity, smooth);
            targetRing.rotation = Quaternion.Euler(0, 0, zAngle);
        }

        if (Application.isEditor)
        {
            float cAngle = ARVINO_GPS.compassAngle;
            cAngle = cAngle < 0 ? cAngle + 360 : cAngle;
            HeadingText.text = DegreesToCardinalDetailed(cAngle);
        }
        else
        {
            HeadingText.text = DegreesToCardinalDetailed(Mathf.RoundToInt(Input.compass.trueHeading));
        }
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
