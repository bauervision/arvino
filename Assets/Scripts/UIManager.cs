using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject compassRing;
    public GameObject baseUI;
    public GameObject optionsPanel;
    public GameObject gpsDebugPanel;
    public GameObject statusPanel;
    public GameObject distanceSlider;
    public GameObject map;
    public GameObject mapTargetButtons;
    public GameObject CustomCoordsPanel;

    public Camera arCamera;

    public GameObject ShowTargets;
    public GameObject HideTargets;

    public GameObject Targets;

    public GameObject selectedTargetPanel;



    public Text distanceFilterText;
    public Text listenToText;
    public bool showCompassRing = true;
    public bool showTargetRing = true;
    public bool displayGeometry = false;
    public bool displayDistanceFilter = false;
    public bool displayMap = true;

    public bool displayFriendlies = false;
    public bool displayBox = true;
    public bool displayInActive = false;
    public bool alwaysActive = true;
    public bool displayCustomCoords = false;

    public float distanceFilter = 0.5f;

    public UnityEvent onMinimalUIActive = new UnityEvent();
    public UnityEvent onMinimalUIDeActive = new UnityEvent();

    public int audioInterval = 10;

    private Quaternion arCameraInitialRotation;

    private void Start()
    {
        instance = this;
        optionsPanel.SetActive(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        distanceSlider.SetActive(false);
        map.SetActive(false);
        mapTargetButtons.SetActive(false);
        arCameraInitialRotation = arCamera.transform.localRotation;
        Targets.SetActive(false);
        HideTargets.SetActive(false);
        statusPanel.SetActive(false);
        selectedTargetPanel.SetActive(false);
        CustomCoordsPanel.SetActive(displayCustomCoords);
        //gameVoiceControl.onStartListening();

        // start SONUS with map screen first
        map.SetActive(true);
        mapTargetButtons.SetActive(true);
        ShowTargets.SetActive(true);
    }



    public void ToggleMinimalUI()
    {

        if (baseUI.activeInHierarchy)
        {
            onMinimalUIActive.Invoke();
            baseUI.SetActive(false);
        }
        else
        {
            onMinimalUIDeActive.Invoke();
            baseUI.SetActive(true);
        }
    }

    public void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);
        if (optionsPanel.activeInHierarchy)
            distanceSlider.SetActive(false);
    }

    public void ToggleStatus()
    {
        statusPanel.SetActive(!statusPanel.activeInHierarchy);
    }

    public void ToggleCompassRing()
    {
        showCompassRing = !showCompassRing;
    }

    public void ToggleTargetRing()
    {
        showTargetRing = !showTargetRing;
    }

    public void ToggleGeometryDisplay()
    {
        displayGeometry = !displayGeometry;
        TargetManager.ToggleTargetTrackers();

    }

    public void ToggleFriendlies()
    {
        displayFriendlies = !displayFriendlies;

    }

    public void ToggleBoundingBox()
    {
        displayBox = !displayBox;
    }

    public void ToggleInActive()
    {
        displayInActive = !displayInActive;
    }

    public void ToggleActiveMode()
    {
        alwaysActive = !alwaysActive;
    }

    public void GPSEnabled()
    {
        gpsDebugPanel.SetActive(false);
    }

    public void ToggleDistanceFilter()
    {
        displayDistanceFilter = !displayDistanceFilter;
        distanceSlider.SetActive(displayDistanceFilter);
        // if we are seeing the distance slider, hide the compass
        showCompassRing = !displayDistanceFilter;
        // if we are seeing the slider
        if (displayDistanceFilter)
        {
            // hide the other panels
            statusPanel.SetActive(false);
            optionsPanel.SetActive(false);
        }
    }

    public void SetDistanceFilter(float newDistance)
    {
        distanceFilter = (float)System.Math.Round((double)newDistance, 2);
    }

    public void ToggleMap()
    {
        displayMap = !displayMap;
        map.SetActive(displayMap);
        mapTargetButtons.SetActive(displayMap);
        ShowTargets.SetActive(displayMap);
        AddTargetOnClick.targetIndex = -1;// set that we can't add targets to the map

    }


    public void SetActiveListeningTarget(string targetID)
    {
        foreach (GameObject target in TargetManager.instance.loadedTargets)
        {
            if (target.GetComponent<SURGE_Target>()._ID == targetID)
            {
                // if we already listening to this target
                if (target.GetComponent<SURGE_Target>()._isListening)
                {
                    listenToText.text = "Listen To";
                    target.GetComponent<SURGE_Target>().StopListeningTarget();
                }
                else
                {
                    listenToText.text = "Stop Listen";
                    target.GetComponent<SURGE_Target>().SetActiveListeningTarget();
                }
            }


        }
    }

    public void SetAudioInterval(int value)
    {
        switch (value)
        {
            case 0: audioInterval = 10; break;
            case 1: audioInterval = 30; break;
            default: audioInterval = 60; break;
        }
    }




    public void ShowAddTargetPanel()
    {
        ShowTargets.SetActive(false);
        HideTargets.SetActive(true);
        Targets.SetActive(true);
        // since we have decided to open the target panel and want to add targets, best we zoom to a reasonable level
        OnlineMaps.instance.SetPositionAndZoom(SURGE_GPS.Instance._UserLon, SURGE_GPS.Instance._UserLat, 18);

    }

    public void HideAddTargetPanel()
    {
        ShowTargets.SetActive(true);
        HideTargets.SetActive(false);
        Targets.SetActive(false);
        AddTargetOnClick.targetIndex = -1;// set that we can't add targets to the map
    }


    public void ToggleCustomCoords()
    {
        displayCustomCoords = !displayCustomCoords;
        CustomCoordsPanel.SetActive(displayCustomCoords);
    }


    private void Update()
    {
        if (compassRing.activeInHierarchy != showCompassRing)
            compassRing.SetActive(showCompassRing);

        distanceFilterText.text = distanceFilter.ToString() + "km";

    }
}