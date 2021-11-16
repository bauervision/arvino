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










    /// <summary>Called from AddTargetOnClick whenever new target data is added. Immediately add it to the scene for processing </summary>
    public static void HandleNewTargetData(TargetActor newTarget)
    {
        instance.AddActorToScene(newTarget, instance.loadedTargets);
    }

    /// <summary>Called from AddTargetOnClick whenever a target is removed. Pull it out of the scene </summary>
    public static void HandleRemovedTargetData(TargetActor targetToRemove)
    {
        instance.RemoveTarget(targetToRemove);
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
        OnlineMapsMarker newTargetMarker = OnlineMapsMarkerManager.CreateUserItem(actor._Lon, actor._Lat, markerTexture, actor._ID);
        newTargetMarker["data"] = (TargetActor)actor;

        newTargetMarker.OnClick += AddTargetOnClick.OnTargetClick;

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





    #region Utilities
    private static string DegreesToCardinalDetailed(double degrees)
    {
        string[] caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
        return caridnals[(int)System.Math.Round(((double)degrees % 360) / 45)];
    }
    #endregion

}
