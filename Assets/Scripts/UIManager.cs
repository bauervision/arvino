using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;


    public GameObject map;
    public Camera arCamera;
    public GameObject selectedTargetPanel;
    public GameObject statusWindow;
    public GameObject northPole;
    public GameObject statsPanel;

    public Sprite arImage;
    public Sprite mapImage;

    public Image mapButtonImage;


    public bool showCompassRing = true;
    public bool showTargetRing = true;


    public bool displayMap = true;

    public bool displayBox = true;


    private Quaternion arCameraInitialRotation;

    private void Start()
    {
        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        map.SetActive(false);
        arCameraInitialRotation = arCamera.transform.localRotation;
        selectedTargetPanel.SetActive(false);
        statusWindow.SetActive(false);
        statsPanel.SetActive(false);
    }


    public void ToggleStatsPanel()
    {
        statsPanel.SetActive(!statsPanel.activeInHierarchy);
    }




    public void PointerOverUI()
    {
        print("Pointer over UI");
    }


    public void ToggleCompassRing()
    {
        showCompassRing = !showCompassRing;
    }

    public void ToggleTargetRing()
    {
        showTargetRing = !showTargetRing;
    }


    public void ToggleMap()
    {
        displayMap = !displayMap;
        map.SetActive(displayMap);
        mapButtonImage.sprite = displayMap ? arImage : mapImage;
        if (!map.activeInHierarchy)
        {
            selectedTargetPanel.SetActive(false);
            showTargetRing = true;
        }
        else
            showTargetRing = false;
    }


    public void ToggleStatusWindow()
    {
        statusWindow.SetActive(!statusWindow.activeInHierarchy);
    }

    public void ToggleNorthPole()
    {
        northPole.SetActive(!northPole.activeInHierarchy);
    }

    private void Update()
    {



    }
}