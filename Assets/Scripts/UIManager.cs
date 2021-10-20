using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject compassRing;
    public GameObject baseUI;

    public GameObject map;

    public Camera arCamera;

    public GameObject ShowTargets;
    public GameObject HideTargets;

    public GameObject Targets;

    public GameObject selectedTargetPanel;

    public Sprite arImage;
    public Sprite mapImage;

    public Image mapButtonImage;
    public bool showCompassRing = true;
    public bool showTargetRing = true;
    public bool displayGeometry = false;
    public bool displayDistanceFilter = false;
    public bool displayMap = true;

    public bool displayBox = true;
    public bool displayInActive = false;
    public bool alwaysActive = true;


    public float distanceFilter = 0.5f;

    public UnityEvent onMinimalUIActive = new UnityEvent();
    public UnityEvent onMinimalUIDeActive = new UnityEvent();

    public int audioInterval = 10;

    private Quaternion arCameraInitialRotation;

    private void Start()
    {
        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        map.SetActive(false);
        arCameraInitialRotation = arCamera.transform.localRotation;
        Targets.SetActive(false);
        HideTargets.SetActive(false);
        selectedTargetPanel.SetActive(false);

        // start SONUS with map screen first
        map.SetActive(true);
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



    public void ToggleMap()
    {
        displayMap = !displayMap;
        map.SetActive(displayMap);
        ShowTargets.SetActive(displayMap);

        mapButtonImage.sprite = displayMap ? arImage : mapImage;
    }


    public void ShowAddTargetPanel()
    {
        ShowTargets.SetActive(false);
        HideTargets.SetActive(true);
        Targets.SetActive(true);
        // since we have decided to open the target panel and want to add targets, best we zoom to a reasonable level
        OnlineMaps.instance.SetPositionAndZoom(ARVINO_GPS.Instance._UserLon, ARVINO_GPS.Instance._UserLat, 18);

    }

    public void HideAddTargetPanel()
    {
        ShowTargets.SetActive(true);
        HideTargets.SetActive(false);
        Targets.SetActive(false);

    }




    private void Update()
    {
        if (compassRing.activeInHierarchy != showCompassRing)
            compassRing.SetActive(showCompassRing);



    }
}