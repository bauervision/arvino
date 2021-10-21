using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject baseUI;
    public GameObject map;
    public Camera arCamera;

    public Sprite arImage;
    public Sprite mapImage;

    public Image mapButtonImage;


    public bool showCompassRing = true;
    public bool showTargetRing = true;


    public bool displayMap = true;

    public bool displayBox = true;


    public UnityEvent onMinimalUIActive = new UnityEvent();
    public UnityEvent onMinimalUIDeActive = new UnityEvent();



    private Quaternion arCameraInitialRotation;

    private void Start()
    {
        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        map.SetActive(false);
        arCameraInitialRotation = arCamera.transform.localRotation;
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


    public void ToggleMap()
    {
        displayMap = !displayMap;
        map.SetActive(displayMap);
        mapButtonImage.sprite = displayMap ? arImage : mapImage;
    }



    private void Update()
    {



    }
}