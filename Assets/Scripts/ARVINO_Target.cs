using UnityEngine;
using UnityEngine.UI;

using System;

using System.Collections;

public class ARVINO_Target : MonoBehaviour
{
    public static ARVINO_Target instance;

    #region Public Class members

    public double _Lat;
    public double _Lon;

    public string _ID;
    public float _Dir;
    public float _Dis;
    public Vector3 _Heading = new Vector3();
    public Vector3 _Rotation = new Vector3();


    #endregion

    #region UI related members
    public Transform parentCanvas = null;
    public GameObject targetRing = null;


    [Header("Target Coordinates")]
    public GameObject myTracker = null;// bounding 


    private Image _trackerImage;
    private GameObject myIndicator = null;// target ring indicator

    public Camera mainCam = null;
    private Text distanceText;

    #endregion

    private float distanceToCamera = 0.0f;

    private RectTransform _rect;


    ///<summary>When the file initial loads, find and create all the key required assets </summary>
    private void Awake()
    {
        targetRing = GameObject.Find("TargetRing");
        if (targetRing != null)
            myIndicator = Instantiate(Resources.Load<GameObject>("Indicator"), targetRing.transform);

        parentCanvas = GameObject.Find("UI_Canvas").GetComponent<Transform>();

        if (parentCanvas != null)
        {
            myTracker = Instantiate(Resources.Load<GameObject>("ModelTracker"), parentCanvas);
            _trackerImage = myTracker.GetComponent<Image>();
            distanceText = myTracker.transform.GetChild(1).gameObject.GetComponent<Text>();
            _rect = myTracker.GetComponent<RectTransform>();

        }

        mainCam = Camera.main;
    }


    ///<summary>App has offically loaded, set the main camera </summary>
    private void Start()
    {

        instance = this;
        transform.position = Vector3.zero;
    }

    ///<summary>Main update loop, determine whether we display the child mesh geometry for rendering, and position updates </summary>
    private void Update()
    {
        HandlePosUpdate();
    }



    ///<summary>Determine any position updates by interpolating where we are, with where we need to be</summary>
    private void HandlePosUpdate()
    {
        // update this target position

        _Rotation.x = 0;
        _Rotation.y = _Dir;
        _Rotation.z = 0;
        transform.localEulerAngles = _Rotation;
        HandleScreenPos();
    }





    ///<summary>Handle Target Indicator updates, and determine if this target is actually on the screen, if so, turn on the bounding box, otherwise we hide it</summary>
    private void HandleScreenPos()
    {
        // // always process the target ring indicator
        if (myIndicator != null)
            HandleTargetRingIndicatorUpdate();

        // get the heading of this target in relation to the camera
        _Heading.x = 0;
        _Heading.y = _Dir;
        _Heading.z = 0;


        // if this target is in view of the camera
        if (Vector3.Dot(mainCam.transform.forward, _Heading) > 0)
        {
            myTracker.SetActive(true);
            // find where this target is located in the camera view
            Vector3 trackerPos = mainCam.WorldToScreenPoint(this.transform.position);
            // update the bounding box to that screen position
            myTracker.transform.position = trackerPos;
            // figure out the distance from the camera
            HandleDistanceToCamera();
        }
        else
            myTracker.SetActive(false);


    }

    public double KilometerToMeter(double km)
    {
        double METER = 0;

        METER = km * 1000;

        return METER;
    }


    ///<summary>As long as the user wants to see the target ring, handle all calculations</summary>
    private void HandleTargetRingIndicatorUpdate()
    {
        // make sure we haven't lost myIndicator for whatever reason
        if (myIndicator == null)
            return;

        // round the compass heading so we don't get as many updates, makes for a smoother experience
        float compassHeading = (!Application.isEditor) ? Mathf.Round(Input.compass.trueHeading) : ARVINO_GPS.compassAngle;

        // if we are running on the device, use the actual compass
        float finalAngle = !Application.isEditor ? _Dir - compassHeading : _Dir - ARVINO_GPS.compassAngle;

        // make the update
        myIndicator.transform.localRotation = Quaternion.Slerp(myIndicator.transform.localRotation, Quaternion.Euler(0, 0, finalAngle), Time.deltaTime * 2f);
    }



    ///<summary>When this target is visible on screen, process the distance for rendering on the text field</summary>
    private void HandleDistanceToCamera()
    {
        distanceToCamera = _Dis;
        // if 1 kilometer or greater, use "km"
        if (distanceToCamera >= 1.0f)
            distanceText.text = (UIManager.instance.displayBox) ? distanceToCamera.ToString() + "km" : "";
        else // less than 1 kilometer, so switch to meters
            distanceText.text = (UIManager.instance.displayBox) ? ((float)System.Math.Round(KilometerToMeter(distanceToCamera), 2)).ToString() + "m" : "";

    }


    ///<summary>When this target data is loaded, set its data, bounding box and target ring indicator</summary>
    public void SetInitialPosition(ARVINOActor actor)
    {
        _ID = actor._ID;
        _Lat = actor._Lat;
        _Lon = actor._Lon;
        _Dir = actor._Direction;
        _Dis = actor._Distance;

        _Rotation.x = 0;
        _Rotation.y = _Dir;
        _Rotation.z = 0;
        transform.localEulerAngles = _Rotation;

        print("Target Heading: " + _Dir);
        print("Target Distance: " + _Dis);
        print("transform: x " + transform.rotation.x);
        print("transform: y " + transform.rotation.y);
        print("transform: z " + transform.rotation.z);

        HandleTargetRingIndicatorUpdate();
    }


    ///<summary>Called from TargetManager: will handle the removal of this target's gameobject, bounding box tracker, and target ring indicator </summary>
    public void RemoveTarget()
    {
        DestroyImmediate(myTracker);
        DestroyImmediate(myIndicator);
        DestroyImmediate(gameObject);
    }

}
