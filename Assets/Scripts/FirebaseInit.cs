using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;
using System;
using MPUIKIT;
using UnityEngine.Events;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine.UI;


public class FirebaseInit : MonoBehaviour
{
    public static FirebaseInit instance;

    public Color connectedColor;
    public Color disconnectedColor;
    public Color sendingColor;
    public UnityEvent OnFirebaseInit = new UnityEvent();
    [HideInInspector] public static bool initialized = false;

    Firebase.FirebaseApp app;
    private FirebaseDatabase cxSurgeDB;

    public List<TargetActor> rawTargets = new List<TargetActor>();
    public List<TargetActor> inRangeTargetList = new List<TargetActor>();

    private string androidsRef = "cx-surge/androids";
    private string dronesRef = "cx-surge/drones";
    private string targetsRef = "cx-surge/targets";
    // private MPImage gpsImage;
    // private MPImage gpsImage2;
    // private MPImage connectionImage;
    // private MPImage connectionImage2;
    // private MPImage receivedDataImage;
    // private MPImage receivedDataImage2;

    private bool isNewAndroidUser = false;
    private string myAndroidKey = string.Empty;
    private string newTargetKey = string.Empty;

    public static int totalTargets = 0;
    public static int inRangeTargets = 0;

    public List<string> userAddedTargets = new List<string>();


    private void Awake()
    {
        // gpsImage = GameObject.Find("GPSGood").GetComponent<MPImage>();
        // connectionImage = GameObject.Find("Connected").GetComponent<MPImage>();
        // receivedDataImage = GameObject.Find("ReceivedData").GetComponent<MPImage>();
        // gpsImage2 = GameObject.Find("GPSGood2").GetComponent<MPImage>();
        // connectionImage2 = GameObject.Find("Connected2").GetComponent<MPImage>();
        // receivedDataImage2 = GameObject.Find("ReceivedData2").GetComponent<MPImage>();
    }


    ///<summary>Start runs automatically when the app is loaded and handles the connection to firebase and the realtime database. All of the "child added" listeners
    ///will fire off upon initial load as the database is read  </summary>
    private async void Start()
    {
        instance = this;


        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
          {

              if (task.Exception != null)
              {
                  Debug.LogError(System.String.Format(
                     "Could not resolve all Firebase dependencies: {0}", task.Exception));
                  return;
              }
              OnFirebaseInit.Invoke();// hide firebase panel
              initialized = true;
              cxSurgeDB = FirebaseDatabase.DefaultInstance;

              //   receivedDataImage.color = sendingColor;
              //   receivedDataImage2.color = sendingColor;
          });

        StartCoroutine(WaitForGPS());
    }


    ///<summary>We need to make sure that we hold off on fetching the data until we get good GPS running, otherwise our check as to whether the targets
    /// are in range will fail (because there will be no range to check against). Once GPS is working, we can add all of our data listeners  </summary>
    IEnumerator WaitForGPS()
    {
        // set the gps LED to orange to signal we are actively waiting on location services
        // gpsImage.color = sendingColor;
        // gpsImage2.color = sendingColor;
        // we need to make sure that we hold off on pulling in data until we have GPS data
        yield return SURGE_GPS.Instance.WaitForGPS();

        // now that we have GPS coords we can continue with the work, start by signalling that we have gps to the UI
        // gpsImage.color = connectedColor;
        // gpsImage2.color = connectedColor;

        // listen for future data updates
        cxSurgeDB.GetReference(dronesRef).ChildAdded += HandleNewDroneAdded;
        cxSurgeDB.GetReference(dronesRef).ChildRemoved += HandleDroneRemoved;
        cxSurgeDB.GetReference(dronesRef).ChildChanged += HandleDroneChanged;

        cxSurgeDB.GetReference(androidsRef).ChildAdded += HandleNewAndroidAdded;
        cxSurgeDB.GetReference(androidsRef).ChildRemoved += HandleAndroidRemoved;
        cxSurgeDB.GetReference(androidsRef).ChildChanged += HandleAndroidChanged;

        cxSurgeDB.GetReference(targetsRef).ChildAdded += HandleNewTargetAdded;
        cxSurgeDB.GetReference(targetsRef).ChildRemoved += HandleTargetRemoved;
        cxSurgeDB.GetReference(targetsRef).ChildChanged += HandleTargetChanged;
    }


    ///<summary>When the user has closed the app, fire off the removal of this android from the database  </summary>
    public void OnApplicationQuit()
    {
        // gpsImage.color = disconnectedColor;
        // gpsImage2.color = disconnectedColor;
        // connectionImage.color = sendingColor;
        // connectionImage2.color = sendingColor;// show that we are actively removing the android from the database
        // receivedDataImage.color = disconnectedColor;
        // receivedDataImage2.color = disconnectedColor;
        cxSurgeDB.GetReference(androidsRef).Child(myAndroidKey).RemoveValueAsync();
    }

    ///<summary>When the user has closed the app with the quit button, fire off the removal of this android from the database  </summary>
    public void QuitApp()
    {
        // gpsImage.color = disconnectedColor;
        // gpsImage2.color = disconnectedColor;
        // connectionImage.color = sendingColor;// show that we are actively removing the android from the database
        // connectionImage2.color = sendingColor;
        // receivedDataImage.color = disconnectedColor;
        // receivedDataImage2.color = disconnectedColor;

        cxSurgeDB.GetReference(androidsRef).Child(myAndroidKey).RemoveValueAsync();
        Application.Quit();
    }



    ///<summary>Called from TargetManager: set receivedDataImage color to red signalling that we have not processed any data  </summary>
    public void SetReceivedDataFalse()
    {
        // receivedDataImage.color = disconnectedColor;
        // receivedDataImage2.color = disconnectedColor;
    }

    ///<summary>Called from TargetManager: set receivedDataImage color to greed signalling that we have processed data  </summary>
    public void SetReceivedDataTrue()
    {
        // receivedDataImage.color = connectedColor;
        // receivedDataImage2.color = connectedColor;
    }

    ///<summary>Called from TargetManager: set receivedDataImage color to orange signalling that we are retrying for data  </summary>
    public void SetReceivedDataRetry()
    {
        // receivedDataImage.color = sendingColor;
        // receivedDataImage2.color = sendingColor;
    }

    #region Handle All Data Updates

    #region Target Data Updates
    ///<summary>New target data has been found in the database and this handler has been fired off. ProcessNewTargetData has been called if no errors were received.</summary>
    void HandleNewTargetAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        totalTargets++;
        ProcessNewTargetData(args.Snapshot);
    }

    ///<summary>Existing target data has been removed from the database and this handler has been fired off. ProcessRemovedTargetData has been called if no errors were received.</summary>
    void HandleTargetRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        totalTargets--;
        ProcessRemovedTargetData(args.Snapshot);
    }

    ///<summary>Existing target data has been changed in the database and this handler has been fired off.  ProcessUpdatedTargetData has been called if no errors were received.</summary>
    void HandleTargetChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProcessUpdatedTargetData(args.Snapshot);
    }

    #endregion

    #region Drone Data Updates
    ///<summary>Called on initial load and any successive additions to the data </summary>
    void HandleNewDroneAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        ProcessNewDroneData(args.Snapshot);
    }

    ///<summary>  </summary>
    void HandleDroneRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProcessRemovedDroneData(args.Snapshot);
    }

    ///<summary>  </summary>
    void HandleDroneChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProcessUpdatedDroneData(args.Snapshot);
    }

    #endregion

    #region Android Data Updates

    ///<summary>Push new android to database </summary>
    private void PushNewAndroidUser(TargetActor newAndroid)
    {
        // update UI to show that we're sending data
        // connectionImage.color = sendingColor;
        // connectionImage2.color = sendingColor;
        // grab a new key for this android entry and set it as our current users key
        myAndroidKey = cxSurgeDB.GetReference(androidsRef).Push().Key;
        // update our data to reflect this new key so we can compare it later
        newAndroid._ID = myAndroidKey;
        // add a valid timestamp
        newAndroid._Time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
        // set this android as active since it just came online
        newAndroid._isActive = true;
        // convert to json string
        string jsonData = JsonUtility.ToJson(newAndroid);

        // set the data
        cxSurgeDB.GetReference(androidsRef).Child(myAndroidKey).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(task =>
        {
            // if there was an error
            if (task.Exception != null)
            {
                Debug.LogError(System.String.Format("Error SAVING new android data: {0}", task.Exception));
                // set the circle to red color signifying an issue connecting to database
                // connectionImage.color = disconnectedColor;
                // connectionImage2.color = disconnectedColor;
                return;
            }

            // we have stored our android user in firebase so set the connection to valid
            // connectionImage.color = connectedColor;
            // connectionImage2.color = connectedColor;
        });

        // no error so update UI
        isNewAndroidUser = true;

    }

    ///<summary>Updated android in database </summary>
    private void UpdateAndroidUser(TargetActor updatedAndroid)
    {
        // update UI to show that we're sending data
        // connectionImage.color = sendingColor;
        // connectionImage2.color = sendingColor;

        // add a valid timestamp
        updatedAndroid._Time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
        // set this android as active since it just came online
        updatedAndroid._isActive = true;
        // convert to json string
        string jsonData = JsonUtility.ToJson(updatedAndroid);

        // save this data into our android key position within the database
        cxSurgeDB.GetReference(androidsRef).Child(updatedAndroid._ID).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(task =>
        {
            // if there was an error
            if (task.Exception != null)
            {
                Debug.LogError(System.String.Format("Error UPDATING new android data: {0}", task.Exception));
                // set the circle to red color signifying an issue connecting to database
                // connectionImage.color = disconnectedColor;
                // connectionImage2.color = disconnectedColor;
                return;
            }

            // no error so do something if you want
            // we have stored our android user in firebase so set the connection to valid
            // connectionImage.color = connectedColor;
            // connectionImage2.color = connectedColor;

        });
        // or do something here once the set returns
    }

    ///<summary>Called from SURGE_GPS whenever a change has occured in the GPS location  </summary>
    public static void SendMyGPSCoords(TargetActor android)
    {
        // if we havent already pushed this user up as a new active android, do so
        if (!instance.isNewAndroidUser)
            instance.PushNewAndroidUser(android);
        // else
        //     instance.UpdateAndroidUser(android);

    }

    ///<summary>  </summary>
    void HandleNewAndroidAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        ProcessNewAndroidData(args.Snapshot);
    }

    ///<summary>  </summary>
    void HandleAndroidRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProcessRemovedAndroidData(args.Snapshot);
    }

    ///<summary>  </summary>
    void HandleAndroidChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProcessUpdatedAndroidData(args.Snapshot);
    }

    #endregion

    #endregion


    #region Process Incoming Data

    ///<summary>A New Drone has been found in the database, so we need to properly handle this change by examining
    /// the distance to this drone from our location, if it is within 2km, we will add it to our data and render it. </summary>
    public void ProcessNewDroneData(DataSnapshot snapshot)
    {
        // convert and setup the new drone data 
        TargetActor newDrone = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        // add to raw targets so we can process again later
        rawTargets.Add(newDrone);
        // now see if it is in our range to render
        Vector2 droneCoords = new Vector2((float)newDrone._Lon, (float)newDrone._Lat);
        if (TargetManager.IsWithinARRange(droneCoords, SURGE_GPS.Instance._UserCoords, 1.0f, true))
        {
            TargetManager.instance.arTargets.Add(newDrone);
            // send update to target manager to add this drone to the scene
            TargetManager.HandleNewDroneData(newDrone);
        }

    }

    ///<summary>A Drone has been removed from the database, convert the snapshot to drone data, check if this drone is in our scene, and send the data to the target manager for removal. </summary>
    public void ProcessRemovedDroneData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor removedDrone = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        // if we need to process this update, send to target manager
        if (CheckNeedToUpdate(removedDrone, true))
            TargetManager.HandleRemovedDroneData(removedDrone);
    }

    ///<summary>A Target has been removed from the database, so we need to examine
    /// if this target belongs to a drone in our app, if it does, add it to that drone and render it. </summary>
    public void ProcessUpdatedDroneData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor updatedDrone = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        if (CheckNeedToUpdate(updatedDrone, true))
            TargetManager.HandleUpdatedDroneData(updatedDrone);
    }

    ///<summary>A New Target has been found in the database, so we need to examine
    /// if this target is within our range, if it is, render it. </summary>
    public void ProcessNewTargetData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor newTarget = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        // make sure detections start at zero
        newTarget._Detections = 0;

        // add to raw targets so we can process again later
        rawTargets.Add(newTarget);
        CheckTargetGrouping(newTarget);

    }

    private void CheckTargetGrouping(TargetActor newTarget)
    {
        Vector2 newTargetCoords = new Vector2((float)newTarget._Lon, (float)newTarget._Lat);
        // is this target within ar range?
        if (TargetManager.IsWithinARRange(newTargetCoords, SURGE_GPS.Instance._UserCoords, 1.0f, false))
        {
            inRangeTargets++;

            // if we have added inRangeTargets already, then we need to start handling comparisons
            if (inRangeTargetList.Count > 0)
            {
                bool targetIsGroup = false;

                ProcessTargetGrouping(newTarget, newTargetCoords, out targetIsGroup);

                // now if we determined that we need to add this new target, do so now
                if (!targetIsGroup)
                    inRangeTargetList.Add(newTarget);
            }
            else // this is the first inRangeTarget
            {
                inRangeTargetList.Add(newTarget);
                // send update to target manager to add targets to the scene
                TargetManager.HandleNewTargetData(newTarget);
            }

        }
    }

    private void CheckUpdatedTargetGrouping(TargetActor newTarget)
    {
        Vector2 newTargetCoords = new Vector2((float)newTarget._Lon, (float)newTarget._Lat);
        // is this target within ar range?
        if (TargetManager.IsWithinARRange(newTargetCoords, SURGE_GPS.Instance._UserCoords, 1.0f, false))
        {
            // if we have added inRangeTargets already, then we need to start handling comparisons
            if (inRangeTargetList.Count > 0)
            {
                bool targetIsGroup = false;
                ProcessTargetGrouping(newTarget, newTargetCoords, out targetIsGroup);

                // now if we determined that we need to add this new target, do so now
                if (!targetIsGroup)
                    inRangeTargetList.Add(newTarget);
            }
            else // this is the first inRangeTarget
            {
                inRangeTargetList.Add(newTarget);
                // send update to target manager to add targets to the scene
                TargetManager.HandleNewTargetData(newTarget);
            }

        }
    }

    private void ProcessTargetGrouping(TargetActor newTarget, Vector2 newTargetCoords, out bool isGroup)
    {
        isGroup = false;
        // each time we add an in-range target, check to see if we need to group it
        foreach (TargetActor target in inRangeTargetList)
        {
            // setup this target's vector2
            Vector2 currentTargetCoords = new Vector2((float)target._Lon, (float)target._Lat);
            float distance;
            // is this new target in range of this target already in the scene?
            if (TargetManager.IsWithinGroupingRange(currentTargetCoords, newTargetCoords, out distance))
            {
                // it is, so we need to handle grouping, 
                // therefore we increment the detection count of this current target in the inRangeTargetList
                target._Detections++;
                // and add it to inRangeTargets
                isGroup = true;
                TargetManager.HandleUpdatedTargetData(target);
            }
            else// this target is not within group range so add it to the app as a single target
            {
                TargetManager.HandleNewTargetData(newTarget);

            }
        }
    }


    ///<summary>A Target has been removed from the database, convert the snapshot to target data, check if this target is in our scene, and send the data to the target manager for removal. </summary>
    public void ProcessRemovedTargetData(DataSnapshot snapshot)
    {

        // convert and setup the new target data 
        TargetActor removedTarget = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());

        // if we need to process this update, send to target manager
        if (CheckNeedToUpdate(removedTarget, false))
        {
            inRangeTargets--;
            TargetManager.HandleRemovedTargetData(removedTarget);
        }
    }

    ///<summary>A Target has been updated in the database, convert the data, and send to the target manager for updating </summary>
    public void ProcessUpdatedTargetData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor updatedTarget = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        if (CheckNeedToUpdate(updatedTarget, false))
        {
            // make sure detections start at zero
            updatedTarget._Detections = 0;

            //CheckUpdatedTargetGrouping(updatedTarget);
            TargetManager.HandleUpdatedTargetData(updatedTarget);
        }
    }


    ///<summary>A New Android has been found in the database, so we need to properly handle this change by examining
    /// the distance to this android from our location, if it is within 2km, we will add it to our data and render it. </summary>
    public void ProcessNewAndroidData(DataSnapshot snapshot)
    {
        // convert and setup the new drone data 
        TargetActor newAndroid = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        // first make sure this updated android data is not us
        if (newAndroid._ID != myAndroidKey)
        {
            // add to raw targets so we can process again later
            rawTargets.Add(newAndroid);
            // now see if it is in our range to render
            Vector2 androidCoords = new Vector2((float)newAndroid._Lon, (float)newAndroid._Lat);
            if (TargetManager.IsWithinARRange(androidCoords, SURGE_GPS.Instance._UserCoords, 1.0f, true))
            {
                // add this android to the app
                TargetManager.instance.arTargets.Add(newAndroid);
                // send update to target manager to add this drone to the scene
                TargetManager.HandleNewAndroidData(newAndroid);
            }
        }
    }


    ///<summary>An android has been removed from the database, convert the snapshot to android data, check if this android is in our scene, and send the data to the target manager for removal. </summary>
    public void ProcessRemovedAndroidData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor removedAndroid = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        // if we need to process this update, send to Android manager
        if (CheckNeedToUpdate(removedAndroid, true))
            TargetManager.HandleRemovedAndroidData(removedAndroid);
    }

    ///<summary>An android has been updated in the database, convert the data, check if this android is in our scene, and send to the target manager for updating</summary>
    public void ProcessUpdatedAndroidData(DataSnapshot snapshot)
    {
        // convert and setup the new target data 
        TargetActor updatedAndroid = JsonUtility.FromJson<TargetActor>(snapshot.GetRawJsonValue());
        if (CheckNeedToUpdate(updatedAndroid, true))
            TargetManager.HandleUpdatedAndroidData(updatedAndroid);
    }

    #endregion

    ///<summary>Checks if the distance of this actor to the user is less than 1km, and if this actor is present in the data. It is possible that a surge actor
    ///could be present in the scene, but no longer within the 1km range. Eventually, we could do something particular to the UI in this case. </summary>
    private bool CheckNeedToUpdate(SurgeActor actorToCheck, bool isFriendly)
    {
        Vector2 coords = new Vector2((float)actorToCheck._Lon, (float)actorToCheck._Lat);
        // check to see if this actor is even within our range
        bool isInRange = TargetManager.IsWithinARRange(coords, SURGE_GPS.Instance._UserCoords, 1.0f, isFriendly);
        // or currently in our scene
        bool isInScene = TargetManager.IsInScene(actorToCheck);
        return isInRange || isInScene;
    }



    ///<summary>Push new target to database This is called when the user opens the map and places a new visually detected target </summary>
    public void PushNewTarget(TargetActor newTarget)
    {
        // update UI to show that we're sending data
        // connectionImage.color = sendingColor;
        // connectionImage2.color = sendingColor;
        // grab a new key for this android entry and set it as our current users key
        newTargetKey = cxSurgeDB.GetReference(targetsRef).Push().Key;
        // update our data to reflect this new key so we can compare it later
        newTarget._ID = newTargetKey;
        // add a valid timestamp
        newTarget._Time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
        // set this android as active since it just came online
        newTarget._isActive = true;
        // convert to json string
        string jsonData = JsonUtility.ToJson(newTarget);
        // set the data
        cxSurgeDB.GetReference(targetsRef).Child(newTargetKey).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(task =>
        {
            // if there was an error
            if (task.Exception != null)
            {
                Debug.LogError(System.String.Format("Error SAVING new target data: {0}", task.Exception));
                // set the circle to red color signifying an issue connecting to database
                //connectionImage.color = disconnectedColor;
                return;
            }
            // no error so update UI
            // connectionImage.color = connectedColor;
            // connectionImage2.color = connectedColor;

            // add so we can clear later if we want
            userAddedTargets.Add(newTargetKey);

        });
    }

    ///<summary>Push new target to database This is called when the user opens the map and places a new visually detected target </summary>
    public void RemoveAddedTarget(string TargetKey)
    {
        // update UI to show that we're sending data
        // connectionImage.color = sendingColor;
        // connectionImage2.color = sendingColor;

        // set the data
        cxSurgeDB.GetReference(targetsRef).Child(TargetKey).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            // if there was an error
            if (task.Exception != null)
            {
                Debug.LogError(System.String.Format("Error REMOVING target data: {0}", task.Exception));
                // set the circle to red color signifying an issue connecting to database
                //connectionImage.color = disconnectedColor;
                return;
            }
            // no error so update UI
            // connectionImage.color = connectedColor;
            // connectionImage2.color = connectedColor;

            int indexOfRemovedTarget = userAddedTargets.IndexOf(TargetKey);
            if (indexOfRemovedTarget != -1)
                userAddedTargets.RemoveAt(indexOfRemovedTarget);

        });
    }
}
